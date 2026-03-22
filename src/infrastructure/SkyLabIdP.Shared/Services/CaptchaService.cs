
using System.Security.Cryptography;
using System.Text;

using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SkiaSharp;

namespace SkyLabIdP.Shared.Services;

public class CaptchaService : ICaptchaService
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CaptchaService> _logger;

    public CaptchaService(IDistributedCache cache, IConfiguration configuration, ILogger<CaptchaService> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StoreCaptchaCodeAsync(string captchaId, string captchaCode, CancellationToken cancellationToken)
    {
        _logger.LogInformation("儲存驗證碼中：CaptchaId={CaptchaId}", captchaId);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        var captchaBytes = Encoding.UTF8.GetBytes(captchaCode);
        await _cache.SetAsync(captchaId, captchaBytes, options, cancellationToken);
    }

    public async Task<bool> ValidateCaptchaCodeAsync(string captchaId, string userInputCaptcha, CancellationToken cancellationToken)
    {
        _logger.LogInformation("驗證驗證碼中：CaptchaId={CaptchaId}", captchaId);

        var storedCaptchaBytes = await _cache.GetAsync(captchaId, cancellationToken);

        if (storedCaptchaBytes != null && storedCaptchaBytes.Length > 0)
        {
            var storedCaptchaCode = Encoding.UTF8.GetString(storedCaptchaBytes);
            var isValid = storedCaptchaCode.Equals(userInputCaptcha, StringComparison.OrdinalIgnoreCase);
            await _cache.RemoveAsync(captchaId, cancellationToken);
            return isValid;
        }

        _logger.LogWarning("驗證碼無效或不存在：CaptchaId={CaptchaId}", captchaId);
        return false;
    }

    public async Task<(bool Success, string CaptchaCode)> TryGetCaptchaCodeAsync(string captchaId, CancellationToken cancellationToken)
    {
        var captchaBytes = await _cache.GetAsync(captchaId, cancellationToken);
        if (captchaBytes == null || captchaBytes.Length == 0)
        {
            return (false, string.Empty);
        }
        
        var captchaCode = Encoding.UTF8.GetString(captchaBytes);
        return (!string.IsNullOrEmpty(captchaCode), captchaCode);
    }



    public async Task<string> GenerateRandomCaptchaCodeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("生成隨機驗證碼中");

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int codeLength = 5;
        var captchaCode = new char[codeLength];

        using (var rng = RandomNumberGenerator.Create())
        {
            var randomBytes = new byte[codeLength];

            rng.GetBytes(randomBytes);

            for (int i = 0; i < codeLength; i++)
            {
                captchaCode[i] = chars[randomBytes[i] % chars.Length];
            }
        }

        return await Task.FromResult(new string(captchaCode));
    }

    public async Task<byte[]> GenerateCaptchaImageAsync(string captchaCode, CancellationToken cancellationToken)
    {
        _logger.LogInformation("生成驗證碼圖像中");

        int width = 200;
        int height = 60;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        using var rng = RandomNumberGenerator.Create();

        // Secure random lines
        using var linePaint = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1 };
        for (int i = 0; i < 20; i++)
        {
            var startX = GetSecureRandomNumber(rng, width);
            var startY = GetSecureRandomNumber(rng, height);
            var endX = GetSecureRandomNumber(rng, width);
            var endY = GetSecureRandomNumber(rng, height);

            canvas.DrawLine(new SKPoint(startX, startY), new SKPoint(endX, endY), linePaint);
        }

        // Secure random noise dots
        using var dotPaint = new SKPaint { Color = SKColors.Gray, StrokeWidth = 2 };
        for (int i = 0; i < 100; i++)
        {
            var x = GetSecureRandomNumber(rng, width);
            var y = GetSecureRandomNumber(rng, height);
            canvas.DrawPoint(x, y, dotPaint);
        }

        // 創建 SKFont 替代 SKPaint.TextSize
        var font = new SKFont
        {
            Size = 36 // 設定文字大小
        };

        // 創建 SKPaint，仍然可以用於顏色和其他屬性
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        // 使用 SKFont.MeasureText 替代 SKPaint.MeasureText
        var textWidth = font.MeasureText(captchaCode);

        // 計算文字的繪製位置
        var xText = (width - textWidth) / 2;
        var yText = (height + font.Size) / 2;

        // 使用新的 SKCanvas.DrawText 方法，包含 SKFont
        canvas.DrawText(captchaCode, xText, yText, SKTextAlign.Left, font, textPaint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return await Task.FromResult(data.ToArray());
    }

    private static int GetSecureRandomNumber(RandomNumberGenerator rng, int maxValue)
    {
        var randomNumber = new byte[4];
        rng.GetBytes(randomNumber);
        return Math.Abs(BitConverter.ToInt32(randomNumber, 0) % maxValue);
    }

    public async Task<byte[]> GenerateCaptchaAudioAsync(string captchaCode, CancellationToken cancellationToken)
    {
        _logger.LogInformation("生成驗證碼音頻中");

        var audioFilesPath = _configuration["Captcha:AudioFilesPath"];
        if (string.IsNullOrEmpty(audioFilesPath))
        {
            _logger.LogError("音頻文件路徑未設置");
            throw new InvalidOperationException("音頻文件路徑未設置");
        }

        using var finalStream = new MemoryStream();
        int totalAudioDataLength = 0;
        byte[] audioFormatInfo = [];

        foreach (char c in captchaCode)
        {
            string filePath = GetAudioFilePath(audioFilesPath, c);
            byte[] audioData = await Task.Run(() => ReadAudioData(filePath, ref audioFormatInfo), cancellationToken);
            totalAudioDataLength += audioData.Length;
            await finalStream.WriteAsync(audioData, 0, audioData.Length, cancellationToken);
        }

        if (audioFormatInfo == null)
        {
            _logger.LogError("音頻格式信息未初始化");
            throw new InvalidOperationException("音頻格式信息未初始化");
        }

        UpdateWavHeader(audioFormatInfo, totalAudioDataLength);

        using var completeStream = new MemoryStream();
        await completeStream.WriteAsync(audioFormatInfo, 0, audioFormatInfo.Length, cancellationToken);
        finalStream.Position = 0;
        await finalStream.CopyToAsync(completeStream, cancellationToken);
        completeStream.Position = 0;

        return completeStream.ToArray();
    }

    private static string GetAudioFilePath(string audioFilesPath, char c)
    {
        string fileName = $"{char.ToUpper(c)}.wav";
        string filePath = Path.Combine(audioFilesPath, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"音頻文件 {fileName} 不存在");
        }

        return filePath;
    }

    private static byte[] ReadAudioData(string filePath, ref byte[] audioFormatInfo)
    {
        using var digitStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(digitStream);

        // 讀取並檢查 RIFF 標頭
        string riff = new(reader.ReadChars(4));
        if (riff != "RIFF")
        {
            throw new InvalidDataException("無效的 WAV 文件格式。");
        }

        reader.ReadInt32(); // 總檔案大小（不使用）

        // 讀取並檢查 WAVE 標頭
        string wave = new string(reader.ReadChars(4));
        if (wave != "WAVE")
        {
            throw new InvalidDataException("無效的 WAV 文件格式。");
        }

        // 讀取並檢查 fmt 標頭
        reader.ReadChars(4); // fmtChunkID
        int fmtChunkSize = reader.ReadInt32();
        short audioFormat = reader.ReadInt16();
        short numChannels = reader.ReadInt16();
        int sampleRate = reader.ReadInt32();
        int byteRate = reader.ReadInt32();
        short blockAlign = reader.ReadInt16();
        short bitsPerSample = reader.ReadInt16();

        // 若 fmt 區塊超過 16 位元組，則跳過多餘的資料
        if (fmtChunkSize > 16)
        {
            reader.ReadBytes(fmtChunkSize - 16);
        }

        // 尋找並讀取 data 區塊
        string dataChunkID = new string(reader.ReadChars(4));
        while (dataChunkID != "data")
        {
            int chunkSize = reader.ReadInt32();
            reader.ReadBytes(chunkSize);
            dataChunkID = new string(reader.ReadChars(4));
        }

        int dataChunkSize = reader.ReadInt32();
        byte[] audioData = reader.ReadBytes(dataChunkSize);

        // 若音訊格式資訊尚未初始化，則建立並設定
        if (audioFormatInfo.Length == 0)
        {
            using var headerStream = new MemoryStream();
            using var writer = new BinaryWriter(headerStream);

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(0); // 將檔案大小設為 0，稍後會更新
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(fmtChunkSize);
            writer.Write(audioFormat);
            writer.Write(numChannels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            if (fmtChunkSize > 16)
            {
                byte[] extraBytes = reader.ReadBytes(fmtChunkSize - 16);
                writer.Write(extraBytes);
            }

            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(0); // data 區塊大小設為 0，稍後會更新

            audioFormatInfo = headerStream.ToArray();
        }

        return audioData;
    }

    private static void UpdateWavHeader(byte[] audioFormatInfo, int totalAudioDataLength)
    {
        int totalFileSize = audioFormatInfo.Length - 8 + totalAudioDataLength;
        int dataChunkSizeOffset = audioFormatInfo.Length - 4;

        // 更新 RIFF 區塊的檔案大小（在音訊格式資訊的第 4 位元組）
        audioFormatInfo[4] = (byte)(totalFileSize & 0xFF);
        audioFormatInfo[5] = (byte)((totalFileSize >> 8) & 0xFF);
        audioFormatInfo[6] = (byte)((totalFileSize >> 16) & 0xFF);
        audioFormatInfo[7] = (byte)((totalFileSize >> 24) & 0xFF);

        // 更新 data 區塊的大小（在音訊格式資訊的最後 4 位元組）
        audioFormatInfo[dataChunkSizeOffset] = (byte)(totalAudioDataLength & 0xFF);
        audioFormatInfo[dataChunkSizeOffset + 1] = (byte)((totalAudioDataLength >> 8) & 0xFF);
        audioFormatInfo[dataChunkSizeOffset + 2] = (byte)((totalAudioDataLength >> 16) & 0xFF);
        audioFormatInfo[dataChunkSizeOffset + 3] = (byte)((totalAudioDataLength >> 24) & 0xFF);
    }
}