using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types; // CS0246 hatasını çözen kritik satır

namespace DocumentVerificationSystemApi.Service
{
	public class GeminiServices
	{
		// API Anahtarını buraya yazıyoruz
		private readonly string _apiKey = "";
		private readonly GoogleAI _googleAI;
		private readonly GenerativeModel _model;

		public GeminiServices(IConfiguration configuration)
		{

			_apiKey = configuration["GeminiApiKey"];

			if (string.IsNullOrEmpty(_apiKey))
				throw new Exception("GeminiApiKey bulunamadı");

			_googleAI = new GoogleAI(_apiKey);

			_model = _googleAI.GenerativeModel(Model.Gemini25Flash);
		}

		public string OlusturVergiLevhasiPromptu(string ocrText)
		{
			return ocrText + @"
            Yukarıdaki metni analiz et ve aşağıdaki JSON formatında verileri çıkar. 
            Eğer metin bir 'Vergi Levhası' ise ilgili alanları doldur. Veri bulamazsan null geç.
            
            KURALLAR:
            1. Sayısal değerleri (GrossIncome, TaxBase vb.) para birimi simgesi olmadan ve nokta (.) ile ayrılmış yaz. (Örn: 12500.50)
            2. JSON anahtarları (Keys) kesinlikle aşağıdaki isimlerle aynı olmalıdır.

            İstenen JSON Formatı:
            {
                ""DocumentType"": ""Belgenin türü"",
                ""TaxpayerType"": ""Mükellef türü"",
                ""CompanyType"": ""Şirket türü"",
                ""CompanyName"": ""Ad Soyad veya Ticaret Unvanı"",
                ""TaxNumber"": ""Vergi Kimlik Numarası"",
                ""TcIdentityNumber"": ""T.C. Kimlik Numarası"",
                ""TaxOffice"": ""Vergi Dairesi Adı"",
                ""ActivityCode"": ""Faaliyet Kodu"",
                ""GrossIncome"": 0.00,
                ""TaxBase"": 0.00,
                ""CalculatedTax"": 0.00,
                ""AccruedTax"": 0.00,
                ""TaxPeriod"": ""Yıl"",
                ""ExtractedConfidence"": 0.99
            }
            Sadece saf JSON döndür, markdown kullanma.
        ";
		}

		public async Task<string> SoruSorAsync(string prompt)
		{
			int maxRetries = 3;
			int delay = 2000;

			for (int i = 0; i < maxRetries; i++)
			{
				try
				{
					Console.WriteLine($"[GeminiService] {i + 1}. Deneme yapılıyor...");

					var response = await _model.GenerateContent(prompt);

					Console.WriteLine("[GeminiService] Başarılı! Cevap alındı.");
					return response.Text;
				}
				catch (Exception ex)
				{
					// Hata detayını konsola yaz
					Console.WriteLine($"[GeminiService] HATA: {ex.Message}");

					bool isRateLimit = ex is GeminiApiTimeoutException || ex.Message.Contains("429");

					if (isRateLimit && i < maxRetries - 1)
					{
						Console.WriteLine($"[GeminiService] Rate Limit takıldı. {delay / 1000} saniye bekleniyor...");
						await Task.Delay(delay);
						delay *= 2;
						continue;
					}
					else
					{
						Console.WriteLine("[GeminiService] Tüm denemeler başarısız oldu veya kritik hata alındı.");
						// Burada hatayı logluyoruz ama boş dönüyoruz ki sistem çökmesin
						return "";
					}
				}
			}
			return "";
		}
	}
}
