using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MacroscopTestWork.Services
{
    public class ImageDownloadService: IImageDownloadService
    {
        private readonly HttpClient _httpClient;
        public ImageDownloadService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ImageSource?> DownloadAsync(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url)) 
                return null;

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.DecodePixelWidth = 1200;                
            bitmap.EndInit();                       
            return bitmap;
        }
    }
}
