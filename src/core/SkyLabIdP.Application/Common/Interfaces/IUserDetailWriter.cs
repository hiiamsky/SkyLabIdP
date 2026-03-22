using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkyLabIdP.Application.Common.Interfaces;

/// <summary>
/// 使用者詳細資訊寫入器介面
/// </summary>
/// <typeparam name="TRequest">請求類型</typeparam>
/// <typeparam name="TResponse">回應類型</typeparam>
public interface IUserDetailWriter<TRequest, TResponse>
{
    /// <summary>
    /// 寫入使用者詳細資訊
    /// </summary>
    /// <param name="request">請求</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>回應</returns>
    public Task<TResponse> WriteAsync(TRequest request, CancellationToken cancellationToken);
}