using ASP.NETCoreD14.Data.Models;
using ASP.NETCoreD14.DTOs.Auth;
using ASP.NETCoreD14.Srttings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ASP.NETCoreD14.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /*------------------------------------------------------------------*/
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        /*------------------------------------------------------------------*/
        public AuthController(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> options)
        {
            _userManager = userManager;
            _jwtSettings = options.Value;
        }
        /*------------------------------------------------------------------*/
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(registerDto.Email);
            if (user != null)
            {
                return BadRequest("Email is already registered.");
            }

            var applicationUser = new ApplicationUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.UserName
            };

            IdentityResult result = await _userManager.CreateAsync(applicationUser, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Add To Roles
            var roleResult = await _userManager.AddToRoleAsync(applicationUser, "Admin");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            return Ok("User registered successfully");
        }
        /*------------------------------------------------------------------*/
        //[HttpPost]
        //[Route("login")]
        //public ActionResult Login(LoginDto loginDto)
        //{
        //    if (loginDto.Email != "admin" && loginDto.Password != "1234")
        //    {
        //        return Unauthorized();
        //    }

        //    // Generate Token
        //    #region Define Claims
        //    List<Claim> claims = new List<Claim>()
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, "1"),
        //        new Claim(ClaimTypes.Name, "Test User"),
        //        new Claim(ClaimTypes.Role, "Test Role")
        //    };
        //    #endregion

        //    #region Secret Key
        //    var key = "This is Key from Auth Controller";
        //    var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        //    #endregion

        //    #region Generate Token
        //    var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        //    var tokenAsString = new JwtSecurityToken(
        //            claims: claims,
        //            signingCredentials: signingCredentials,
        //            expires: DateTime.UtcNow.AddDays(7)
        //        );

        //    // Encoding the token
        //    var token = new JwtSecurityTokenHandler().WriteToken(tokenAsString);
        //    #endregion

        //    return Ok(token);
        //}
        /*------------------------------------------------------------------*/
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<TokenDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user is null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Check Password
            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("Invalid email or password.");
            }

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"));
            claims.Add(new Claim(ClaimTypes.Email, user.Email!));

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDto = GenerateToken(claims);
            return Ok(tokenDto);
        }
        /*------------------------------------------------------------------*/
        // This method will generate a JWT token based on the provided claims and the JWT settings.
        private TokenDto GenerateToken(List<Claim> claims)
        {
            var keyFromConfiguration = _jwtSettings.SecretKey;
            var keyInBytes = Convert.FromBase64String(keyFromConfiguration);
            var key = new SymmetricSecurityKey(keyInBytes);
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryDateTime = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

            var jwt = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                signingCredentials: signingCredentials,
                expires: expiryDateTime
                );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var tokenDto = new TokenDto(token, _jwtSettings.DurationInMinutes);
            return tokenDto;
        }
        /*------------------------------------------------------------------*/
    }
}
