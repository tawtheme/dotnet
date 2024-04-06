using BlueCollarEngine.API.DBContext;
using BlueCollarEngine.API.Filters;
using BlueCollarEngine.API.Models.Account;
using BlueCollarEngine.API.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace BlueCollarEngine.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiValidationFilter]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;

        }
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtConfig:issuer"],
                    audience: _configuration["JwtConfig:audience"],
                    expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtConfig:expirationInMinutes"])),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new APIResponse((int)HttpStatusCode.OK, null, new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    role = string.Join(",", userRoles),
                    email = user.UserName
                }));
            }
            return Unauthorized(new APIResponse((int)HttpStatusCode.Unauthorized));
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegsiterModel model)
        {

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            var errors = new List<string>();

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                return BadRequest(new APIResponse((int)HttpStatusCode.BadRequest, null, errors));
            }
            if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = "SuperAdmin" });
            }
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            }

            await _userManager.AddToRoleAsync(user, model.RoleName);
            return Ok(new APIResponse((int)HttpStatusCode.OK));
        }

        [HttpGet("GetClaims"), Authorize]
        public async Task<IActionResult> GetClaims()
        {
            ClaimModel claimObj = new ClaimModel();
            claimObj.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            claimObj.UserName = User.FindFirstValue(ClaimTypes.Name);
            claimObj.Email = User.FindFirstValue(ClaimTypes.Name);
            claimObj.RoleName = User.FindFirstValue(ClaimTypes.Role);
            claimObj.FirstName = "";
            claimObj.LastName = "";
            return Ok(new APIResponse((int)HttpStatusCode.OK, null, claimObj));
        }
    }
}
