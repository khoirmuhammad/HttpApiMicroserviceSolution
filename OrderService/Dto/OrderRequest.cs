namespace OrderService.Dto
{
    public class OrderRequest
    {
        public string CustomerID { get; set; }
        public List<ProductRequest> Products { get; set; }
    }
}
