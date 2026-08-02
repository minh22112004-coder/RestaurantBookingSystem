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

        bool emailTaken = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailTaken)
        {
            return Conflict(new
            {
                message = "Email đã được sử dụng."
            });
        }

        bool usernameTaken = await _context.Users
            .AnyAsync(u => u.Username == username);

        if (usernameTaken)
        {
            return Conflict(new
            {
                message = "Tên đăng nhập đã được sử dụng."
            });
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Phone = request.Phone
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        // Gán role mặc định "Customer" (many-to-many qua navigation, không cần bảng UserRole thủ công)
        Role? customerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == RoleNames.Customer);

        if (customerRole is null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = $"Không tìm thấy role '{RoleNames.Customer}'. Hãy chạy seed role trước." }
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
        string email = request.Email.Trim().ToLowerInvariant();

        User? user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Email hoặc mật khẩu không đúng."
            });
        }

        PasswordVerificationResult verifyResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Email hoặc mật khẩu không đúng."
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