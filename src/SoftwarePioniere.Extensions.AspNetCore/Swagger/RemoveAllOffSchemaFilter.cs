using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SoftwarePioniere.Extensions.AspNetCore.Swagger
{
    public class RemoveAllOffSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties != null)
            {
                foreach (var pKey in schema.Properties.Keys)
                {
                    var p = schema.Properties[pKey];

                    if (p.AllOf != null && p.AllOf.Count == 1)
                    {
                        p.Reference = p.AllOf[0].Reference;
                        p.AllOf = new List<OpenApiSchema>();
                    }

                }
            }
        }
    }
}