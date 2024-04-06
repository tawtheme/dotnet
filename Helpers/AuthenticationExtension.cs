using BlueCollarEngine.API.Models.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

namespace BlueCollarEngine.API.Helpers
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var secret = config.GetSection("JwtConfig:secret").Value;
            var key = Encoding.ASCII.GetBytes(secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = config.GetSection("JwtConfig:issuer").Value,
                        ValidAudience = config.GetSection("JwtConfig:audience").Value
                    };
                    x.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            APIResponse errorResponse;
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            errorResponse = new APIResponse(context.Response.StatusCode);
                            await context.Response.WriteAsync(errorResponse.ToString());
                        }
                    };
                });
            return services;
        }
    }
}
