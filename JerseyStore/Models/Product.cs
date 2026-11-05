namespace JerseyStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
