using FintechApp.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CORSProject")]
    public class PaySwiffCallbackController : ControllerBase
    {
        private readonly Logger _logger;
        public PaySwiffCallbackController() { _logger = new Logger(); }

        [HttpPost("post_PaySwiff_Response")] public async Task<string> post_PaySwiff_Response() { string body = ""; using (var reader = new System.IO.StreamReader(Request.Body)) { body = await reader.ReadToEndAsync(); } _logger.Write(new Exception($"PaySwiffCallback.post_PaySwiff_Response: {body}")); return "OK"; }
    }
}
