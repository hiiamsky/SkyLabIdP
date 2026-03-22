using System.Text.Json.Serialization;

namespace SkyLabIdP.Application.Dtos
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public List<string> Messages { get; set; } = [];
        public int StatusCode { get; set; }

        public object? Data { get; set; }

        [JsonConstructor]
        public OperationResult(bool success, List<string> messages, int statusCode = 200)
        {
            Success = success;
            Messages = messages ?? [];
            StatusCode = statusCode;
        }

        // 接受單個消息參數的構造函數
        public OperationResult(bool success, string message, int statusCode = 200)
            : this(success, [message], statusCode)
        {
        }

        // 新增帶有 Data 的構造函數
        public OperationResult(bool success, string message, int statusCode, object data)
            : this(success, message, statusCode)
        {
            Data = data;
        }
    }


}
