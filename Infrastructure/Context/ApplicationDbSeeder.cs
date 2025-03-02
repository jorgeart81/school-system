using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class ApplicationDbSeeder(
    IMultiTenantContextAccessor<SchoolSystemTenantInfo> tenantInfoContextAccessor,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext applicationDbContext)
{
    private readonly IMultiTenantContextAccessor<SchoolSystemTenantInfo> _tenantInfoContextAccessor = tenantInfoContextAccessor;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        if (_applicationDbContext.Database.GetMigrations().Any())
        {
            if ((await _applicationDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await _applicationDbContext.Database.MigrateAsync(cancellationToken);
            }

            if (await _applicationDbContext.Database.CanConnectAsync(cancellationToken))
            {
                await InitializeDefaultRoleAsync(cancellationToken);
                // Users > Assing Roles
                await InitializeAdminUserAsync();
            }
        }
    }

    private async Task InitializeDefaultRoleAsync(CancellationToken cancellationToken)
    {
        foreach (var rolename in RoleConstants.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(role => role.Name == rolename, cancellationToken) is not ApplicationRole incomingRole)
            {
                incomingRole = new ApplicationRole()
                {
                    Name = rolename,
                    Description = $"{rolename} Role"
                };

                await _roleManager.CreateAsync(incomingRole);
            }

            // Assing permissions
            if (rolename == RoleConstants.Admin)
            {
                await AssingPermissionsToRoleAsync(SchoolPermissions.Admin, incomingRole, cancellationToken);

                if (_tenantInfoContextAccessor?.MultiTenantContext?.TenantInfo?.Id == TenancyConstans.Root.Id)
                {
                    await AssingPermissionsToRoleAsync(SchoolPermissions.Root, incomingRole, cancellationToken);
                }
            }
            else if (rolename == RoleConstants.Basic)
            {
                await AssingPermissionsToRoleAsync(SchoolPermissions.Basic, incomingRole, cancellationToken);
            }

        }
    }

    private async Task AssingPermissionsToRoleAsync(
        IReadOnlyList<SchoolPermission> incomingRolePermissions,
        ApplicationRole role,
        CancellationToken cancellationToken)
    {
        var currentlyAssignedClaims = await _roleManager.GetClaimsAsync(role);

        foreach (var incomingPermission in incomingRolePermissions)
        {
            if (!currentlyAssignedClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == incomingPermission.Name))
            {
                await _applicationDbContext.RoleClaims.AddAsync(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = ClaimConstants.Permission,
                    ClaimValue = incomingPermission.Name,
                    Description = incomingPermission.Description,
                    Group = incomingPermission.Group,
                }, cancellationToken);

                await _applicationDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task InitializeAdminUserAsync()
    {
        if (string.IsNullOrEmpty(_tenantInfoContextAccessor?.MultiTenantContext?.TenantInfo?.Email)) return;

        if (await _userManager.Users
            .FirstOrDefaultAsync(user => user.Email == _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email)
            is not ApplicationUser incomingUser)
        {
            incomingUser = new ApplicationUser
            {
                FirsName = TenancyConstans.FirsName,
                LastName = TenancyConstans.LastName,
                UserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpperInvariant(),
                NormalizedUserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpperInvariant(),
                IsActive = true,
            };

            var passwordHash = new PasswordHasher<ApplicationUser>();

            incomingUser.PasswordHash = passwordHash.HashPassword(incomingUser, TenancyConstans.DefautlPassword);
            await _userManager.CreateAsync(incomingUser);
        }

        if (!await _userManager.IsInRoleAsync(incomingUser, RoleConstants.Admin))
        {
            await _userManager.AddToRoleAsync(incomingUser, RoleConstants.Admin);
        }
    }
}
