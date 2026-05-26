using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareSystem.API.Dtos.Common;
using PetCareSystem.API.Services.Cloudinary;

namespace PetCareSystem.API.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public UploadController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("image")]
        [RequestSizeLimit(10_000_000)]
        public async Task<ActionResult<UploadImageResponseDto>> UploadImage([FromForm] IFormFile file, [FromForm] string folder = "general")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required");
            }

            var (url, publicId) = await _cloudinaryService.UploadImageAsync(file, folder);
            return Ok(new UploadImageResponseDto { Url = url, PublicId = publicId });
        }
    }
}
