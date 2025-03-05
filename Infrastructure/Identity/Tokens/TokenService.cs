using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Applicaction.Exceptions;
using Applicaction.Features.Identity.Tokens;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Tokens;

public class TokenService(UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IMultiTenantContextAccessor<SchoolSystemTenantInfo> tenantContextAccessor) : ITokenService
{
    public async Task<TokenResponse> LoginAsync(TokenRequest request)
    {
        #region Validations
        var tenantInfo = tenantContextAccessor.MultiTenantContext.TenantInfo;

        if (tenantInfo is null || !tenantInfo.IsActive)
            throw new UnauthorizedException(["Tenant Subscription is not Active. Contact Administrator."]);


        var userInDb = await userManager.FindByNameAsync(request.Username)
            ?? throw new UnauthorizedException(["Authentication no successful."]);

        if (await userManager.CheckPasswordAsync(userInDb, request.Password))
            throw new UnauthorizedException(["Incorrect credentials."]);


        if (!userInDb.IsActive)
            throw new UnauthorizedException(["User not active. Contact Administrator."]);


        if (tenantInfo.Id is not TenancyConstans.Root.Id)
        {
            if (tenantInfo.ValidUpTo < DateTime.UtcNow)
                throw new UnauthorizedException(["Subscription has expired. Contact Administrator."]);
        }
        #endregion

        return await GenerateTokenAndUpdateUserAsync(userInDb);
    }

    public Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        throw new NotImplementedException();
    }

    private async Task<TokenResponse> GenerateTokenAndUpdateUserAsync(ApplicationUser user)
    {
        string newJwt = await GenerateToken(user);

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        await userManager.UpdateAsync(user);

        return new TokenResponse
        {
            Jwt = newJwt,
            RereshToken = user.RefreshToken,
            RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
        };
    }

    private async Task<string> GenerateToken(ApplicationUser user)
    {
        SigningCredentials signingCredentials = GenerateSigningCredentials();
        IEnumerable<Claim> claims = await GetUserClaims(user);
        return GenerateEncryptedToken(signingCredentials, claims);
    }

    private static string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        JwtSecurityToken token = new(
             claims: claims,
             expires: DateTime.UtcNow.AddMinutes(60),
             signingCredentials: signingCredentials);

        JwtSecurityTokenHandler tokenHandler = new();

        return tokenHandler.WriteToken(token);
    }

    private static SigningCredentials GenerateSigningCredentials()
    {
        string jwtKey = "MIIBVgIBADANBgkqhkiG9w0BAQEFAASCAUAwggE8AgEAAkEA1XIpaAmDvLkGV7J8ipFIOu3bcK3TOPEUziZnJSCRzti3j7nuChGik+2PDGuHBHFOm205lU0SIpKeGpFgxxblXrFcFDC";

        byte[] secret = Encoding.UTF8.GetBytes(jwtKey);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }

    private async Task<IEnumerable<Claim>> GetUserClaims(ApplicationUser user)
    {
        var userClaims = await userManager.GetClaimsAsync(user);
        var userRoles = await userManager.GetRolesAsync(user);

        List<Claim> roleClaims = [];
        List<Claim> permissionClaims = [];

        foreach (var userRole in userRoles)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, userRole));

            ApplicationRole? currentRole = await roleManager.FindByNameAsync(userRole);
            var allPermissionsForCurrentRole = await roleManager.GetClaimsAsync(currentRole);

            permissionClaims.AddRange(allPermissionsForCurrentRole);
        }

        return new List<Claim> {
            new(ClaimTypes.NameIdentifier,user.Id),
            new(ClaimTypes.Email,user.Email ?? string.Empty),
            new(ClaimTypes.Name,user.FirsName?? string.Empty),
            new(ClaimConstants.Tenant,tenantContextAccessor?.MultiTenantContext?.TenantInfo?.Id ?? string.Empty),
            new(ClaimTypes.MobilePhone,user.PhoneNumber ?? string.Empty) }
            .Union(roleClaims)
            .Union(userClaims)
            .Union(permissionClaims);
    }

    private static string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}
