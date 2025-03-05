using System;

namespace Applicaction.Features.Identity.Tokens;

public class TokenResponse
{
    public string? Jwt { get; set; }
    public string? RereshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
