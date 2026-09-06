using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.eKYC;
using Project.Response;
using Project.Response.eKYC;
using Project.StaticModel;
using System;


namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class SignUpEkycController
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [AllowAnonymous]
        [HttpPost]
        public APIResponse SignupeKYCInitiate([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            SignUP_eKYCInitiateRequest mainRequest;
            string IPAddress;

            try
            {
                mainRequest = JsonConvert.DeserializeObject<SignUP_eKYCInitiateRequest>(ed.Decrypt(aPIRequest.Request));
                IPAddress = "Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()";
                //IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            SignUp_eKYCInitiateResponse response = new SignUp_eKYCInitiateResponse();
            try
            {
                response = new SignUpEkycBal().SignUPeKYCInitiate(mainRequest, out failureMessage, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(failureMessage);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }


        [AllowAnonymous]
        [HttpPost]
        public APIResponse SignupKYCValidate([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Signup_eKYCValidateRequest mainRequest;

            try
            {
                mainRequest = JsonConvert.DeserializeObject<Signup_eKYCValidateRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            Signup_eKYCValidateResponse response = new Signup_eKYCValidateResponse();
            try
            {
                response = new SignUpEkycBal().SignUpeKYCValidate(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}
