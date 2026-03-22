using System;
using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Shared.Services;

/// <summary>
/// 資料保護服務實作類別
/// </summary>
public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<DataProtectionService> _logger;
    private readonly bool _isDevEnvironment;
    private readonly bool _encryptInDevelopment;

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="provider">資料保護提供者</param>
    /// <param name="configuration">配置</param>
    /// <param name="environment">環境</param>
    /// <param name="logger">日誌記錄器</param>
    public DataProtectionService(
        IDataProtectionProvider provider,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<DataProtectionService> logger)
    {
        string purpose = configuration["DataProtection:Purpose"] ?? "SkyLabMgm2g.DataProtection";
        _protector = provider.CreateProtector(purpose);
        _logger = logger;

        // 判斷開發環境
        _isDevEnvironment = environment.EnvironmentName.Equals("Development",
            StringComparison.OrdinalIgnoreCase);

        // 優先檢查環境變數，然後才是設定檔
        var envVar = Environment.GetEnvironmentVariable("SKYLABMGM_ENCRYPT_IN_DEV");
        if (!string.IsNullOrEmpty(envVar))
        {
            _encryptInDevelopment = bool.TryParse(envVar, out var result) && result;
        }
        else
        {
            _encryptInDevelopment = configuration.GetValue<bool>("DataProtection:EncryptInDevelopment", false);
        }

        _logger.LogInformation(
            "資料保護服務已初始化，目前環境: {Environment}, 實際加密: {IsEncrypting}, 設定來源: {Source}",
            environment.EnvironmentName,
            !_isDevEnvironment || (_isDevEnvironment && _encryptInDevelopment),
            !string.IsNullOrEmpty(envVar) ? "環境變數" : "設定檔");
    }

    /// <summary>
    /// 加密文字
    /// </summary>
    /// <param name="text">要加密的文字</param>
    /// <returns>加密後的文字</returns>
    public string Protect(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        try
        {
            // 開發環境且設定不加密時，直接返回原始資料
            if (_isDevEnvironment && !_encryptInDevelopment)
            {
                return text;
            }

            // 否則進行實際加密
            return _protector.Protect(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加密資料時發生錯誤: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 解密文字
    /// </summary>
    /// <param name="text">要解密的文字</param>
    /// <returns>解密後的文字</returns>
    public string Unprotect(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        try
        {
            // 開發環境且設定不加密時，假設所有資料都未加密
            if (_isDevEnvironment && !_encryptInDevelopment)
            {
                return text;
            }

            // 否則進行實際解密
            return _protector.Unprotect(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解密資料時發生錯誤: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 檢查是否為加密文字
    /// </summary>
    /// <param name="text">要檢查的文字</param>
    /// <returns>是否為加密文字</returns>
    public bool IsProtected(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        // 開發環境且設定不加密時，所有資料都未被保護
        if (_isDevEnvironment && !_encryptInDevelopment)
        {
            return false;
        }

        // 否則嘗試解密判斷
        try
        {
            _protector.Unprotect(text);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
