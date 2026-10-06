using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentMarketplaceSystem.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        public string BuyerId { get; set; } = string.Empty;

        [ForeignKey("BuyerId")]
        public ApplicationUser? Buyer { get; set; }

        [Required]
        public string SellerId { get; set; } = string.Empty;

        [ForeignKey("SellerId")]
        public ApplicationUser? Seller { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}