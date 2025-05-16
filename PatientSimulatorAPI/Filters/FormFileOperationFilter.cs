using Microsoft.OpenApi.Models;
using PatientSimulatorAPI.DTOs;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace PatientSimulatorAPI.Filters
{

    /// <summary>
    /// Detects any IFormFile parameters and updates the OpenAPI schema
    /// so Swagger UI will render a file picker.
    /// </summary>
    public class FormFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile)
                         || p.ParameterType == typeof(FileUploadDto))
                .ToList();

            if (!formFileParams.Any()) return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = formFileParams.ToDictionary(
                                p => p.Name,
                                _ => new OpenApiSchema { Type = "string", Format = "binary" }
                            ),
                            Required = new HashSet<string>(
                                formFileParams.Select(p => p.Name)
                            )
                        }
                    }
                }
            };
        }
    }
}

    
