using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.AEPS;
using Project.Requests.PaymentGateway;
using Project.Response;
using Project.Response.AEPS;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class AEPSController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectActiveAEPS([FromForm] APIRequest aPIRequest)
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
            Response.AEPSSetting.ViewModel response = new Response.AEPSSetting.ViewModel();
            try
            {
                response = new AEPSBal().SelectActiveAEPS(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse SelectMultiActiveAEPS([FromForm] APIRequest aPIRequest)
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

            List<Response.AEPSSetting.ViewModel> response = new List<Response.AEPSSetting.ViewModel>();
            try
            {
                response = new AEPSBal().SelectMultiActiveAEPS(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }




        [HttpPost]
        public APIResponse TransactionStatus([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            TransactionStatusRequest mainRequest;
            KeyDataModel keyDataModel;

            try
            {
                mainRequest = JsonConvert.DeserializeObject<TransactionStatusRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string response = "";
            try
            {
                RandomNumberProcessing.RandomDelay(400, 1400);

                response = new AEPSBal().TransactionStatus(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(response);
        }

        [HttpPost]
        public APIResponse SelectPIDOption([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            KeyDataModel keyDataModel;
            PIDOptionSelect mainRequest;

            string IPAddress, userAgent;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<PIDOptionSelect>(ed.Decrypt(aPIRequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                userAgent = Request.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!keyDataModel.Platform.EqualsIgnoreCase(PlatformCode.Android))
            {
                failureMessage = AEPSMessage.InvalidPlatform;
                return ResponseProcess.GetFailReason(failureMessage);
            }

            PIDOptionResponse response = new PIDOptionResponse();
            try
            {
                response = new AEPSBal().SelectPIDOption(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}