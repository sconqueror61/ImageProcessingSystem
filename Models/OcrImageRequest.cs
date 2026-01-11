using Microsoft.AspNetCore.Http;

namespace DocumentVerificationSystemApi.Models
{
    public class OcrImageRequest
    {
        public IFormFile File { get; set; }
    }
}
