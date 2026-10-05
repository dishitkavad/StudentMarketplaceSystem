using System.ComponentModel.DataAnnotations;

namespace StudentMarketplaceSystem.ViewModels
{
    public class ProductViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Product Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000000)]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Condition")]
        public string Condition { get; set; } = string.Empty;
    }
}