using System.Net;

namespace RestaurantBookingSystem.Web.ClientServices;

public sealed class ApiClientException : Exception
{
    public ApiClientException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }
    public string? ResponseBody { get; }
}
