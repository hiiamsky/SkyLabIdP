using System;

namespace SkyLabIdP.Application.Common.Interfaces;

public interface IUserDetailWriterFactory
{
    IUserDetailWriter<TRequest, TResponse> GetWriter<TRequest, TResponse>(string tenantId);
}
