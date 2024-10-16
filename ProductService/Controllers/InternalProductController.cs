using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Dto;

namespace ProductService.Controllers
{
    [Route("api/internal/product")]
    [ApiController]
    [Authorize]
    public class InternalProductController : ControllerBase
    {
        private readonly ProductDbContext _context;
        public InternalProductController(ProductDbContext context)
        {
            _context = context;
        }

        [HttpPost("check-stock")]
        public async Task<IActionResult> CheckStock([FromBody] List<ProductRequest> products)
        {
            var result = new List<ProductAvailability>();

            foreach (var product in products)
            {
                var dbProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == product.ProductID);

                if (dbProduct == null || dbProduct.Qty < product.Quantity)
                {
                    result.Add(new ProductAvailability { ProductID = product.ProductID, Available = false });
                }
                else
                {
                    result.Add(new ProductAvailability { ProductID = product.ProductID, Available = true });
                }
            }

            return Ok(result);
        }

        // New endpoint to update stock
        [HttpPost("update-stock")]
        public async Task<IActionResult> UpdateStock([FromBody] List<ProductStockUpdate> updates)
        {
            foreach (var update in updates)
            {
                var dbProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == update.ProductID);

                if (dbProduct != null && dbProduct.Qty >= update.Quantity)
                {
                    dbProduct.Qty -= update.Quantity;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok("Stock updated successfully.");
        }
    }
}
