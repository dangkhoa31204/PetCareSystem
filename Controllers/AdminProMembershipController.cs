using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareSystem.API.Dtos.Admin;
using PetCareSystem.API.Enums;

namespace PetCareSystem.API.Controllers
{
    [Route("api/admin/pro-membership")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminProMembershipController : ControllerBase
    {
        [HttpPut("packages/{packageType:int}/price")]
        public IActionResult UpdateProPackagePrice(int packageType, UpdateProPackagePriceDto dto)
        {
            if (!Enum.IsDefined(typeof(ProPackageType), packageType))
            {
                return BadRequest("Invalid package type");
            }

            var success = ProMembershipController.UpdatePackagePricing(packageType, dto.Price, dto.ServiceDiscount, dto.ProductDiscount);
            if (!success)
            {
                return BadRequest("Package not found");
            }

            return Ok("Pro package pricing updated successfully");
        }
    }
}
