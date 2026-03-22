using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SkyLabIdP.WebApi.Helpers
{
    /// <summary>
    /// SwaggerDefaultValues
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Apply
        /// </summary>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null)
                return;

            foreach (var iParam in operation.Parameters)
            {
                if (iParam is not OpenApiParameter parameter) continue;

                var description = apiDescription.ParameterDescriptions.First(
                    pd => pd.Name == parameter.Name);

                parameter.Description ??= description.ModelMetadata.Description;

                if (parameter.Schema is OpenApiSchema schema
                    && schema.Default == null
                    && description.DefaultValue != null)
                {
                    schema.Default = JsonValue.Create(description.DefaultValue.ToString());
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}

