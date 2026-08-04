using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Controllers;

[Route("error")]
public sealed class ErrorController : Controller
{
    [HttpGet("status")]
    public IActionResult StatusCodePage(int code)
    {
        Response.StatusCode = code;
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            StatusCode = code,
            Title = code == 404 ? "Page not found" : "The request could not be completed",
            Message = code == 404
                ? "The page you requested does not exist or has been moved."
                : "Please return to the previous page and try again."
        });
    }
}
