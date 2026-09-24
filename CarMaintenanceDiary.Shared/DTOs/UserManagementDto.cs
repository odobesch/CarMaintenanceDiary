using System;
using System.Collections.Generic;
using System.Text;

namespace CarMaintenanceDiary.Shared.DTOs
{
    public record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string? UserName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    IList<string> Roles,
    string? ProfilePictureUrl
);

    public record CreateUserRequest(
        string FirstName,
        string LastName,
        string UserName,
        string Email,
        string Password,
        string? PhoneNumber,
        IList<string> Roles
    );

    public record UpdateUserRequest(
        string Id,
        string FirstName,
        string LastName,
        string Email,
        string? UserName,
        string? PhoneNumber,
        IList<string> Roles
    );

    public record ChangePasswordRequest(
        string UserId,
        string CurrentPassword,
        string NewPassword
    );

    public record ResetPasswordRequest(
        string UserId,
        string NewPassword
    );

    public record UploadProfilePictureRequest(
        string UserId,
        string FileName,
        string ContentType,
        byte[] Data
    );
}
