using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestaurantBookingSystem.Web.Authentication;
using RestaurantBookingSystem.Web.ClientServices;
using RestaurantBookingSystem.Web.Contracts;
using RestaurantBookingSystem.Web.Filters;
using RestaurantBookingSystem.Web.Models;

namespace RestaurantBookingSystem.Web.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IJwtSessionService _sessionService;

    public AccountController(IAuthApiClient authApiClient, IJwtSessionService sessionService)
    {
        _authApiClient = authApiClient;
        _sessionService = sessionService;
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_sessionService.Current is not null)
            return RedirectAfterAuthentication(returnUrl);

        return View(new LoginViewModel { ReturnUrl = NormalizeReturnUrl(returnUrl) });
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        model.ReturnUrl = NormalizeReturnUrl(model.ReturnUrl);
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _authApiClient.LoginAsync(
                new LoginRequest { Email = model.Email, Password = model.Password },
                cancellationToken);

            SaveAuthentication(response);
            TempData["SuccessMessage"] = $"Welcome back, {response.User.Username}.";
            return RedirectAfterAuthentication(model.ReturnUrl);
        }
        catch (ApiClientException exception) when (
            exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.StatusCode == HttpStatusCode.Unauthorized
                    ? "Email, username, or password is incorrect."
                    : "The sign-in request is invalid. Please check your details.");
            return View(model);
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            ModelState.AddModelError(
                string.Empty,
                "The backend API is not running. Start the API project and try signing in again.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View(model);
        }
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        if (_sessionService.Current is not null)
            return RedirectAfterAuthentication(null);

        return View(new RegisterViewModel());
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _authApiClient.RegisterAsync(
                new RegisterRequest
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password
                },
                cancellationToken);

            SaveAuthentication(response);
            TempData["SuccessMessage"] = "Registration complete. You are now signed in.";
            return RedirectToAction("Index", "RestaurantView");
        }
        catch (ApiClientException exception) when (
            exception.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.StatusCode == HttpStatusCode.Conflict
                    ? "That email address or username is already in use."
                    : "Registration could not be completed. Please check your details.");
            return View(model);
        }
        catch (ApiClientException exception) when (exception.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            ModelState.AddModelError(
                string.Empty,
                "The backend API is not running. Start the API project and try creating the account again.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View(model);
        }
    }

    [HttpGet("profile")]
    [RequireSessionRole("Customer")]
    public IActionResult Profile()
    {
        var currentUser = _sessionService.Current!;
        return View(new ProfileViewModel(
            currentUser.UserId,
            currentUser.Username,
            currentUser.Email,
            currentUser.Role));
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _sessionService.Clear();
        TempData["SuccessMessage"] = "You have been signed out safely.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("unauthorized")]
    public IActionResult UnauthorizedPage()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            StatusCode = 403,
            Title = "Access denied",
            Message = "Your account does not have permission to open this page."
        });
    }

    private void SaveAuthentication(AuthResponse response) =>
        _sessionService.Save(new AuthSession(
            response.AccessToken,
            response.ExpiresAt,
            response.User.Id,
            response.User.Username,
            response.User.Email,
            response.User.Role));

    private IActionResult RedirectAfterAuthentication(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return _sessionService.Current?.IsAdmin == true
            ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
            : RedirectToAction("Index", "ReservationView");
    }

    private string? NormalizeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : null;
}
