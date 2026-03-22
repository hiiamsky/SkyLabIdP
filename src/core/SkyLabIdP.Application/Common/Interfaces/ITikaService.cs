using System;

namespace SkyLabIdP.Application.Common.Interfaces
{
    public interface ITikaService
    {
        Task<string?> ExtractTextFromPdfAsync(byte[] fileContent, CancellationToken cancellationToken);
    }
}
