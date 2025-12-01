using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using static CarMaintenanceDiary.Infrastructure.Email.SmtpEmailSender;

namespace CarMaintenanceDiary.Infrastructure.Email
{
    public sealed class MailjetOptionsValidator : IValidateOptions<MailjetOptions>
    {
        public ValidateOptionsResult Validate(string? name, MailjetOptions options)
        {
            var errors = new List<string>();
            if (!string.IsNullOrWhiteSpace(options.ApiKey) && string.IsNullOrWhiteSpace(options.ApiSecret))
                errors.Add("Mailjet ApiSecret is required when ApiKey is set.");
            if (!string.IsNullOrWhiteSpace(options.ApiSecret) && string.IsNullOrWhiteSpace(options.ApiKey))
                errors.Add("Mailjet ApiKey is required when ApiSecret is set.");
            return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
        }
    }

    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; } = false;
        public string? UserName
        {
            get; set;
        }
        public string? Password
        {
            get; set;
        }
        public string From { get; set; } = "no-reply@localhost";
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("email is required", nameof(email));

            using var mail = new MailMessage();
            mail.From = new MailAddress(_options.From);
            mail.To.Add(new MailAddress(email));
            mail.Subject = subject;
            mail.Body = htmlMessage;
            mail.IsBodyHtml = true;

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl
            };

            if (!string.IsNullOrEmpty(_options.UserName))
                client.Credentials = new NetworkCredential(_options.UserName, _options.Password ?? string.Empty);

            try
            {
                _logger.LogDebug("Sending email to {Email} via {Host}:{Port}", email, _options.Host, _options.Port);
                await client.SendMailAsync(mail);
                _logger.LogInformation("Email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw;
            }
        }

        public sealed class MailjetOptions
        {
            public string ApiKey { get; set; } = string.Empty;
            public string ApiSecret { get; set; } = string.Empty;
            public string FromEmail { get; set; } = "no-reply@localhost";
            public string FromName { get; set; } = "Car Maintenance Diary";
        }

        public sealed class MailjetEmailSender : IEmailSender
        {
            private readonly MailjetOptions _options;
            private readonly ILogger<MailjetEmailSender> _logger;
            private readonly IHttpClientFactory _httpClientFactory;

            public MailjetEmailSender(
                IOptions<MailjetOptions> options,
                ILogger<MailjetEmailSender> logger,
                IHttpClientFactory httpClientFactory)
            {
                _options = options.Value;
                _logger = logger;
                _httpClientFactory = httpClientFactory;
            }

            public async Task SendEmailAsync(string email, string subject, string htmlMessage)
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Recipient email required.", nameof(email));
                if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.ApiSecret))
                    throw new InvalidOperationException("Mailjet API credentials are not configured.");

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.mailjet.com/");
                var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ApiKey}:{_options.ApiSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);

                var payload = new
                {
                    Messages = new[]
                    {
                    new
                    {
                        From = new { Email = _options.FromEmail, Name = _options.FromName },
                        To = new[] { new { Email = email } },
                        Subject = subject,
                        HTMLPart = htmlMessage
                    }
                }
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("v3.1/send", content);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Mailjet email sent to {Email}", email);
                        return;
                    }

                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Mailjet send failed ({Status}): {Body}", response.StatusCode, body);
                    throw new InvalidOperationException($"Mailjet send failed: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Mailjet send exception for {Email}", email);
                    throw;
                }
            }
        }

        public sealed class MailtrapOptions
        {
            // Mailtrap Email Testing (SMTP sandbox) uses SMTP credentials -> handled by SmtpEmailSender
            // Mailtrap Sending (Production) uses an API token for real delivery
            public string ApiToken { get; set; } = string.Empty;          // Mailtrap Sending API token
            public string FromEmail { get; set; } = "no-reply@localhost"; // Must be a verified/allowed address/domain in Mailtrap Sending
            public string FromName { get; set; } = "Car Maintenance Diary";
        }

        public sealed class MailtrapEmailSender : IEmailSender
        {
            private readonly MailtrapOptions _options;
            private readonly ILogger<MailtrapEmailSender> _logger;
            private readonly IHttpClientFactory _httpClientFactory;

            public MailtrapEmailSender(
                IOptions<MailtrapOptions> options,
                ILogger<MailtrapEmailSender> logger,
                IHttpClientFactory httpClientFactory)
            {
                _options = options.Value;
                _logger = logger;
                _httpClientFactory = httpClientFactory;
            }

            public async Task SendEmailAsync(string email, string subject, string htmlMessage)
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Recipient email required.", nameof(email));
                if (string.IsNullOrWhiteSpace(_options.ApiToken))
                    throw new InvalidOperationException("Mailtrap ApiToken not configured (use SMTP sandbox or set ApiToken).");

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://send.api.mailtrap.io/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _options.ApiToken);

                // Mailtrap Sending API v1 body
                var payload = new
                {
                    from = new
                    {
                        email = _options.FromEmail,
                        name = _options.FromName
                    },
                    to = new[] { new { email } },
                    subject,
                    html = htmlMessage
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("api/send", content);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Mailtrap email sent to {Email}", email);
                        return;
                    }

                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Mailtrap send failed ({Status}): {Body}", response.StatusCode, body);
                    throw new InvalidOperationException($"Mailtrap send failed: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Mailtrap send exception for {Email}", email);
                    throw;
                }
            }
        }
    }
}
