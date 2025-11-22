using Microsoft.AspNetCore.Identity;

namespace CarMaintenanceDiary.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {        
        public byte[]? ProfilePicture { get; set; }
        public string? ProfilePictureContentType { get; set; }
    }
}
