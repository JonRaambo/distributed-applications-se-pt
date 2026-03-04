using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarService.Api.Dtos;
using CarService.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CarService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtOptions _jwt;

    private static readonly Dictionary<string, string> Users = new()
    {
        ["admin"] = "admin123",
        ["user"] = "user123"
    };

    public AuthController(IOptions<JwtOptions> jwtOptions) => _jwt = jwtOptions.Value;

    [HttpPost("login")]
    public ActionResult<object> Login(LoginRequest req)
    {
        if (!Users.TryGetValue(req.Username, out var pass) || pass != req.Password)
            return Unauthorized(new { message = "Invalid credentials" });

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, req.Username),
            new(ClaimTypes.Role, req.Username == "admin" ? "Admin" : "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpMinutes),
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiresInMinutes = _jwt.ExpMinutes
        });
    }
}
