using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SoftwarePioniere.Extensions.AspNetCore.Swagger
{
    public class FileBodyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //https://stackoverflow.com/questions/39152612/swashbuckle-5-and-multipart-form-data-helppages

            var uploadFileMediaType = new OpenApiMediaType()
            {
                Schema = new OpenApiSchema()
                {
                    Type = "object",
                    Properties =
                    {
                        ["file"] = new OpenApiSchema()
                        {
                            Type = "file"
                        }
                    }
                }
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = uploadFileMediaType
                }
            };
        }
    }
}
