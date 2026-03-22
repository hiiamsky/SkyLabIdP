
using Asp.Versioning.ApiExplorer;

namespace SkyLabIdP.WebApi.Extensions
{
    /// <summary>
    /// AppExtensions
    /// </summary>
    public static class AppExtensions
    {
        /// <summary>
        /// UseSwaggerExtension
        /// </summary>
        /// <param name="app"></param>
        /// <param name="provider"></param>
        public static void UseSwaggerExtension(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }

    }



}
