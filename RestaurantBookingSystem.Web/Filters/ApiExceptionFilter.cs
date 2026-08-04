using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RestaurantBookingSystem.Web.Authentication;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Filters;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    private readonly IJwtSessionService _sessionService;
    private readonly IModelMetadataProvider _metadataProvider;
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(
        IJwtSessionService sessionService,
        IModelMetadataProvider metadataProvider,
        ILogger<ApiExceptionFilter> logger)
    {
        _sessionService = sessionService;
        _metadataProvider = metadataProvider;
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ApiClientException apiException)
            return;

        _logger.LogWarning(
            apiException,
            "Backend API returned status code {StatusCode}.",
            (int)apiException.StatusCode);

        if (apiException.StatusCode == HttpStatusCode.Unauthorized)
        {
            _sessionService.Clear();
            var request = context.HttpContext.Request;
            var returnUrl = $"{request.PathBase}{request.Path}{request.QueryString}";
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            context.ExceptionHandled = true;
            return;
        }

        var statusCode = (int)apiException.StatusCode;
        var model = new ErrorViewModel
        {
            StatusCode = statusCode,
            Title = apiException.StatusCode switch
            {
                HttpStatusCode.Forbidden => "Access denied",
                HttpStatusCode.NotFound => "Data not found",
                HttpStatusCode.Conflict => "The data is currently in use",
                HttpStatusCode.BadRequest => "The submitted data is invalid",
                HttpStatusCode.ServiceUnavailable => "Backend service unavailable",
                _ => "The request could not be completed"
            },
            Message = apiException.StatusCode switch
            {
                HttpStatusCode.Forbidden => "Your account does not have permission to complete this action.",
                HttpStatusCode.NotFound => "The requested record could not be found.",
                HttpStatusCode.Conflict => "This record is currently in use and cannot be changed as requested.",
                HttpStatusCode.BadRequest => "Please review the submitted information and try again.",
                HttpStatusCode.ServiceUnavailable => "Start the backend API project, then try this request again.",
                _ => "The service could not complete your request. Please try again."
            },
            RequestId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
        };

        context.Result = new ViewResult
        {
            ViewName = "~/Views/Shared/Error.cshtml",
            StatusCode = statusCode,
            ViewData = new ViewDataDictionary<ErrorViewModel>(_metadataProvider, new ModelStateDictionary())
            {
                Model = model
            }
        };
        context.ExceptionHandled = true;
    }
}
