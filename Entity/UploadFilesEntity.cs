namespace DocumentVerificationSystemApi.Entity;

public class UploadFilesEntity : BaseEntity
{
	public Guid CreaterId { get; set; }
	public string FileName { get; set; } = string.Empty;
	public string FileExtension { get; set; } = string.Empty;
	public long FileSize { get; set; }

	public byte[] FileData { get; set; } = Array.Empty<byte>();

	public string? OcrText { get; set; }
	public bool OcrCompleted { get; set; }
	public DateTime? OcrProcessedDate { get; set; }

	public bool IsValidDocument { get; set; }
	public string? AnalysisResult { get; set; }
}