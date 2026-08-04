using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RestaurantBookingSystem.Web.Authentication;

namespace RestaurantBookingSystem.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireSessionRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;

    public RequireSessionRoleAttribute(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        _role = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var sessionService = context.HttpContext.RequestServices.GetRequiredService<IJwtSessionService>();
        var currentUser = sessionService.Current;

        if (currentUser is null)
        {
            var request = context.HttpContext.Request;
            var returnUrl = $"{request.PathBase}{request.Path}{request.QueryString}";
            context.Result = new RedirectToActionResult(
                "Login",
                "Account",
                new { area = string.Empty, returnUrl });
            return;
        }

        if (!string.Equals(currentUser.Role, _role, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult(
                "UnauthorizedPage",
                "Account",
                new { area = string.Empty });
        }
    }
}
