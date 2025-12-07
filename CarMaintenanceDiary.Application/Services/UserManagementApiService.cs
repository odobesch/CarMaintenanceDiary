using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CarMaintenanceDiary.Application.Services;

public class UserManagementApiService : IUserManagementService
{
    private readonly HttpClient _http;
    private readonly IAuthTokenAccessor _tokenAccessor;

    public UserManagementApiService(HttpClient http, IAuthTokenAccessor tokenAccessor)
    {
        _http = http;
        _tokenAccessor = tokenAccessor;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _tokenAccessor.GetTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            Console.WriteLine("[VehicleApiService] WARNING: Token is null or empty, NOT setting auth header!");
        }
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<UserDto>>("api/usermanagement") ?? new();
    }

    public async Task<List<string>> GetRolesAsync()
    {
        await SetAuthHeaderAsync();
        return await _http.GetFromJsonAsync<List<string>>("api/usermanagement/roles") ?? new();
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/usermanagement", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/usermanagement/{userId}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(string userId)
    {
        await SetAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/usermanagement/{userId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateUserAsync(string userId)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsync($"api/usermanagement/{userId}/activate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateUserAsync(string userId)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsync($"api/usermanagement/{userId}/deactivate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync($"api/usermanagement/{userId}/change-password", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task ResetPasswordAsync(string userId, ResetPasswordRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync($"api/usermanagement/{userId}/reset-password", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string?> UploadProfilePictureAsync(string userId, Stream fileStream, string fileName, string contentType)
    {
        await SetAuthHeaderAsync();
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var response = await _http.PostAsync($"api/usermanagement/{userId}/profile-picture", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProfilePictureUploadResult>();
        return result?.Url;
    }

    public async Task DeleteProfilePictureAsync(string userId)
    {
        await SetAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/usermanagement/{userId}/profile-picture");
        response.EnsureSuccessStatusCode();
    }

    private class ProfilePictureUploadResult
    {
        public string? Url { get; set; }
    }
}
