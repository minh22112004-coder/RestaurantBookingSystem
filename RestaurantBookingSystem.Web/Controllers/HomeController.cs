using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            StatusCode = 500,
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
}
