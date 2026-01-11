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

			// Servis başlatıldığında GoogleAI nesnesini oluşturur
			_googleAI = new GoogleAI(_apiKey);


			// Kullanacağın modeli seçiyoruz (Flash öğrenci dostudur)
			_model = _googleAI.GenerativeModel(Model.Gemini25Flash);
		}

		// Dışarıdan çağrılacak asenkron metod
		public async Task<string> SoruSorAsync(string prompt)
		{
			try
			{
				var response = await _model.GenerateContent(prompt);
				return response.Text;
			}
			catch (Exception ex)
			{
				return $"Hata oluştu: {ex.Message}";
			}
		}
	}
}