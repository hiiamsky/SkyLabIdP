using System;

namespace SkyLabIdP.Application.Common.Interfaces;

/// <summary>
/// 資料保護服務介面
/// </summary>
public interface IDataProtectionService
{
    /// <summary>
    /// 加密文字，依據環境設定決定是否真正加密
    /// </summary>
    /// <param name="text">要加密的文字</param>
    /// <returns>加密後的文字</returns>
    string Protect(string text);

    /// <summary>
    /// 解密文字，依據環境設定處理
    /// </summary>
    /// <param name="text">要解密的文字</param>
    /// <returns>解密後的文字</returns>
    string Unprotect(string text);

    /// <summary>
    /// 檢查是否為加密文字
    /// </summary>
    /// <param name="text">要檢查的文字</param>
    /// <returns>是否為加密文字</returns>
    bool IsProtected(string text);
}
