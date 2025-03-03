using System;
using System.Net;

namespace Applicaction.Exceptions;

public class NotFoundException(List<string>? errorMessages = default,
    HttpStatusCode statusCode = HttpStatusCode.NotFound) : Exception
{
    public List<string>? ErrorMessages { get; set; } = errorMessages;
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}
