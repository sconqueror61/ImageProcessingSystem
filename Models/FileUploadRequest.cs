using Microsoft.AspNetCore.Http;

namespace DocumentVerificationSystemApi.Models;

public class FileUploadRequest
{
	public Guid TanetId { get; set; }
	public IFormFile File { get; set; }
}