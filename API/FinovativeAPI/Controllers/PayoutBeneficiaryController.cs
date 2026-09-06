using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;

using Project.Requests.PayoutBeneficiary;
using Project.Response;
using Project.Response.PayoutBeneficiary;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class PayoutBeneficiaryController : Controller
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Add([FromForm] APIRequest aPIRequest)
        {
            InsertRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<InsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            InsertResponse obj;
            try
            {
                obj = new PayoutBeneficiaryBAL().Insert(mainRequest, keyDataModel.UserID, IPAddress, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (obj == null)
                return ResponseProcess.GetFailReason(failureMessage);


            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIRequest)
        {
            DeleteRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<DeleteRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string response;
            try
            {
                response = new PayoutBeneficiaryBAL().Delete(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!response.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(response);

            return ResponseProcess.GetSucessReason(BeneficiaryMessage.DeleteSuccess);
        }

        [HttpPost]
        public APIResponse VerifyOTP([FromForm] APIRequest aPIRequest)
        {
            UpdateRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UpdateRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            try
            {
                failureMessage = new PayoutBeneficiaryBAL().Update(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
     
            return ResponseProcess.GetSucessReason(Message.BeneficiaryMessage.BeneRegistrationSuccess);
        }

        [HttpPost]
        public APIResponse ResendOTP([FromForm] APIRequest aPIRequest)
        {
            ResendOTPRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<ResendOTPRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            try
            {
                failureMessage = new PayoutBeneficiaryBAL().ResendOTP(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.BeneficiaryMessage.OTPSentOnMobileNumer);
        }

        [HttpPost]
        public APIResponse SelectBeneficiary([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            PayoutBeneficiaryReportRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<PayoutBeneficiaryReportRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            BeneficiaryWrapper wrapper;
            try
            {
                wrapper = new PayoutBeneficiaryBAL().SelectAllBeneficiary(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (wrapper.list == null || wrapper.list.Count <= 0)
                return ResponseProcess.GetFailReason(failureMessage);
            if (mainRequest.IsPDFDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf("Beneficiary_Report", wrapper.list, keyDataModel.UserID);
            }
            if (mainRequest.IsExcelDownload)
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Beneficiary_Report", wrapper.list, keyDataModel.UserID);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse UpdateStatus([FromForm] APIRequest aPIRequest)
        {
            UpdateStatusRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UpdateStatusRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string response;
            try
            {
                response = new PayoutBeneficiaryBAL().UpdateStatus(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!response.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(response);

            return ResponseProcess.GetSucessReason(BeneficiaryMessage.UpdateStatusSuccess);
        }
        [HttpPost]
        public APIResponse PayoutReceipt([FromForm] APIRequest aPIRequest)
        {
            Project.Requests.PayoutBeneficiary.CalculateGstRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.PayoutBeneficiary.CalculateGstRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Project.Response.PayoutBeneficiary.CalculateGstResponse response = new Project.Response.PayoutBeneficiary.CalculateGstResponse();
            string FailureMessage = "";
            try
            {
                (response, FailureMessage) = new PayoutBeneficiaryBAL().PayoutReceipt(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!FailureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(FailureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));

        }

    }
}