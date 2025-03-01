using Finbuckle.MultiTenant;
using Infrastructure.Context;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            });
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        return app
            .UseMultiTenant();
    }
}
