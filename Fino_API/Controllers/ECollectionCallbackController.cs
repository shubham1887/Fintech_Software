using FintechApp.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CORSProject")]
    public class ECollectionCallbackController : ControllerBase
    {
        private readonly Logger _logger;
        public ECollectionCallbackController() { _logger = new Logger(); }

        [HttpPost("Validate_Response")] public async Task<string> Validate_Response() { string body = ""; using (var reader = new System.IO.StreamReader(Request.Body)) { body = await reader.ReadToEndAsync(); } _logger.Write(new Exception($"ECollectionCallback.Validate_Response: {body}")); return "OK"; }

        [HttpPost("Notify_Response")] public async Task<string> Notify_Response() { string body = ""; using (var reader = new System.IO.StreamReader(Request.Body)) { body = await reader.ReadToEndAsync(); } _logger.Write(new Exception($"ECollectionCallback.Notify_Response: {body}")); return "OK"; }
    }
}
