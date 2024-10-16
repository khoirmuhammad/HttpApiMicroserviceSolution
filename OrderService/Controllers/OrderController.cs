using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Dto;
using System.Net.Http.Headers;
using System.Net.Http;

namespace OrderService.Controllers
{
    [Route("api/order")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderController(OrderDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Step 1: Check stock availability in Product Service
            var stockCheckResponse = await httpClient.PostAsJsonAsync("https://localhost:7272/api/internal/product/check-stock", request.Products);
            var stockAvailability = await stockCheckResponse.Content.ReadFromJsonAsync<List<ProductAvailability>>();

            if (stockAvailability.Any(p => !p.Available))
            {
                return BadRequest("Some products are out of stock.");
            }

            // Step 2: Place the order
            var order = new Models.Order
            {
                Customer = request.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = request.Products.Sum(p => p.Quantity * p.Price)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Store order items temporarily
            var orderItems = new List<Models.OrderItem>();
            foreach (var product in request.Products)
            {
                var orderItem = new Models.OrderItem
                {
                    OrderID = order.Id,
                    ProductID = product.ProductID,
                    Quantity = product.Quantity,
                    Price = product.Price
                };
                orderItems.Add(orderItem);
            }

            // Step 3: Reduce stock in Product Service
            var stockUpdateList = request.Products.Select(p => new ProductStockUpdate
            {
                ProductID = p.ProductID,
                Quantity = p.Quantity
            }).ToList();

            var stockUpdateResponse = await httpClient.PostAsJsonAsync(
                "https://localhost:7272/api/internal/product/update-stock", stockUpdateList);

            if (!stockUpdateResponse.IsSuccessStatusCode)
            {
                // Compensation: Delete the order and its items
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                return StatusCode(500, "Failed to update stock. Order rolled back.");
            }

            // Add order items to the database if stock update is successful
            _context.OrderItems.AddRange(orderItems);
            await _context.SaveChangesAsync();

            return Ok("Order placed and stock updated successfully.");
        }

    }
}
