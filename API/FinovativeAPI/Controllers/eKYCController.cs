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
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class eKYCController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse VerifyPan([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            VerifyPanRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<VerifyPanRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            VerifyPanResponse response = new VerifyPanResponse();
            try
            {
                response = new eKYCBal().VerifyPan(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

         




        [HttpPost]
        public APIResponse YesBankEkycInitiate([FromForm] APIRequest aPIRequest)
        {

            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            KeyDataModel keyDataModel;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            Yes_eKYCInitiateResponse response = new Yes_eKYCInitiateResponse();
            try
            {
                response = new eKYCBal().YesBankEkycInitiate(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!response.Message.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            response.Message = Message.eKYCMessage.SendOTP;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
        [HttpPost]
        public APIResponse YesBankEkycValidate([FromForm] APIRequest aPIRequest)
        {

            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            KeyDataModel keyDataModel;
            Requests.eKYC.eKYCValidateRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.eKYC.eKYCValidateRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            Yes_eKYCValidateResponse response = new Yes_eKYCValidateResponse();
            try
            {
                response = new eKYCBal().YesBankEkycValidate(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!response.Message.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(response.Message);
            }
            response.Message = Message.eKYCMessage.VerifyedOTP;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
        [HttpPost]
        public APIResponse YesBankBioMatricVerification([FromForm] APIRequest aPIRequest)
        {

            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            KeyDataModel keyDataModel;
            Project.Requests.eKYC.YesBank_AadharBiometricRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.eKYC.YesBank_AadharBiometricRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            Yes_eKYCValidateResponse response = new Yes_eKYCValidateResponse();
            try
            {
                response = new eKYCBal().YesBankBioMatricVerification(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!response.Message.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(response.Message);
            }
            response.Message = Message.eKYCMessage.EkycSuccessful;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}