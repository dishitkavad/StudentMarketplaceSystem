using System.ComponentModel.DataAnnotations;

namespace StudentMarketplaceSystem.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Branch { get; set; } = string.Empty;

        [Required]
        [Range(1, 4)]
        public int Year { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;
    }
}