using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace SkyLabIdP.Shared.Services
{
    public class TikaService : ITikaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TikaService> _logger;

        public TikaService(HttpClient httpClient, ILogger<TikaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> ExtractTextFromPdfAsync(byte[] fileContent, CancellationToken cancellationToken)
        {
            try
            {
                using var content = new ByteArrayContent(fileContent);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                var request = new HttpRequestMessage(HttpMethod.Put, "/tika")
                {
                    Content = content
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

               
                request.Headers.Add("X-Tika-PDFOcrStrategy", "no_ocr");

                var response = await _httpClient.SendAsync(request, cancellationToken);

                response.EnsureSuccessStatusCode();

                var extractedText = await response.Content.ReadAsStringAsync(cancellationToken);

                
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    _logger.LogInformation("未從 PDF 中提取到文本。");
                    return null; 
                }

                return extractedText;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
               
                throw new TimeoutException("解析 PDF 文本超時。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析 PDF 文本時出現異常。");
                throw new Exception("解析 PDF 文本時出現異常。");
            }
        }
    }
}