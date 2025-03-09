using System;
using Applicaction.Wrappers;
using MediatR;

namespace Applicaction.Features.Identity.Tokens.Queries;

public class GetTokenQuery : IRequest<IResponseWrapper>
{
    public required TokenRequest TokenRequest { get; set; }
}

public class GetTokenQueryHandler(ITokenService tokenService) : IRequestHandler<GetTokenQuery, IResponseWrapper>
{
    private readonly ITokenService _tokenService = tokenService;

    public async Task<IResponseWrapper> Handle(GetTokenQuery request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.LoginAsync(request.TokenRequest);

        return await ResponseWrapper<TokenResponse>.SuccesAsync(data: token);
    }
}
