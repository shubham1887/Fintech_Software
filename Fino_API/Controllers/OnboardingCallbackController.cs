using FintechApp.Helpers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CORSProject")]
    public class OnboardingCallbackController : ControllerBase
    {
        private readonly Logger _logger;
        public OnboardingCallbackController() { _logger = new Logger(); }

        [HttpPost("post_Response")] public async Task<string> post_Response() { string body = ""; using (var reader = new System.IO.StreamReader(Request.Body)) { body = await reader.ReadToEndAsync(); } _logger.Write(new Exception($"OnboardingCallback.post_Response: {body}")); return "OK"; }
    }
}
