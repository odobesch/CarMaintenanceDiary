using CarMaintenanceDiary.Shared.DTOs;

namespace CarMaintenanceDiary.Application.Interfaces;

public interface IUserManagementService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<List<string>> GetRolesAsync();
    Task CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(string userId, UpdateUserRequest request);
    Task DeleteUserAsync(string userId);
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task ResetPasswordAsync(string userId, ResetPasswordRequest request);
    Task<string?> UploadProfilePictureAsync(string userId, Stream fileStream, string fileName, string contentType);
    Task DeleteProfilePictureAsync(string userId);
}
