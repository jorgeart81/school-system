using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using Applicaction;
using Applicaction.Features.Identity.Tokens;
using Applicaction.Wrappers;
using Finbuckle.MultiTenant;
using Infrastructure.Constants;
using Infrastructure.Context;
using Infrastructure.Identity.Auth;
using Infrastructure.Identity.Models;
using Infrastructure.Identity.Tokens;
using Infrastructure.OpenApi;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        string? defaultConnection = config.GetConnectionString("DefaultConnection");

        return services
            .AddDbContext<TenantDbContext>(options =>
            {
                options.UseNpgsql(defaultConnection);
            })
            .AddMultiTenant<SchoolSystemTenantInfo>()
                .WithHeaderStrategy(TenancyConstans.TenantIdName)
                .WithClaimStrategy(TenancyConstans.TenantIdName)
                .WithEFCoreStore<TenantDbContext, SchoolSystemTenantInfo>()
                .Services
            .AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(defaultConnection);
            })
            .AddTransient<ITenantDbSeeder, TenantDbSeeder>()
            .AddTransient<ApplicationDbSeeder>()
            .AddIdentityService()
            .AddPermissions()
            .AddOpenApiDocumentation(config);
    }

    public static async Task AddDatabaseInitializerAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ITenantDbSeeder>()
            .InitializeDatabaseAsync(cancellationToken);
    }

    internal static IServiceCollection AddIdentityService(this IServiceCollection services)
    {
        return services
            .AddIdentity<ApplicationUser, ApplicationRole>(opntions =>
            {
                opntions.Password.RequiredLength = 8;
                opntions.Password.RequireDigit = false;
                opntions.Password.RequireLowercase = false;
                opntions.Password.RequireUppercase = false;
                opntions.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .Services
            .AddScoped<ITokenService, TokenService>();
    }

    internal static IServiceCollection AddPermissions(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }

    public static JwtSettings GetJwtSettings(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettingsConfig = config.GetSection(nameof(JwtSettings));

        // Registers the jwtSettings Config configuration section with the ASP.NET Core dependency injection (DI) system.
        services.Configure<JwtSettings>(jwtSettingsConfig);

        return jwtSettingsConfig.Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings configuration was not found or is empty.");
    }

    public static IServiceCollection AddJwtAuthetication(this IServiceCollection services, JwtSettings jwtSettings)
    {
        byte[]? secret = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        services.AddAuthentication(auth =>
        {
            auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(bearer =>
        {
            bearer.RequireHttpsMetadata = false;
            bearer.SaveToken = true;
            bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };

            bearer.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    static Task ContextResponse(AuthenticationFailedContext context, HttpStatusCode statusCode, string message)
                    {
                        context.Response.StatusCode = (int)statusCode;
                        context.Response.ContentType = "application/json";

                        var result = JsonConvert.SerializeObject(ResponseWrapper.Fail(message));
                        return context.Response.WriteAsync(result);
                    }

                    if (context.Exception is SecurityTokenExpiredException)
                        return ContextResponse(context, HttpStatusCode.Unauthorized, "Token has expired.");
                    else
                        return ContextResponse(context, HttpStatusCode.InternalServerError, "An unhandled error has occured.");
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";

                        var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("You are not authorized."));
                        return context.Response.WriteAsync(result);
                    }

                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";

                    var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("You are not authorized to access this resource."));
                    return context.Response.WriteAsync(result);
                }
            };
        });

        services.AddAuthorization(options =>
        {
            foreach (var prop in typeof(SchoolPermission).GetNestedTypes()
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
            {
                var propertyValue = prop.GetValue(null)?.ToString();
                if (propertyValue is not null)
                {
                    options.AddPolicy(propertyValue, policy =>
                        policy.RequireClaim(ClaimConstants.Permission, propertyValue));
                }
            }
        });

        return services;
    }

    internal static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration config)
    {

        var swaggerSettings = config.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>();

        services.AddEndpointsApiExplorer();
        _ = services.AddOpenApiDocument((document, serviceProvider) =>
        {
            document.PostProcess = doc =>
            {
                doc.Info.Title = swaggerSettings.Title;
                doc.Info.Description = swaggerSettings.Description;
                doc.Info.Contact = new OpenApiContact
                {
                    Name = swaggerSettings.ContactName,
                    Email = swaggerSettings.ContactEmail,
                    Url = swaggerSettings.ContactUrl,
                };
                doc.Info.License = new OpenApiLicense
                {
                    Name = swaggerSettings.LicenseName,
                    Url = swaggerSettings.LicenseUrl,
                };
            };

            document.AddSecurity(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter your Bearer token to attach it as a header on your requests.",
                In = OpenApiSecurityApiKeyLocation.Header,
                Type = OpenApiSecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            });

            document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor());
            document.OperationProcessors.Add(new SwaggerGlobalProcessor());
            document.OperationProcessors.Add(new SwaggerHeaderAttributeProcessor());
        });

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        return app
            .UseAuthentication()
            .UseMultiTenant()
            .UseAuthorization()
            .UseOpenApiDocumentation();
    }

    internal static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
    {
        app.UseOpenApi();
        app.UseSwaggerUi(options =>
        {
            options.DefaultModelExpandDepth = -1;
            options.DocExpansion = "none";
            options.TagsSorter = "alpha";
        });

        return app;
    }
}
