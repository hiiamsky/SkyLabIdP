using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;

namespace SkyLabIdP.WebApi.Helpers.Middleware;

/// <summary>
    /// Http請求和響應的包裝類，用於讀取請求體和響應體
    /// </summary>
    public class HttpRequestWrapper
    {
        private readonly HttpRequest _request;
        private readonly RecyclableMemoryStreamManager _streamManager = new RecyclableMemoryStreamManager();
        private string? _body;

        /// <summary>
        /// 初始化 HttpRequestWrapper，啟用請求體緩衝以支援多次讀取
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        public HttpRequestWrapper(HttpRequest request)
        {
            _request = request;
            // 確保請求體可以多次讀取
            EnableBuffering();
        }

        private void EnableBuffering()
        {
            // 只有當請求體存在且未啟用緩衝時才啟用
            if (!_request.Body.CanSeek)
            {
                _request.EnableBuffering();
            }
        }

        /// <summary>
        /// 非同步讀取請求體內容
        /// </summary>
        /// <returns>請求體字串</returns>
        public async Task<string> GetBodyAsync()
        {
            if (_body != null)
            {
                return _body;
            }

            _request.Body.Position = 0;
            using var stream = _streamManager.GetStream();
            await _request.Body.CopyToAsync(stream);
            _body = Encoding.UTF8.GetString(stream.ToArray());

            // 重置位置以便後續中間件可以讀取
            _request.Body.Position = 0;

            return _body;
        }
    }
    /// <summary>
    /// HttpResponseWrapper
    /// 用於攔截和讀取HttpResponse的內容
    /// </summary>
    public class HttpResponseWrapper
    {
        private readonly RecyclableMemoryStreamManager _streamManager = new RecyclableMemoryStreamManager();
        private readonly Stream _originalBody;
        private readonly MemoryStream _bodyStream;
        private string? _body;
        /// <summary>
        /// HttpResponseWrapper
        /// 用於攔截和讀取HttpResponse的內容
        /// </summary>
        /// <param name="response"></param>
        public HttpResponseWrapper(HttpResponse response)
        {
            _originalBody = response.Body;
            
            // 攔截響應流以便我們可以讀取
            _bodyStream = _streamManager.GetStream();
            response.Body = _bodyStream;
        }

        /// <summary>
        /// 非同步讀取響應體內容
        /// </summary>
        /// <returns>響應體字串</returns>
        public async Task<string> GetBodyAsync()
        {
            if (_body != null)
            {
                return _body;
            }

            _bodyStream.Position = 0;
            using (var reader = new StreamReader(_bodyStream, Encoding.UTF8, leaveOpen: true))
            {
                _body = await reader.ReadToEndAsync();
            }

            _bodyStream.Position = 0;
            return _body;
        }

        /// <summary>
        /// 將攔截的響應體複製回原始輸出流
        /// </summary>
        public async Task CopyBodyToOriginalStreamAsync()
        {
            _bodyStream.Position = 0;
            await _bodyStream.CopyToAsync(_originalBody);
        }
    }
