using BlueCollarEngine.API.Models.Common;
using BlueCollarEngine.API.Models.TimeZone;
using BlueCollarEngine.API.Repositories.LoggerRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;

namespace BlueCollarEngine.API.Controllers
{
    //[AllowAnonymous]
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TimeZoneController : ControllerBase
    {
        private ILoggerRepository _loggerRepository;
        private IHostingEnvironment _environment;
        public TimeZoneController(ILoggerRepository loggerRepository, IHostingEnvironment environment)
        {
            _loggerRepository = loggerRepository;
            _environment = environment;
        }
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            string fullPath = Path.Combine(_environment.ContentRootPath, "Utilities/timezones.json");
            var jsonData = System.IO.File.ReadAllText(fullPath);
            var model = JsonConvert.DeserializeObject<List<TimeZoneModel>>(jsonData);            
            return Ok(new APIResponse((int)HttpStatusCode.OK, null, model));
        }
    }
}
