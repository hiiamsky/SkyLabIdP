namespace SkyLabIdP.Application.Common.Interfaces;

public interface ICaptchaService
{
    Task StoreCaptchaCodeAsync(string captchaId, string captchaCode, CancellationToken cancellationToken);
    Task<bool> ValidateCaptchaCodeAsync(string captchaId, string userInputCaptcha, CancellationToken cancellationToken);
    Task<(bool Success, string CaptchaCode)> TryGetCaptchaCodeAsync(string captchaId, CancellationToken cancellationToken);
    Task<byte[]> GenerateCaptchaImageAsync(string captchaCode, CancellationToken cancellationToken);
    Task<byte[]> GenerateCaptchaAudioAsync(string captchaCode, CancellationToken cancellationToken);
    Task<string> GenerateRandomCaptchaCodeAsync(CancellationToken cancellationToken);
}