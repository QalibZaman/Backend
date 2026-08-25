using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.Request
{
    public class UploadProfilePictureRequest
    {
        public required IFormFile File { get; set; }
    }
}
