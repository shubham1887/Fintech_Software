using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.PinGeneration;
using Project.Response;
using Project.Response.PinGeneration;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class PinGenerationController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> GeneratePinOTP([FromForm] APIRequest aPIrequest)
        {
            Requests.PinGeneration.PinGenerationRequest updateRequest;
            try
            {
                updateRequest = JsonConvert.DeserializeObject<Requests.PinGeneration.PinGenerationRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage;

            try
            {
                failureMessage = await new PinGenerationBAL().GeneratePinOTP(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.UserMessage.OTPsendSuccess);
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> ResendPinOTP([FromForm] APIRequest aPIrequest)
        {
            Requests.PinGeneration.PinGenerationRequest updateRequest;
            try
            {
                updateRequest = JsonConvert.DeserializeObject<Requests.PinGeneration.PinGenerationRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage;

            try
            {
                failureMessage = await new PinGenerationBAL().ResendOTP(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.UserMessage.OTPsendSuccess);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> VerifyPinOTP([FromForm] APIRequest aPIrequest)
        {
            Requests.PinGeneration.PinVerifyRequest updateRequest;
            try
            {
                updateRequest = JsonConvert.DeserializeObject<Requests.PinGeneration.PinVerifyRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage;
            VerifyOTPResponse response = null;
            
            try
            {
                response = new PinGenerationBAL().VerifyPinOTP(updateRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            response.Message = Message.UserMessage.SignUpVerifyOTP;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> ForgotMpinGenerateOTP([FromForm] APIRequest aPIrequest)
        {
            ForgotMpinRequest forgotPasswordRequest;
            try
            {
                forgotPasswordRequest = JsonConvert.DeserializeObject<ForgotMpinRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage;
            try
            {
                failureMessage = await new PinGenerationBAL().ForgotMpinGenerateOTP(forgotPasswordRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.UserMessage.UserSuccess);
        }
    }
}
