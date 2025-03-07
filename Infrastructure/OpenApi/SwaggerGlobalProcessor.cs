using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Infrastructure.OpenApi;

/// <summary>
/// A custom operation processor for adding global authentication support in Swagger/OpenAPI
/// </summary>
/// <param name="scheme">
/// The authentication scheme to be applied. Defaults to <see cref="JwtBearerDefaults.AuthenticationScheme"/>
/// </param>
public class SwaggerGlobalProcessor(string scheme) : IOperationProcessor
{
    private readonly string _schema = scheme;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerGlobalProcessor"/> class
    /// with the default authetication scheme (<see cref="JwtBearerDefaults.AuthenticationScheme"/>)
    /// </summary>
    public SwaggerGlobalProcessor() : this(JwtBearerDefaults.AuthenticationScheme)
    {
    }

    /// <summary>
    /// Processes the given operation to conditionally apply requirements for Swagger documentation
    /// </summary>
    /// <param name="context">The context containing operation and API metadata</param>
    /// <returns>
    /// <c>true</c> If the processing is successful; otherwise, <c>false</c>
    /// This implementation always returns <c>true</c>
    /// </returns>
    /// <remarks>
    /// The processor checks for the presence of the <see cref="AllowAnonymousAttribute"/>
    /// If not present and no security requirements exist, it adds a security requirement
    /// for the specified authentication scheme.
    /// </remarks>
    public bool Process(OperationProcessorContext context)
    {
        IList<object> list = ((AspNetCoreOperationProcessorContext)context)
            .ApiDescription.ActionDescriptor.TryGetPropertyValue<IList<object>>("EndpointMetadata");

        if (list is not null)
        {
            if (list.OfType<AllowAnonymousAttribute>().Any())
            {
                return true;
            }

            if (context.OperationDescription.Operation.Security.Count == 0)
            {
                (context.OperationDescription.Operation.Security ??= new List<OpenApiSecurityRequirement>())
                    .Add(new OpenApiSecurityRequirement
                    {
                        {
                            _schema,
                            Array.Empty<string>()
                        }
                    });
            }
        }

        return true;
    }
}

public static class ObjectExtensions
{
    public static T TryGetPropertyValue<T>(this object obj, string propertyName, T defaultValue = default) =>
        obj.GetType().GetRuntimeProperty(propertyName) is PropertyInfo propertyInfo
            ? (T)propertyInfo.GetValue(obj)
            : defaultValue;
}