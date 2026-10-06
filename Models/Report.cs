using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentMarketplaceSystem.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        // User who submitted the report
        [Required]
        public string ReporterId { get; set; } = string.Empty;

        [ForeignKey("ReporterId")]
        public ApplicationUser? Reporter { get; set; }

        // Product being reported (optional)
        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // User being reported (optional)
        public string? ReportedUserId { get; set; }

        [ForeignKey("ReportedUserId")]
        public ApplicationUser? ReportedUser { get; set; }

        // Reason for the report
        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        // Additional explanation
        [StringLength(1000)]
        public string? Description { get; set; }

        // Pending / Resolved
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}