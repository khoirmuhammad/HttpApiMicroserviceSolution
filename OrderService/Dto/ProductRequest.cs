namespace OrderService.Dto
{
    public class ProductRequest
    {
        public Guid ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
