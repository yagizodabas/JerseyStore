using System.ComponentModel.DataAnnotations;

namespace JerseyStore.Models
{
    public class OrderRequest
    {
        [Required(ErrorMessage = "Please enter the customer name.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Delivery address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string Phone { get; set; }

        public List<OrderItem> Items { get; set; }
    }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public int? JerseyNumber { get; set; } 
        public string? PrintName { get; set; }
    }
}