using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SkyLabIdP.WebApi.Helpers
{
    /// <summary>
    /// ConfigureSwaggerOptions
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="provider"></param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;
        /// <summary>
        /// Configure
        /// </summary>
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
        /// <summary>
        /// CreateInfoForApiVersion
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = " SkyLab查詢系統 - 身分認證系統 (IdP) Web API",
                Version = description.ApiVersion.ToString(),
                Description = "SkyLab查詢系統 - 身分認證系統 (IdP).",
                Contact = new OpenApiContact
                {
                    Name = "Sky Hsieh",
                    Email = "skyhsieh@skylab.com.tw",
                    Url = new Uri("https://skylabdoc.skylab.com.tw/support")
                }
            };

            if (description.IsDeprecated)
                info.Description += " <strong> SkyLab查詢系統 - 身分認證系統 (IdP)</strong>";

            return info;
        }
    }
}


