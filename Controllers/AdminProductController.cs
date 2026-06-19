using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Dtos.Admin;
using PetCareSystem.API.Models;

namespace PetCareSystem.API.Controllers
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminProductController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public AdminProductController(PetCareSystemContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductManagementDto>>> GetProducts()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = products.Select(MapToProductManagementDto).ToList();
            return Ok(result);
        }

        [HttpGet("{productId:long}")]
        public async Task<ActionResult<ProductManagementDto>> GetProduct(long productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            return Ok(MapToProductManagementDto(product));
        }

        [HttpPost]
        public async Task<ActionResult<ProductManagementDto>> CreateProduct(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Category = dto.Category.HasValue ? (int)dto.Category.Value : null,
                Brand = dto.Brand,
                Weight = dto.Weight,
                ThumbnailUrl = dto.ThumbnailUrl,
                IsActive = dto.IsActive ?? true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { productId = product.ProductId }, MapToProductManagementDto(product));
        }

        [HttpPut("{productId:long}")]
        public async Task<IActionResult> UpdateProduct(long productId, UpdateProductDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                product.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                product.Description = dto.Description;
            }

            if (dto.Price.HasValue)
            {
                product.Price = dto.Price.Value;
            }

            if (dto.StockQuantity.HasValue)
            {
                product.StockQuantity = dto.StockQuantity.Value;
            }

            if (dto.Category.HasValue)
            {
                product.Category = (int)dto.Category.Value;
            }

            if (dto.Brand != null)
            {
                product.Brand = dto.Brand;
            }

            if (dto.Weight.HasValue)
            {
                product.Weight = dto.Weight.Value;
            }

            if (dto.ThumbnailUrl != null)
            {
                product.ThumbnailUrl = dto.ThumbnailUrl;
            }

            if (dto.IsActive.HasValue)
            {
                product.IsActive = dto.IsActive.Value;
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Product updated successfully");
        }

        [HttpDelete("{productId:long}")]
        public async Task<IActionResult> DeleteProduct(long productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            var hasReferences = await _context.OrderItems.AnyAsync(oi => oi.ProductId == productId) ||
                                await _context.Feedbacks.AnyAsync(f => f.ProductId == productId);

            if (hasReferences)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return Ok("Product has associated orders or feedback and has been marked as inactive.");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully");
        }

        private static ProductManagementDto MapToProductManagementDto(Product product)
        {
            return new ProductManagementDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category.HasValue ? (Enums.ProductCategory)product.Category.Value : null,
                Brand = product.Brand,
                Weight = product.Weight,
                ThumbnailUrl = product.ThumbnailUrl,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
