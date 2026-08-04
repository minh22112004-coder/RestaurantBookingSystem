using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBookingSystem.Features.Authentication.DTOs;
using RestaurantBookingSystem.Features.Authentication.Services;
using RestaurantBookingSystem.Features.Authorization.Constants;
using RestaurantBookingSystem.Models;
using System.Security.Claims;

namespace RestaurantBookingSystem.Features.Authentication.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly RestaurantReservationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthController(
        RestaurantReservationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = new PasswordHasher<User>();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        string username = request.Username.Trim();

        if (username.Length == 0)
            return BadRequest(new { message = "Username cannot contain only whitespace." });

        bool emailTaken = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailTaken)
        {
            return Conflict(new
            {
                message = "The email address is already in use."
            });
        }

        bool usernameTaken = await _context.Users
            .AnyAsync(u => u.Username == username);

        if (usernameTaken)
        {
            return Conflict(new
            {
                message = "The username is already in use."
            });
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Phone = request.Phone?.Trim()
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        // Assign the default Customer role through the many-to-many navigation.
        Role? customerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == RoleNames.Customer);

        if (customerRole is null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = $"Role '{RoleNames.Customer}' was not found. Seed the roles before registering users." }
            );
        }

        user.Roles.Add(customerRole);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        IList<string> roles = new List<string> { customerRole.RoleName };

        string accessToken = _tokenService.CreateToken(user, roles);

        return StatusCode(
            StatusCodes.Status201Created,
            new AuthResponse
            {
                AccessToken = accessToken,
                ExpiresAt = _tokenService.GetExpirationTime(),
                User = new UserResponse
                {
                    Id = user.UserId,
                    Username = user.Username,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? RoleNames.Customer
                }
            }
        );
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        string login = request.Email.Trim();
        string normalizedLogin = login.ToLowerInvariant();

        User? user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u =>
                u.Email == normalizedLogin ||
                u.Username.ToLower() == normalizedLogin);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Email, username, or password is incorrect."
            });
        }

        PasswordVerificationResult verifyResult;
        try
        {
            verifyResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );
        }
        catch (FormatException)
        {
            // Invalid or legacy password hashes must not make the login endpoint return 500.
            verifyResult = PasswordVerificationResult.Failed;
        }

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Email, username, or password is incorrect."
            });
        }

        IList<string> roles = user.Roles
            .Select(r => r.RoleName)
            .ToList();

        string accessToken = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAt = _tokenService.GetExpirationTime(),
            User = new UserResponse
            {
                Id = user.UserId,
                Username = user.Username,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? RoleNames.Customer
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            username = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            roles = User.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .ToList()
        });
    }
}
