namespace OrderService.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public Guid OrderID { get; set; }
        public Guid ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
