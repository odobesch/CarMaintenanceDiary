using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CarMaintenanceDiary.Application.Services;

public class UserManagementApiService : IUserManagementService
{
    private readonly HttpClient _http;

    public UserManagementApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<UserDto>>("api/usermanagement") ?? new();
    }

    public async Task<List<string>> GetRolesAsync()
    {
        return await _http.GetFromJsonAsync<List<string>>("api/usermanagement/roles") ?? new();
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/usermanagement", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/usermanagement/{userId}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(string userId)
    {
        var response = await _http.DeleteAsync($"api/usermanagement/{userId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ActivateUserAsync(string userId)
    {
        var response = await _http.PostAsync($"api/usermanagement/{userId}/activate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeactivateUserAsync(string userId)
    {
        var response = await _http.PostAsync($"api/usermanagement/{userId}/deactivate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/usermanagement/{userId}/change-password", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task ResetPasswordAsync(string userId, ResetPasswordRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/usermanagement/{userId}/reset-password", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string?> UploadProfilePictureAsync(string userId, Stream fileStream, string fileName, string contentType)
    {
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
        var response = await _http.DeleteAsync($"api/usermanagement/{userId}/profile-picture");
        response.EnsureSuccessStatusCode();
    }

    private class ProfilePictureUploadResult
    {
        public string? Url { get; set; }
    }
}
