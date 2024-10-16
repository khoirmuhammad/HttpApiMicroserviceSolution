namespace OrderService.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string Customer { get; set; } =string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
