using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using BlueCollarEngine.API.Models.Common;
using System.Net;

namespace BlueCollarEngine.API.Filters
{
    public class ApiValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(new APIResponse((int)HttpStatusCode.BadRequest, null, context.ModelState));
            }
            base.OnActionExecuting(context);
        }
    }
}
