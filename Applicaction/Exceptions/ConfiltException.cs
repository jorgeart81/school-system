using System;
using System.Net;

namespace Applicaction.Exceptions;

public class ConfiltException(List<string>? errorMessages = default,
    HttpStatusCode statusCode = HttpStatusCode.Conflict) : Exception
{
    public List<string>? ErrorMessages { get; set; } = errorMessages;
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}
