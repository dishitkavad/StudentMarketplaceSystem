using Microsoft.AspNetCore.Identity;

namespace StudentMarketplaceSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public int Year { get; set; }
    }
}