using BlueCollarEngine.API.DBContext;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlueCollarEngine.API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public JwtMiddleware(RequestDelegate next, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task Invoke(HttpContext context, ApplicationDbContext dataContext)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
                await attachAccountToContext(context, dataContext, token);
            await _next(context);
        }
        private async Task attachAccountToContext(HttpContext context, ApplicationDbContext dataContext, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"].ToString());
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidAudience = _httpContextAccessor.HttpContext.Request.Headers["scope"].ToString(),
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userName = (jwtToken.Claims.Where(c => c.Type == ClaimTypes.Name).First().Value);
                var roleName = (jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList());
                // attach account to context on successful jwt validation
                context.Items["User"] = userName;
                context.Items["Role"] = roleName;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
