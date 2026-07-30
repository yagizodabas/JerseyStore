namespace JerseyStore.Models
{

    public class OrderRequest
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        // Siparişin içindeki ürünlerin listesi
        public List<OrderItem> Items { get; set; }
    }


    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}