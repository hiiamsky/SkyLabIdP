using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SkyLabIdP.Domain.Entities;

[Table("AuditLog")]
public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // 使用者資訊
    public string? UserId { get; set; }
    public string? UserName { get; set; }

    // 追蹤資訊
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; }

    // 請求資訊
    public string? RequestMethod { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestQueryString { get; set; }

    [Column(TypeName = "nvarchar(MAX)")]
    public string? RequestBody { get; set; }

    // 回應資訊
    public int StatusCode { get; set; }

    [Column(TypeName = "nvarchar(MAX)")]
    public string? ResponseBody { get; set; }

    // 效能資訊
    public long ExecutionTime { get; set; }

    // 網路資訊
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }

    public required Dictionary<string, string> AdditionalInfo { get; set; }

    [JsonIgnore]
    public virtual ICollection<string> ExcludedPaths { get; } = new List<string>
        {
            "/api/health",
            "/health",
            "/swagger",
            "/metrics",
            "/favicon.ico"
        };
}
