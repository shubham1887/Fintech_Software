using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL.YesBankDMR;
using Project.Processing;
using Project.Requests;
using Project.Requests.Sender;
using Project.Requests.Transaction;
using Project.Response;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Controllers
{
    [CustomAuthorization]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class YesBankDMTController : Controller
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SenderRegistration([FromForm] APIRequest aPIRequest)
        {
            InsertRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Sender.InsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Project.Response.Sender.RegistrationResponseViewModel resp;
            try
            {
                resp = new SenderBAL().SenderRegistration(mainRequest, keyDataModel.UserID, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!resp.status.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(resp.Message);  

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(resp));
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            SelectRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.Sender.ViewModel obj;
            string failureMessage;
            try
            {
                obj = new SenderBAL().Select(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.SenderMessage.InvalidSender);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse AddBeneficiary([FromForm] APIRequest aPIRequest)
        {
            Requests.Beneficiary.InsertRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Beneficiary.InsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Project.Response.Beneficiary.RegistrationResponseViewModel resp;
            try
            {
                resp = new BeneficiaryBAL().AddBeneficiary(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!resp.Status.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(resp.Message);
        
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(resp));
        }
          
        [HttpPost]
        public APIResponse BeneficiaryOTPValidate([FromForm] APIRequest aPIRequest)
        {
            Requests.Beneficiary.BeneficiaryVerifyOTPRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Beneficiary.BeneficiaryVerifyOTPRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            YesBankCore.Response.MoneyTransfer.AddBeneResponse beneficiary = new();
            string failureMessage;
            try
            {
                beneficiary = new BeneficiaryBAL().BeneficiaryOTPValidate(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(beneficiary));
        }

        [HttpPost]
        public APIResponse ViewBeneficiary([FromForm] APIRequest aPIRequest)
        {
            SelectRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<Response.Beneficiary.ViewModel> obj;
            string failureMessage;
            try
            {
                obj = new BeneficiaryBAL().Select(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.SenderMessage.InvalidSender);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse DeleteBeneficiary([FromForm] APIRequest aPIRequest)
        {
            Requests.Beneficiary.DeleteRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Beneficiary.DeleteRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            YesBankCore.Response.MoneyTransfer.DeActivateBeneResponse obj;

            try
            {
                obj = new BeneficiaryBAL().DeleteBeneficiary(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.BeneficiaryMessage.DeleteFailure);

            if (obj.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                return ResponseProcess.GetFailReason(obj.Message);
            }

            return ResponseProcess.GetSucessReason(obj.Message);
        }

        [HttpPost]
        public APIResponse RefundSendOTP([FromForm] APIRequest aPIRequest)
        {
            Requests.Beneficiary.RefundSendOTPRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Beneficiary.RefundSendOTPRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Project.Response.Beneficiary.RefundResponse resp;
            try
            {
                resp = new DMTRefundBAL().RefundSendOTP(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!resp.Status.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(resp.Message);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(resp));
        }
    
        [HttpPost]
        public APIResponse MoneyTransfer([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            MoneyTransferRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<MoneyTransferRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            DMTTransactionResponse response;
            mainRequest.ProviderID = Company.YesBankProviderID;
            try
            {
                response = new DMTProcessing().MoneyTransfer(mainRequest, IPAddress, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

    }
}