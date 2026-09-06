using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL.YesBankDMR;
using Project.Processing;
using Project.Requests;
using Project.Requests.AEPS.YesBankAEPS;
using Project.Response;
using Project.Response.AEPS;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class HitachiYesBankAEPSController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Authentication([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            YesBank_AuthenticationRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress, userAgent;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<YesBank_AuthenticationRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

                userAgent = Request.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            YesBank_AuthenticationResponse response = new YesBank_AuthenticationResponse();
            mainRequest.IPAddess = IPAddress;
            try
            {
                response = new HitachiYesBankBAL().Authentication(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                response.Status = StatusType.Failure;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }

            if (response.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse CashWithdrawal([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            YesBank_CashWithdrawalRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress, userAgent;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<YesBank_CashWithdrawalRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

                userAgent = Request.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            AEPSViewModelResponse response = new AEPSViewModelResponse();
            try
            {
                response = new HitachiYesBankBAL().YesBank_CashWithdrawal(mainRequest, keyDataModel.UserID, IPAddress, userAgent, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                response.Status = StatusType.Failure;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            response.Message = Message.UserMessage.CashWithdraw;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse BalanceEnquiry([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            YesBank_BalanceEnquiryRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress, userAgent;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<YesBank_BalanceEnquiryRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

                userAgent = Request.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            AEPSViewModelResponse response = new AEPSViewModelResponse();
            try
            {
                response = new HitachiYesBankBAL().YesBank_BalanceEnquiry(mainRequest, keyDataModel.UserID, IPAddress, userAgent, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                response.Status = StatusType.Failure;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            response.Message = Message.UserMessage.BalanceEnquiry;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse MiniStatement([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            YesBank_MinistatementRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress, userAgent;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<YesBank_MinistatementRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

                userAgent = Request.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            AEPSViewModelResponse response = new AEPSViewModelResponse();
            try
            {
                response = new HitachiYesBankBAL().YesBank_MiniStatement(mainRequest, keyDataModel.UserID, IPAddress, userAgent, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                response.Status = StatusType.Failure;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }

            if (response.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            response.Message = Message.UserMessage.MiniStatement;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

    }
}