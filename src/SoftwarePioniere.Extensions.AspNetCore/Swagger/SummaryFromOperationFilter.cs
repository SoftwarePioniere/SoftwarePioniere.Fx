using System;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SoftwarePioniere.Extensions.AspNetCore.Swagger
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SummaryFromOperationFilter : IOperationFilter
    {
    
        public void Apply(Operation operation, OperationFilterContext context)
        {
#if DEBUG
            Console.WriteLine($"OperationId: {operation.OperationId} , Summary: {operation.Summary}, Description: {operation.Description}");
#endif

            if (string.IsNullOrEmpty(operation.Summary) && !string.IsNullOrEmpty(operation.OperationId))
            {
                operation.Summary = operation.OperationId;
            }

            //            var swagOp = context.MethodInfo.GetCustomAttributes(true)
            //                .OfType<SwaggerOperationAttribute>()
            //                .FirstOrDefault();

            //#if DEBUG
            //            Console.WriteLine($"OperationId: {operation.OperationId} , Summary: {operation.Summary}, Description: {operation.Description}");

            //            if (swagOp != null)
            //            {
            //                Console.WriteLine($"OperationId: {operation.OperationId} , SwaggerOperationAttributeSummary: {swagOp.Summary}");
            //            }
            //#endif

            //            if (string.IsNullOrEmpty(operation.Summary) && swagOp != null && !string.IsNullOrEmpty(swagOp.Summary))
            //            {

            //                operation.Summary = swagOp.Summary;
            //            }
        }
    }
}