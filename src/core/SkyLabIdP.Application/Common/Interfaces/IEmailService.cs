using SkyLabIdP.Application.Dtos.Email;

namespace SkyLabIdP.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailDto emailRequest);
        Task SendErrorEmailAsync(Exception ex, string subjectPrefix = "[SkyLabDoc]");
    }
}
