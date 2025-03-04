using System;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Identity.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissios = context.User.Claims
            .Where(claim => claim.Type == ClaimConstants.Permission && claim.Value == requirement.Permission);

        if (permissios.Any())
        {
            context.Succeed(requirement);
            await Task.CompletedTask;
        }
    }
}
