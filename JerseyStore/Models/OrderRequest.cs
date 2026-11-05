using System.ComponentModel.DataAnnotations;

namespace JerseyStore.Models
{
    public class OrderRequest
    {
        [Required(ErrorMessage = "Please enter the customer name.")]
        [StringLength(120)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery address is required.")]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [Required, MinLength(1), MaxLength(100)]
        public List<OrderItem> Items { get; set; } = [];
    }

    public class OrderItem
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Range(0, 99)]
        public int? JerseyNumber { get; set; } 
        [StringLength(40)]
        public string? PrintName { get; set; }
    }
}
