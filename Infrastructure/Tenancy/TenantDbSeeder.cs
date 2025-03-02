using System;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenancy;

public class TenantDbSeeder(
    TenantDbContext tenantDbContext,
    IServiceProvider serviceProvider) : ITenantDbSeeder
{
    private readonly TenantDbContext _tenantDbContext = tenantDbContext;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        await InitializeDatabaseWithTenantAsync(cancellationToken);

        foreach (var tenant in await _tenantDbContext.TenantInfo.ToListAsync(cancellationToken))
        {
            await InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
        }
    }

    private async Task InitializeDatabaseWithTenantAsync(CancellationToken cancellationToken)
    {
        if (await _tenantDbContext.TenantInfo.FindAsync([TenancyConstans.Root.Id], cancellationToken) is null)
        {
            var rootTenant = new SchoolSystemTenantInfo
            {
                Id = TenancyConstans.Root.Id,
                Identifier = TenancyConstans.Root.Id,
                Name = TenancyConstans.Root.Name,
                Email = TenancyConstans.Root.Email,
                FirsName = TenancyConstans.FirsName,
                LastName = TenancyConstans.LastName,
                IsActive = true,
                ValidUpTo = DateTime.UtcNow.AddYears(2),
            };

            await _tenantDbContext.TenantInfo.AddAsync(rootTenant, cancellationToken);
            await _tenantDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task InitializeApplicationDbForTenantAsync(SchoolSystemTenantInfo currentTenant, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        _serviceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<SchoolSystemTenantInfo>()
            {
                TenantInfo = currentTenant
            };

        await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
            .InitializeDatabaseAsync(cancellationToken);
    }
}
