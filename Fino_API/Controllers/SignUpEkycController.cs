using FintechApp.Helpers;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpEkycController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public SignUpEkycController(IConfiguration configuration)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        // Public — Initiates eKYC verification during sign-up via external eKYC provider (pending integration)
        [HttpPost("SignupeKYCInitiate")]
        public async Task<APIResponse> SignupeKYCInitiate([FromForm] APIRequest apiRequest)
        {
            return await Task.FromResult(ResponseProcess.GetFailResponse("SignupeKYCInitiate pending: external eKYC provider (OTP initiation) integration not yet implemented."));
        }

        // Public — Validates eKYC OTP response from external eKYC provider during sign-up (pending integration)
        [HttpPost("SignupKYCValidate")]
        public async Task<APIResponse> SignupKYCValidate([FromForm] APIRequest apiRequest)
        {
            return await Task.FromResult(ResponseProcess.GetFailResponse("SignupKYCValidate pending: external eKYC provider (OTP validation) integration not yet implemented."));
        }
    }
}
