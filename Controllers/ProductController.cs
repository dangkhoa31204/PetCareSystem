using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareSystem.API.Models;
using PetCareSystem.API.Dtos.Customer;
using PetCareSystem.API.Enums;
using System.Security.Claims;

namespace PetCareSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public ProductController(PetCareSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [Public] Lấy danh sách tất cả các sản phẩm đang hoạt động
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductDto>>> GetAllProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsActive == true)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Category = p.Category,
                    Brand = p.Brand,
                    Weight = p.Weight,
                    ThumbnailUrl = p.ThumbnailUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// [Public] Lấy chi tiết một sản phẩm bằng ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDto>> GetProduct(long id)
        {
            var product = await _context.Products
                .Where(p => p.ProductId == id && p.IsActive == true)
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound("Product not found");

            var productDto = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                Brand = product.Brand,
                Weight = product.Weight,
                ThumbnailUrl = product.ThumbnailUrl
            };

            return Ok(productDto);
        }

        /// <summary>
        /// [Public] Lấy danh sách sản phẩm theo danh mục (Category)
        /// </summary>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductDto>>> GetProductsByCategory(int category)
        {
            var products = await _context.Products
                .Where(p => p.Category == category && p.IsActive == true)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Category = p.Category,
                    Brand = p.Brand,
                    Weight = p.Weight,
                    ThumbnailUrl = p.ThumbnailUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// [Public] Lấy danh sách sản phẩm theo thương hiệu (Brand)
        /// </summary>
        [HttpGet("brand/{brand}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductDto>>> GetProductsByBrand(string brand)
        {
            var products = await _context.Products
                .Where(p => p.Brand == brand && p.IsActive == true)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Category = p.Category,
                    Brand = p.Brand,
                    Weight = p.Weight,
                    ThumbnailUrl = p.ThumbnailUrl
                })
                .ToListAsync();

            return Ok(products);
        }
    }
}
