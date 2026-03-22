using SkyLabIdP.Application.Common.Exceptions;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.File;
using Microsoft.AspNetCore.Http;
using MimeDetective;
using MimeDetective.Engine;
using System.Collections.Immutable;

namespace SkyLabIdP.Shared.Services
{
    public class FileService : IFileService
    {
        protected string FileExtension { get; set; } = string.Empty;
        public async Task<Stream> GetFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new NotFoundException("File path cannot be null or empty.");
            }

            if (!File.Exists(filePath))
            {
                throw new NotFoundException("File not found.");
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0; // 重置流的位置，以便讀取者從頭開始讀取
            return memoryStream;
        }

        public string GetFileExtension(FileDto fileDto)
        {
            var file = fileDto.File;
            ValidateFileIsNull(file);
            InspectFile(file, out MemoryStream memoryStream, out ImmutableArray<DefinitionMatch> Matches);
            var Extension = Matches.ByFileExtension().FirstOrDefault()?.Extension;
            return Extension ?? string.Empty;
        }

        protected static void ValidateFileIsNull(IFormFile file)
        {
            if (file == null)
            {
                throw new NotFoundException(nameof(file), "The provided file is null.");
            }
        }

        public long GetFileSize(FileDto fileDto)
        {
            var file = fileDto.File;
            ValidateFileIsNull(file);

            return file.Length;
        }

        public string GetMimeType(FileDto fileDto)
        {
            var file = fileDto.File;
            ValidateFileIsNull(file);

            InspectFile(file, out MemoryStream memoryStream, out ImmutableArray<DefinitionMatch> Matches);
            var MimeType = Matches.ByMimeType().FirstOrDefault()?.MimeType;
            return MimeType ?? string.Empty;
        }

        private static void InspectFile(IFormFile file, out MemoryStream memoryStream, out ImmutableArray<DefinitionMatch> matches)
        {
            memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            memoryStream.Position = 0;

            var inspector = new ContentInspectorBuilder()
            {
                Definitions = new MimeDetective.Definitions.ExhaustiveBuilder()
                {
                    UsageType = MimeDetective.Definitions.Licensing.UsageType.PersonalNonCommercial
                }.Build()
            }.Build();
            matches = inspector.Inspect(memoryStream);
        }

        public async Task SaveFileAsync(FileDto fileDto)
        {
            var file = fileDto.File;
            FileExtension = GetFileExtension(fileDto);
            ValidateFileIsNull(file);

            if (!ValidateFileSize(fileDto))
            {
                throw new ApiException("File size is too large.");
            }
            if (!ValidateFileMime(fileDto))
            {
                throw new ApiException("File real format is not allowed.");
            }
            if (!ValidateFileExtension(fileDto))
            {
                throw new ApiException("File extension is not allowed.");
            }
            if (string.IsNullOrEmpty(fileDto.FileSavePath))
            {
                throw new ApiException("File save path is not specified.");
            }
            if (!Directory.Exists(fileDto.FileSavePath))
            {
                Directory.CreateDirectory(fileDto.FileSavePath);
            }

            var filePath = Path.Combine(fileDto.FileSavePath, $"{fileDto.FileId}.{FileExtension}");

            // 檢查是否已提供文件內容，否則從 IFormFile 讀取
            fileDto.FileContent = await ConvertStreamToByteArrayAsync(file.OpenReadStream());

            // 保存文件
            await File.WriteAllBytesAsync(filePath, fileDto.FileContent);
        }


        public bool ValidateFileExtension(FileDto fileDto)
        {
            // 檢查文件副檔名是否符合限制 linq
            var file = fileDto.File;
            ValidateFileIsNull(file);

            if (fileDto.FileCheckExtensions == null || !fileDto.FileCheckExtensions.Any())
            {
                throw new ApiException("No file extensions specified for validation.");
            }

            return fileDto.FileCheckExtensions.Exists(ext => ext.ToLower() == FileExtension.ToLower());
        }

        public bool ValidateFileMime(FileDto fileDto)
        {
            var file = fileDto.File;
            ValidateFileIsNull(file);

            if (fileDto.FileCheckMimes == null || !fileDto.FileCheckMimes.Any())
            {
                throw new ApiException("No file mimes specified for validation.");
            }
            var fileMinetype = GetMimeType(fileDto);
            return fileDto.FileCheckMimes.Exists(ext => ext.ToLower() == fileMinetype.ToLower());

        }

        public bool ValidateFileSize(FileDto fileDto)
        {
            ValidateFileIsNull(fileDto.File);

            return fileDto.File.Length <= fileDto.FileSizeLimit;
        }


        protected static async Task<byte[]> ConvertStreamToByteArrayAsync(Stream input)
        {
            using var memoryStream = new MemoryStream();
            await input.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}


