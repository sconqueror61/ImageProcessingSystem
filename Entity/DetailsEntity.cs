using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentVerificationSystemApi.Entity
{
	public class DetailsEntity : BaseEntity
	{
		public Guid UploadFileId { get; set; }

		[ForeignKey(nameof(UploadFileId))]
		public virtual UploadFilesEntity UploadFile { get; set; }
		public string? DocumentType { get; set; }      // Vergilevhası
		public string? TaxpayerType { get; set; }      // Gerçek Kişi / Tüzel Kişi
		public string? CompanyType { get; set; }       // Şahıs / Ltd / AŞ / Serbest Meslek
		public string? CompanyName { get; set; }       // company_name
		public string? TaxNumber { get; set; }         // Vergi Kimlik No
		public string? TcIdentityNumber { get; set; } // TC no
		public string? TaxOffice { get; set; }         // Bağlı olunan vergi dairesi 
		public string? ActivityCode { get; set; }      // NACE / faaliyet kodu

		[Column(TypeName = "decimal(18,2)")]
		public decimal? GrossIncome { get; set; }      //Brüt satış hasılatı
		[Column(TypeName = "decimal(18,2)")]
		public decimal? TaxBase { get; set; }          // tax_base (Matrah)
		[Column(TypeName = "decimal(18,2)")]
		public decimal? CalculatedTax { get; set; }    // calculated_tax (Hesaplanan Vergi)
		[Column(TypeName = "decimal(18,2)")]
		public decimal? AccruedTax { get; set; }       // accrued_tax (Tahakkuk Eden)
		public string? TaxPeriod { get; set; }         // tax_period (Yıl: 2023, 2024 vb.)

		public float? ExtractedConfidence { get; set; } // extracted_confidence (0.95, 98.5 vb.)

	}
}