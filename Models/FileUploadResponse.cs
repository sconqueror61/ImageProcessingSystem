namespace DocumentVerificationSystemApi.Models;

public class FileUploadResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public FileInfo File { get; set; }
}

public class FileInfo
{
	public Guid Id { get; set; }
	public string FileName { get; set; }
	public string FileExtension { get; set; }
	public long FileSize { get; set; }
	public bool OcrCompleted { get; set; }
	public bool IsValidDocument { get; set; }
	public DateTime CreatedDate { get; set; }
}