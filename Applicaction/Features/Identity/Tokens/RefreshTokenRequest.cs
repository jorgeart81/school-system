using System;

namespace Applicaction.Features.Identity.Tokens;

public class RefreshTokenRequest
{
    public string? CurrentJwt { get; set; }
    public string? CurrentRefreshJwt { get; set; }
    public DateTime? RefreshTokenExporyDate { get; set; }
}
