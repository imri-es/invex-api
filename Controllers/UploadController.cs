using invex_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace invex_api.Controllers
{
    [Route("api/upload")]
    [ApiController]
    [Authorize] // Requires user to be logged in
    public class UploadController : ControllerBase
    {
        private readonly S3StorageService _storageService;
        private readonly IConfiguration _configuration;

        public UploadController(S3StorageService storageService, IConfiguration configuration)
        {
            _storageService = storageService;
            _configuration = configuration;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                // Basic validation for image
                if (!file.ContentType.StartsWith("image/"))
                {
                    return BadRequest("Only image files are allowed.");
                }

                var bucketName = _configuration["YandexS3:BucketName"];
                if (string.IsNullOrEmpty(bucketName))
                {
                    return StatusCode(500, "Bucket name is not configured.");
                }

                var url = await _storageService.UploadImageAsync(file, bucketName);

                return Ok(new { url });
            }
            catch (Exception ex)
            {
                // In a real application, log the exception.
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
