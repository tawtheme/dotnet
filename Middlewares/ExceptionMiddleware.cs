using BlueCollarEngine.API.Models.Common;
using BlueCollarEngine.API.Repositories.LoggerRepository;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace BlueCollarEngine.API.Middlewares
{
    public static class ExceptionMiddleware
    {
        public static void ConfigureExceptionMiddleware(this IApplicationBuilder app, IWebHostEnvironment env, ILoggerRepository logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    APIResponse errorResponse;
                    HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)statusCode;
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        if (env.IsDevelopment())
                        {
                            errorResponse = new APIResponse((int)statusCode, contextFeature.Error.Message, contextFeature.Error.StackTrace);
                        }
                        else
                        {
                            errorResponse = new APIResponse((int)statusCode, contextFeature.Error.Message);
                        }
                        logger.LogError($"Something went wrong: {contextFeature.Error}");
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
                    }
                });
            });
        }
    }
}
