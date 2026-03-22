using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SkyLabIdP.WebApi.Helpers
{
    /// <summary>
    /// OperationFilter to add X-Tenant-Id header to Swagger documentation.
    /// </summary>
    public class SwaggerTenantHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<IOpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Tenant-Id",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Tenant identifier for multi-tenant support",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                }
            });
        }
    }
}