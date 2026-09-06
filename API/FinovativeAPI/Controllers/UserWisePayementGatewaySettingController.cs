using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.PaymentGateway;
using Project.Response;
using Project.Response.PaymentGateway;
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
    public class UserWisePayementGatewaySettingController : Controller
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            UserwisePaymentGatewayRequest InsertRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                string request = ed.Decrypt(aPIRequest.Request);
                InsertRequest = JsonConvert.DeserializeObject<UserwisePaymentGatewayRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string Result = "";
            try
            {
                Result = new UserWisePayementGatewaySettingBal().Insert(InsertRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Userwise Gateway Insert", InsertRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Result, InsertRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(Result,
                 keyDataModel.UserID, Result, InsertRequest.ToString());
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            List<UserwiseGatewayViewModel> Details;
            try
            {
                Details = new UserWisePayementGatewaySettingBal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (Details == null || Details.Count <= 0)
            {
                return ResponseProcess.GetFailReason(PaymentGatewayMessage.DataNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Details));
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest aPIRequest)
        {
            SelectByUserwiseIDRequest selectRequest;
            try
            {
                selectRequest = JsonConvert.DeserializeObject<SelectByUserwiseIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            UserwiseGatewayViewModel Details = new UserwiseGatewayViewModel();
            string failureMessage;
            try
            {
                Details = new UserWisePayementGatewaySettingBal().SelectByID(selectRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            if (Details == null)
            {
                return ResponseProcess.GetFailReason(PaymentGatewayMessage.DataNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Details));
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectByUserwiseIDRequest deleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<SelectByUserwiseIDRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                failureMessage = new UserWisePayementGatewaySettingBal().Delete(deleteRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "UserWisePayementGateway Delete", deleteRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "UserWisePayementGateway Delete", deleteRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(PaymentGatewayMessage.DeleteSuccess,
                 keyDataModel.UserID, "UserWisePayementGateway Delete", deleteRequest.ToString());
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIRequest)
        {
            UserwisePaymentGatewayRequest InsertRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                string request = ed.Decrypt(aPIRequest.Request);
                InsertRequest = JsonConvert.DeserializeObject<UserwisePaymentGatewayRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string Result = "";
            try
            {
                Result = new UserWisePayementGatewaySettingBal().Update(InsertRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "UserwisePaymentGatewayRequest ", InsertRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Result, InsertRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(Result,
                 keyDataModel.UserID, Result, InsertRequest.ToString());
        }
    }
}