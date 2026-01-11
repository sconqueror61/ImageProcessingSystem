namespace DocumentVerificationSystemApi.Entity;

public class UploadFilesEntity : BaseEntity
{
	// Dosya bilgileri
	public string FileName { get; set; } = string.Empty;
	public string FileExtension { get; set; } = string.Empty;
	public long FileSize { get; set; }

	// Dosya içeriği
	public byte[] FileData { get; set; } = Array.Empty<byte>();

	// OCR sonuçları
	public string? OcrText { get; set; }
	public bool OcrCompleted { get; set; }
	public DateTime? OcrProcessedDate { get; set; }

	// Analiz bilgileri
	public bool IsValidDocument { get; set; }
	public string? AnalysisResult { get; set; } // JSON veya açıklama
}