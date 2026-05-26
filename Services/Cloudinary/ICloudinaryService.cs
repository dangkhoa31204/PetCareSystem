using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PetCareSystem.API.Services.Cloudinary
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folder);
    }
}
