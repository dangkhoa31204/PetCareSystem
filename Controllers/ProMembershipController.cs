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
    public class ProMembershipController : ControllerBase
    {
        private readonly PetCareSystemContext _context;

        public ProMembershipController(PetCareSystemContext context)
        {
            _context = context;
        }

        // Define pro packages
        private static readonly Dictionary<int, (string Name, decimal Price, decimal ServiceDiscount, decimal ProductDiscount, List<string> Benefits, List<int> AvailableServices)> ProPackages = new()
        {
            { 
                (int)ProPackageType.Free, 
                (
                    "Free Package",
                    0m,
                    0m,  // No discount
                    0m,  // No discount
                    new List<string>
                    {
                        "Access to all services (pay full price)",
                        "Standard customer support",
                        "View booking history"
                    },
                    new List<int> { 1, 2, 3 }  // All services available
                )
            },
            { 
                (int)ProPackageType.Pro, 
                (
                    "Pro Package",
                    99000m,
                    0.15m,  // 15% discount on services
                    0.20m,  // 20% discount on products
                    new List<string>
                    {
                        "15% discount on all services",
                        "20% discount on all products",
                        "Priority booking slots",
                        "Dedicated customer support",
                        "Free health consultation (1/month)",
                        "Free pet grooming (1/month)"
                    },
                    new List<int> { 1, 2, 3 }  // All services available
                )
            }
        };

        public static bool UpdatePackagePricing(int packageType, decimal price, decimal? serviceDiscount, decimal? productDiscount)
        {
            if (!ProPackages.TryGetValue(packageType, out var package))
            {
                return false;
            }

            var updatedServiceDiscount = serviceDiscount ?? package.ServiceDiscount;
            var updatedProductDiscount = productDiscount ?? package.ProductDiscount;

            ProPackages[packageType] = (
                package.Name,
                price,
                updatedServiceDiscount,
                updatedProductDiscount,
                package.Benefits,
                package.AvailableServices);

            return true;
        }

        // GET: api/promembership/packages - Get all pro packages
        [HttpGet("packages")]
        [AllowAnonymous]
        public ActionResult<List<ProPackageDto>> GetProPackages()
        {
            var packages = ProPackages.Select(p => new ProPackageDto
            {
                PackageType = p.Key,
                PackageName = p.Value.Name,
                Price = p.Value.Price,
                Description = GetPackageDescription(p.Key),
                Benefits = p.Value.Benefits,
                AvailableServiceCategories = p.Value.AvailableServices
            }).ToList();

            return Ok(packages);
        }

        // GET: api/promembership/my-subscription - Get current user's subscription status
        [HttpGet("my-subscription")]
        public async Task<ActionResult<ProSubscriptionResponseDto>> GetMySubscription()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found");

            var isProActive = user.IsProMember == true;
            var packageType = isProActive ? (int)ProPackageType.Pro : (int)ProPackageType.Free;
            var packageData = ProPackages[packageType];

            return Ok(new ProSubscriptionResponseDto
            {
                PackageType = packageType,
                PackageName = packageData.Name,
                IsActive = isProActive,
                ServiceDiscount = packageData.ServiceDiscount * 100,  // Convert to percentage
                ProductDiscount = packageData.ProductDiscount * 100,  // Convert to percentage
                Benefits = packageData.Benefits,
                AvailableServiceCategories = packageData.AvailableServices
            });
        }

        // GET: api/promembership/benefits - Get pro benefits
        [HttpGet("benefits")]
        [AllowAnonymous]
        public ActionResult<ProMemberBenefitsDto> GetBenefits()
        {
            var userId = GetUserIdFromClaims();

            if (userId == 0)
            {
                var freePackage = ProPackages[(int)ProPackageType.Free];
                return Ok(new ProMemberBenefitsDto
                {
                    PackageType = (int)ProPackageType.Free,
                    PackageName = freePackage.Name,
                    IsActive = true,
                    ServiceDiscount = freePackage.ServiceDiscount * 100,
                    ProductDiscount = freePackage.ProductDiscount * 100,
                    Benefits = freePackage.Benefits
                });
            }

            var user = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.UserId == userId);

            var isProActive = user?.IsProMember == true;
            var activePackageType = isProActive ? (int)ProPackageType.Pro : (int)ProPackageType.Free;
            var packageData = ProPackages[activePackageType];

            return Ok(new ProMemberBenefitsDto
            {
                PackageType = activePackageType,
                PackageName = packageData.Name,
                IsActive = isProActive,
                ServiceDiscount = packageData.ServiceDiscount * 100,
                ProductDiscount = packageData.ProductDiscount * 100,
                Benefits = packageData.Benefits
            });
        }

        // POST: api/promembership/subscribe - Subscribe to pro package
        [HttpPost("subscribe")]
        public async Task<ActionResult<ProSubscriptionResponseDto>> SubscribeToPro(SubscribeProPackageDto subscribeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            if (!ProPackages.ContainsKey(subscribeDto.PackageType))
                return BadRequest("Invalid package type");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found");

            if (subscribeDto.PackageType == (int)ProPackageType.Free)
            {
                // Downgrade to free
                user.IsProMember = false;
                user.ProExpiredAt = null;
            }
            else if (subscribeDto.PackageType == (int)ProPackageType.Pro)
            {
                // Upgrade to pro
                user.IsProMember = true;
                user.ProExpiredAt = DateTime.UtcNow.AddDays(30);  // Pro for 1 month by default
            }

            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var packageData = ProPackages[subscribeDto.PackageType];

            return Ok(new ProSubscriptionResponseDto
            {
                PackageType = subscribeDto.PackageType,
                PackageName = packageData.Name,
                IsActive = subscribeDto.PackageType == (int)ProPackageType.Pro,
                ServiceDiscount = packageData.ServiceDiscount * 100,
                ProductDiscount = packageData.ProductDiscount * 100,
                Benefits = packageData.Benefits,
                AvailableServiceCategories = packageData.AvailableServices
            });
        }

        // POST: api/promembership/cancel - Cancel pro subscription (downgrade to free)
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelPro()
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0) return Unauthorized();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found");

            if (user.IsProMember != true)
                return BadRequest("User is already on free package");

            user.IsProMember = false;
            user.ProExpiredAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Downgraded to free package successfully");
        }

        // Helper methods
        private string GetPackageDescription(int packageType)
        {
            return packageType switch
            {
                (int)ProPackageType.Free => "Perfect for casual pet owners",
                (int)ProPackageType.Pro => "Best for pet lovers who want special treatment and discounts",
                _ => ""
            };
        }

        private long GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
                return userId;
            return 0;
        }
    }
}
