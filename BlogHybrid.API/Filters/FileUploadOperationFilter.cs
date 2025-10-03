// BlogHybrid.API/Filters/FileUploadOperationFilter.cs
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BlogHybrid.API.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // ตรวจสอบว่ามี IFormFile parameter หรือไม่
            var hasFormFile = context.ApiDescription.ParameterDescriptions
                .Any(p => p.ModelMetadata?.ModelType == typeof(IFormFile) ||
                         (p.ModelMetadata?.ModelType?.GetInterfaces()
                             .Any(i => i == typeof(IFormFile)) ?? false));

            if (!hasFormFile)
                return;

            // ลบ parameters เดิม
            operation.Parameters?.Clear();

            // สร้าง RequestBody สำหรับ multipart/form-data
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "Upload file"
                                }
                            },
                            Required = new HashSet<string> { "file" }
                        }
                    }
                }
            };
        }
    }
}