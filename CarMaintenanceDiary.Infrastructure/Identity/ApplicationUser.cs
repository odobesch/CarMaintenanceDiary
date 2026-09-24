using Microsoft.AspNetCore.Identity;

namespace CarMaintenanceDiary.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public string? ProfilePictureContentType { get; set; }
    }
}
