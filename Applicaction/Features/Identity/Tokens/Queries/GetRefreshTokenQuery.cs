using System;
using Applicaction.Wrappers;
using MediatR;

namespace Applicaction.Features.Identity.Tokens.Queries;

public class GetRefreshTokenQuery : IRequest<IResponseWrapper>
{
    public required RefreshTokenRequest RefreshToken { get; set; }
}

public class GetRefreshTokenQueryHandler(ITokenService tokenService) : IRequestHandler<GetRefreshTokenQuery, IResponseWrapper>
{
    private readonly ITokenService _tokenService = tokenService;

    public async Task<IResponseWrapper> Handle(GetRefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var refreshToken = await _tokenService.RefreshTokenAsync(request.RefreshToken);

        return await ResponseWrapper<TokenResponse>.SuccesAsync(data: refreshToken);
    }
}