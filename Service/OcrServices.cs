using Google.Cloud.Vision.V1;
using Google.Apis.Auth.OAuth2;

namespace DocumentVerificationSystemApi.Service
{

	public class OcrService
	{
		private readonly ImageAnnotatorClient _client;

		public OcrService()
		{
			var jsonPath = Path.Combine(
				Directory.GetCurrentDirectory(),
				"Key",
				"gleaming-store-484008-e8-842c6cc03b7b.json"
			);

			// Google Cloud Vision API istemcisini kimlik bilgileri ile oluştur
			var credential = GoogleCredential.FromFile(jsonPath)
				.CreateScoped(ImageAnnotatorClient.DefaultScopes);

			var builder = new ImageAnnotatorClientBuilder
			{
				GoogleCredential = credential
			};

			_client = builder.Build();
		}

		/// <summary>
		/// Dosya yolu ile OCR işlemi yapar (async)
		/// </summary>
		public async Task<string> ReadTextAsync(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException($"Dosya bulunamadı: {filePath}");

			var imageBytes = await File.ReadAllBytesAsync(filePath);
			var image = Image.FromBytes(imageBytes);
			var response = await _client.DetectTextAsync(image);

			return response.FirstOrDefault()?.Description ?? "";
		}

		/// <summary>
		/// Byte array ile OCR işlemi yapar (async)
		/// </summary>
		public async Task<string> ReadTextAsync(byte[] imageBytes)
		{
			var image = Image.FromBytes(imageBytes);
			var response = await _client.DetectTextAsync(image);

			return response.FirstOrDefault()?.Description ?? "";
		}

		/// <summary>
		/// Byte array ile OCR işlemi yapar (sync - eski metod)
		/// </summary>
		public string ReadText(byte[] imageBytes)
		{
			var image = Image.FromBytes(imageBytes);
			var response = _client.DetectText(image);

			return response.FirstOrDefault()?.Description ?? "";
		}
	}

}