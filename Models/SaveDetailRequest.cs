namespace DocumentVerificationSystemApi.Models
{
	public class SaveDetailsRequest
	{
		public Guid FileId { get; set; }
		public Guid TanetId { get; set; }
		public string AnalysisResult { get; set; } // JSON formatındaki string
	}
}
