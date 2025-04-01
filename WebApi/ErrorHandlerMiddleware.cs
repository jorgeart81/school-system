using System.Net;
using System.Text.Json;
using Applicaction.Exceptions;
using Applicaction.Wrappers;

namespace WebApi;

public class ErrorHandlerMiddleware(RequestDelegate requestDelegate)
{
    private readonly RequestDelegate _next = requestDelegate;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            HttpResponse? response = context.Response;
            response.ContentType = "application/json";

            IResponseWrapper? responseWrapper = ResponseWrapper.Fail(message: ex.Message);

            switch (ex)
            {
                case ConflictException ce:
                    response.StatusCode = (int)ce.StatusCode;
                    responseWrapper.Messages = ce.ErrorMessages;
                    break;
                case NotFoundException nfe:
                    response.StatusCode = (int)nfe.StatusCode;
                    responseWrapper.Messages = nfe.ErrorMessages;
                    break;
                case ForbiddenException fe:
                    response.StatusCode = (int)fe.StatusCode;
                    responseWrapper.Messages = fe.ErrorMessages;
                    break;
                case IdentityException ie:
                    response.StatusCode = (int)ie.StatusCode;
                    responseWrapper.Messages = ie.ErrorMessages;
                    break;
                case UnauthorizedException ue:
                    response.StatusCode = (int)ue.StatusCode;
                    responseWrapper.Messages = ue.ErrorMessages;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    responseWrapper.Messages = ["Something went wrong. Contact Administrator"];
                    break;
            }

            string? result = JsonSerializer.Serialize(responseWrapper);

            await response.WriteAsync(result);
        }
    }
}
