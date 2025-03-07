using System.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Infrastructure.OpenApi;

public class SwaggerHeaderAttributeProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        if (context.MethodInfo.GetCustomAttributes(typeof(SwaggerHeaderAttribute)) is SwaggerHeaderAttribute swaggerHeader)
        {
            var parameters = context.OperationDescription.Operation.Parameters;

            var existingParam = parameters
                .FirstOrDefault(p => p.Kind == NSwag.OpenApiParameterKind.Header && p.Name == swaggerHeader.HeaderName);

            if (existingParam is not null)
            {
                parameters.Remove(existingParam);
            }

            parameters.Add(new OpenApiParameter
            {
                Name = swaggerHeader.HeaderName,
                Kind = OpenApiParameterKind.Header,
                Description = swaggerHeader.Description,
                IsRequired = true,
                Schema = new JsonSchema
                {
                    Type = JsonObjectType.String,
                    Default = swaggerHeader.DefaultValue,
                }
            });
        }

        return true;
    }
}
