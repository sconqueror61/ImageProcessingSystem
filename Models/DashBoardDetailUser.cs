namespace DocumentVerificationSystemApi.Models
{
	public class DashboardDetailUser
	{
		// Gizli tutulacak ama işlem için gerekli ID'ler
		public Guid DetailId { get; set; }
		public Guid FileId { get; set; } // Download ve Delete için lazım

		// Details Tablosundan Gelen Veriler
		public string DocumentType { get; set; }
		public string TaxpayerType { get; set; }
		public string CompanyType { get; set; }
		public string CompanyName { get; set; }
		public string TaxNumber { get; set; }
		public string TcIdentityNumber { get; set; }
		public string TaxOffice { get; set; }
		public string ActivityCode { get; set; }
		public decimal? GrossIncome { get; set; }
		public decimal? TaxBase { get; set; }
		public decimal? CalculatedTax { get; set; }
		public decimal? AccruedTax { get; set; }
		public string TaxPeriod { get; set; }
		public float? ExtractedConfidence { get; set; }
		public DateTime CreatedDate { get; set; }

		// UploadFiles Tablosundan Gelenler (Download için)
		public string FileName { get; set; }
		public string FileExtension { get; set; }
		public Guid CreaterId { get; set; }
	}

	public class PagedResult<T>
	{
		public List<T> Items { get; set; }
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
	}
}