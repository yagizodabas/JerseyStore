namespace JerseyStore.Models
{
    // Müşteriden POST isteğiyle gelecek ana sipariş paketi
    public class OrderRequest
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        // Siparişin içindeki ürünlerin listesi
        public List<OrderItem> Items { get; set; }
    }

    // Siparişin içindeki her bir ürün kalemini temsil eden yapı
    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}