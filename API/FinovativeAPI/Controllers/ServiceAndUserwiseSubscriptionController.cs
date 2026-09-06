using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.ServiceAndUserwiseSubscription;
using Project.Response;
using Project.Response.ServiceAndUserwiseSubscription;
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
    public class ServiceAndUserwiseSubscriptionController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            InsertRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<InsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            InsertResponse response = new InsertResponse();
            try
            {
                response = new ServiceAndUserwiseSubscriptionBal().Insert(keyDataModel.UserID, mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!response.Status.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(response.Message);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            DeleteRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<DeleteRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            try
            {
                failureMessage = new ServiceAndUserwiseSubscriptionBal().Delete(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Message.ServiceAndUserwiseSubscriptionMessage.DeleteSuccess));
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage = "";
            ReportWrapper wrapper = new ReportWrapper();
            try
            {
                wrapper = new ServiceAndUserwiseSubscriptionBal().Select(mainRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse AutoDebitOnOff([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            AutoDebitOnOffRequest Request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                Request = JsonConvert.DeserializeObject<AutoDebitOnOffRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            failureMessage = new ServiceAndUserwiseSubscriptionBal().AutoDebitOnOff(Request);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string returnMessage = Request.IsAutoDebit ? Message.ServiceAndUserwiseSubscriptionMessage.AutoDebitActive : Message.ServiceAndUserwiseSubscriptionMessage.AutoDebitDeActive;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(returnMessage));
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            UpdateRequest Request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                Request = JsonConvert.DeserializeObject<UpdateRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            failureMessage = new ServiceAndUserwiseSubscriptionBal().Update(Request);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Message.ServiceAndUserwiseSubscriptionMessage.UpdateSuccess));
        }

        [HttpPost]
        public APIResponse SelectChargeReport([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage = "";
            ReportWrapper wrapper = new ReportWrapper();
            try
            {
                wrapper = new ServiceAndUserwiseSubscriptionBal().SelectChargeReport(mainRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            if (mainRequest.IsPDFDownload)
            {
                string FileName = "SubscriptionCharge_Report";
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf(FileName, wrapper.list, keyDataModel.UserID);
            }

            if (mainRequest.IsExcelDownload)
            {
                string FileName = "SubscriptionCharge_Report";
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectUserWiseSubscriptionCharge([FromForm] APIRequest aPIRequest)
        {
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

            string failureMessage = "";
            List<UserSubscriptionChargeViewModel> wrapper = new List<UserSubscriptionChargeViewModel>();
            try
            {
                wrapper = new ServiceAndUserwiseSubscriptionBal().SelectUserWiseSubscriptionCharge(keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectPendingSubscriptionCharge([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectPendingRequest pendingRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                pendingRequest = JsonConvert.DeserializeObject<SelectPendingRequest>(ed.Decrypt(aPIRequest.Request));
            }  
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage = "";
            PendingChargeWrapper wrapper = new PendingChargeWrapper();
            try
            {
                wrapper = new ServiceAndUserwiseSubscriptionBal().SelectPendingSubscriptionCharge(pendingRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            if (pendingRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("PendingSubscription Charge Report", wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse ServiceSubscriptionPayment([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            PaymentRequest Request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                Request = JsonConvert.DeserializeObject<PaymentRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            failureMessage = new ServiceAndUserwiseSubscriptionBal().ServiceSubscriptionPayment(Request, keyDataModel.UserID);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Message.ServiceAndUserwiseSubscriptionMessage.PaymentSuccess));
        }

        [HttpPost]
        public APIResponse SelectSubscriptionPaymentReport([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage = "";
            ReportWrapper wrapper = new ReportWrapper();
            try
            {
                wrapper = new UserServiceSubscriptionChargeBal().SelectSubscriptionPaymentReport(mainRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            if (mainRequest.IsExcelDownload)
            {
                string FileName = "SubscriptionPayment_Report";
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }


        [HttpPost]
        public APIResponse IsPaymentDueForService([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            PaymentDueRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<PaymentDueRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            PaymentDueResponse response = new PaymentDueResponse();
            try
            {
                response = new ServiceAndUserwiseSubscriptionBal().IsPaymentDueForService(keyDataModel.UserID, mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            //if (!response.Status.EqualsIgnoreCase(StatusType.Ok))
            //    return ResponseProcess.GetFailReason(response.Message);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                response.Status = StatusType.Failure;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }


        [HttpPost]
        public APIResponse PaymentApprovalByAdmin([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            PaymentApprovalRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<PaymentApprovalRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new ServiceAndUserwiseSubscriptionBal().PaymentApprovalByAdmin(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

      
            return ResponseProcess.GetSucessReason(Message.ServiceAndUserwiseSubscriptionMessage.PaymentApproveSuccess);
        }


        [HttpPost]
        public APIResponse UpdateSubscriptionStatus([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            UpdateStatusRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<UpdateStatusRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new ServiceAndUserwiseSubscriptionBal().UpdateSubscriptionStatus(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(ServiceAndUserwiseSubscriptionMessage.UpdateSuccess);
        }


        [HttpPost]
        public APIResponse SelectServiceCharge([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectServiceChargeRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectServiceChargeRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            ServiceChargeResponse viewModel;
            try
            {
                viewModel = new ServiceAndUserwiseSubscriptionBal().SelectUserWiseSubscriptionChargev1(keyDataModel.UserID, mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (viewModel == null)
                return ResponseProcess.GetFailReason(PaymentGatewayMessage.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
        }

    }
}
