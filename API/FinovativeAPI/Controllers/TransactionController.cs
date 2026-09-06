using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using project.Requests.Transaction;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Processing.VendorPayProcessing;
using Project.Requests;
using Project.Requests.Transaction;
using Project.Response;
using Project.Response.AEPS;
using Project.Response.TransactionMaster;
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
    public class TransactionController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();


        [HttpPost]
        public APIResponse HourlyPerformance([FromForm] Requests.APIRequest aPIRequest)
        {

            KeyDataModel? keyDataModel;
            try
            {

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {

                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            List<Response.TransactionMaster.HourlyViewModel> response = new TransactionBal().HourlyPerformance();


            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse AllProviderWisePerformance([FromForm] Requests.APIRequest aPIRequest)
        {
            ProviderWisePerformanceGraphRequest mainRequest;
            KeyDataModel? keyDataModel;
            try
            {

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<ProviderWisePerformanceGraphRequest>(ed.Decrypt(aPIRequest.Request));

            }
            catch (Exception ex)
            {

                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            List<project.Response.TransactionMaster.AllProviderWisePerformenceViewModel> response = new TransactionBal().AllProviderWisePerformance(mainRequest);


            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse ProviderWisePerformance([FromForm] Requests.APIRequest aPIRequest)
        {
            ProviderWisePerformanceGraphRequest mainRequest;
            KeyDataModel? keyDataModel;
            try
            {

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<ProviderWisePerformanceGraphRequest>(ed.Decrypt(aPIRequest.Request));

            }
            catch (Exception ex)
            {

                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            List<Response.TransactionMaster.Last12hrViewModel> response = new TransactionBal().ProviderWisePerformance(mainRequest);


            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse Recharge([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            TransactionResponse response = new TransactionResponse();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            RechargeRequest rechargeRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            string userAgent = "";
            try
            {
                rechargeRequest = JsonConvert.DeserializeObject<RechargeRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                userAgent = Request.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.Status = StatusType.Failure;
                response.Message = Messages.ExceptionOccured;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            try
            {
                response = new TransactionBal().Recharge(rechargeRequest, IPAddress, userAgent, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.Status = StatusType.Process;
                response.Message = TransactionMessage.TransactionInProcess;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            new FCMSelection().SendTransactionMessage(keyDataModel.UserID, rechargeRequest, response.Status);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
        [HttpPost]
        public APIResponse UpdateStatusManually([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            UpdateManualRequest UpdateRequest;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                UpdateRequest = JsonConvert.DeserializeObject<UpdateManualRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string Action = UserAction.Update_Status;
            try
            {
                failureMessage = new TransactionBal().UpdateStatusManually(UpdateRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, Action, UpdateRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Action, UpdateRequest.ToString());
            return ResponseProcess.GetSucessReason(TransactionMessage.UpdateManual,
                 keyDataModel.UserID, Action, UpdateRequest.ToString(), UpdateRequest);
        }
        [HttpPost]
        public APIResponse Last12hrPerformance([FromForm] Requests.APIRequest aPIRequest)
        {

            KeyDataModel? keyDataModel;
            try
            {

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {

                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            List<Response.TransactionMaster.Last12hrViewModel> response = new TransactionBal().Last12hrPerformance();



            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse UpdateStatusManuallyPenidngProvider([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            UpdateManualRequest UpdateRequest;
            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                UpdateRequest = JsonConvert.DeserializeObject<UpdateManualRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))

                return ResponseProcess.GetFailReason(failureMessage);

            string Action = UserAction.Update_Status;
            try
            {
                failureMessage = new TransactionBal().UpdateStatusManuallyPenidngProvider(UpdateRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, Action, UpdateRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))

                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Action, UpdateRequest.ToString());

            return ResponseProcess.GetSucessReason(TransactionMessage.UpdateManual,
                 keyDataModel.UserID, Action, UpdateRequest.ToString(), UpdateRequest);
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SelectByIDRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            SelectByIDResponse Model = new SelectByIDResponse();
            try
            {
                Model = new TransactionBal().SelectByID(mainRequest.ID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Model));
        }

        [HttpPost]
        public APIResponse Resend([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            ResendRequest mainRequest;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<ResendRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().ResendTransaction(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Resend", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "Resend", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.ResendSuccess,
                 keyDataModel.UserID, "Resend", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse ShiftRecharge([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            ShiftRequest mainRequest;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<ShiftRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().ShiftTransaction(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "ShiftRecharge", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "ShiftRecharge", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.ShiftSuccess,
                keyDataModel.UserID, "ShiftRecharge", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse ShiftAll([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<ShiftRequest> mainRequest;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<List<ShiftRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                failureMessage = new TransactionBal().ShiftAllTransaction(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "ShiftALLRecharge", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "ShiftALLRecharge", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.ShiftSuccess,
                keyDataModel.UserID, "ShiftALLRecharge", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse StatusCheckAll([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<SuccessFailAllRequest> list;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                list = JsonConvert.DeserializeObject<List<SuccessFailAllRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().StatusCheckAll(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "StatusCheckAll", "StatusCheckAll all pending");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "StatusCheckAll", "StatusCheckAll all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.StatusCheckAllSuccess,
                 keyDataModel.UserID, "StatusCheckAll", "StatusCheckAll all pending");
        }

        [HttpPost]
        public APIResponse RefundAll([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<SuccessFailAllRequest> list;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                list = JsonConvert.DeserializeObject<List<SuccessFailAllRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().RefundAll(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "RefundAll", "Refund all pending");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "RefundAll", "Refund all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.RefundAllSuccess,
                 keyDataModel.UserID, "RefundAll", "Refund all pending");
        }

        [HttpPost]
        public APIResponse RefundAllProviderPenidng([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<SuccessFailAllRequest> list;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                list = JsonConvert.DeserializeObject<List<SuccessFailAllRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().RefundAllProviderPenidng(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "RefundAll", "Refund all pending");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "RefundAll", "Refund all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.RefundAllSuccess,
                 keyDataModel.UserID, "RefundAll", "Refund all pending");
        }

        [HttpPost]
        public APIResponse DMTTransactionRefund([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SuccessFailAllRequest list;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                list = JsonConvert.DeserializeObject<SuccessFailAllRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().DMTTransactionRefund(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "RefundAll", "Refund all pending");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "RefundAll", "Refund all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.RefundAllSuccess,
                 keyDataModel.UserID, "RefundAll", "Refund all pending");
        }

        [HttpPost]
        public APIResponse ResendAll([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<ResendRequest> mainRequest;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<List<ResendRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                failureMessage = new TransactionBal().ResendAllTransactions(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Resend All", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "Resend All", mainRequest.ToString());

            return ResponseProcess.GetSucessReason(TransactionMessage.ResendSuccess,
                 keyDataModel.UserID, "Resend", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse SuccessAll([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<SuccessFailAllRequest> list;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                list = JsonConvert.DeserializeObject<List<SuccessFailAllRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().SuccessAll(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "SuccessAll", "Success all pending");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "SuccessAll", "Success all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.SuccessAllSuccess,
                 keyDataModel.UserID, "SuccessAll", "Success all pending");
        }

        [HttpPost]
        public APIResponse SuccessAllProviderPenidng([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<SuccessFailAllRequest> list;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                list = JsonConvert.DeserializeObject<List<SuccessFailAllRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().SuccessAllProviderPenidng(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "SuccessAll", "Success all pending");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "SuccessAll", "Success all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.SuccessAllSuccess,
                 keyDataModel.UserID, "SuccessAll", "Success all pending");
        }

        [HttpPost]
        public APIResponse Payout([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            PayoutRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<PayoutRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            TransactionResponse response;
            try
            {
                response = new PayoutProcessing().Transfer(mainRequest, IPAddress, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse PayoutV1([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            PayoutRequestV1 mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<PayoutRequestV1>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            TransactionResponse response;
            try
            {
                response = new Processing.PayoutProcessingV1.PayoutProcessingV1().Transfer(mainRequest, IPAddress, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse MoneyTransfer([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            MoneyTransferRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "", userAgent = "";

            try
            {
                mainRequest = JsonConvert.DeserializeObject<MoneyTransferRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                userAgent = Request.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            DMTTransactionResponse response;
            try
            {
                mainRequest.ProviderID = -99;

                string ActiveDMT = new DMTSettingsBal().SelectActiveDMT();

                // below dmt, those dmt have, sender and bene in managed in finno. project, because they only have transfer and status check method
                if (ActiveDMT.EqualsIgnoreCase("PDMT"))
                {
                    mainRequest.ProviderID = Company.PaytmProviderID;
                }
                else if (ActiveDMT.EqualsIgnoreCase("OpenBankDMT"))
                {
                    mainRequest.ProviderID = Company.OpenBankProviderID;
                }
                else if (ActiveDMT.EqualsIgnoreCase("ICashDMT"))
                {
                    mainRequest.ProviderID = Company.ICashProviderID;
                }
                else if (ActiveDMT.EqualsIgnoreCase("ICICI_Composite"))
                {
                    mainRequest.ProviderID = Company.ICICIProviderID;
                }
                else if (ActiveDMT.EqualsIgnoreCase("PayU"))
                {
                    mainRequest.ProviderID = Company.PayUProviderID;
                }
                else if (ActiveDMT.EqualsIgnoreCase("EzyPay"))
                {
                    mainRequest.ProviderID = Company.EzyPayProviderID;
                }
                else if (ActiveDMT.EqualsIgnoreCase("EzyPayPaytm"))
                {
                    mainRequest.ProviderID = Company.EzyPayPaytmProviderID;
                }
                else if (ActiveDMT.EqualsIgnoreCase("HyptoDMT"))
                {
                    mainRequest.ProviderID = Company.HyptoProviderID;
                }

                mainRequest.Through = new BrowserUserAgentBal().GetThrough(userAgent);

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

        [HttpPost]
        public APIResponse GetDMTTransctionStatus([FromForm] APIRequest aPIRequest)
        {
            DMTTransctionStatusRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<DMTTransctionStatusRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            Response.TransactionMaster.DMTTransactionResponse obj;
            try
            {
                obj = new DMTProcessing().DMTTransactionStatusCheck(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.SenderMessage.InvalidSender);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse GetBBPSTransctionStatus([FromForm] APIRequest aPIRequest)
        {
            DMTTransctionStatusRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<DMTTransctionStatusRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            Response.ProviderResponse.ProviderReturnResponse obj;
            try
            {
                obj = new BBPSProcessings().BBPSTransactionStatusCheck(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.SenderMessage.InvalidSender);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse GetPayoutTransactionStatus([FromForm] APIRequest aPIRequest)
        {
            PayoutTransctionStatusRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<PayoutTransctionStatusRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.TransactionMaster.TransactionResponse obj;
            try
            {
                obj = new PayoutProcessing().PayoutTransactionStatusCheck(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.SenderMessage.InvalidSender);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse GetVendorPayTransactionStatus([FromForm] APIRequest aPIRequest)
        {
            SmartPayTransctionStatusRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SmartPayTransctionStatusRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.TransactionMaster.TransactionResponse obj;
            try
            {
                obj = new VendorPayProcessing().VendorPayTransactionStatusCheck(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.SenderMessage.InvalidSender);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse GetAEPSTransactionStatus([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Requests.Transaction.SelectByIDRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Transaction.SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            AEPSViewModelResponse response = new AEPSViewModelResponse();
            try
            {
                response = new AEPSBal().StatusCheck(mainRequest, keyDataModel.UserID, IPAddress, out failureMessage);
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
        public APIResponse GetCreditCardTransactionStatus([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Requests.Transaction.SelectByIDRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Transaction.SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.TransactionMaster.CreditCardTransactionResponse response = new Response.TransactionMaster.CreditCardTransactionResponse();
            try
            {
                response = new CreditCardPaymentBAL().StatusCheck(mainRequest, keyDataModel.UserID, out failureMessage);
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
        public APIResponse GetCashDespositTransactionStatus([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Requests.Transaction.SelectByIDRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Transaction.SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.CashDeposit.CashDepositTransactionResponse response = new Response.CashDeposit.CashDepositTransactionResponse();
            try
            {
                response = new CashDepositBAL().StatusCheck(mainRequest, keyDataModel.UserID, out failureMessage);
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
        public APIResponse GetMicroATMTransactionStatus([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Requests.Transaction.SelectByIDRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Transaction.SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.MicroATM.StatusCheckResponse response = new Response.MicroATM.StatusCheckResponse();
            try
            {
                response = new MicroATMBal().StatusCheckImprove(mainRequest, keyDataModel.UserID, out failureMessage);
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
        public APIResponse GetWalleTopUpTransactionStatus([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Requests.Transaction.SelectByIDRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Transaction.SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.WalletTopUp.WalletTopUpTransactionResponse response = new Response.WalletTopUp.WalletTopUpTransactionResponse();
            try
            {
                response = new WalletTopUpBAL().StatusCheck(mainRequest, keyDataModel.UserID, out failureMessage);
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
        public APIResponse GetDMTTransctionStatusCheckAll([FromForm] APIRequest aPIRequest)
        {
            List<SuccessFailAllRequest> list;
            KeyDataModel keyDataModel;
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                list = JsonConvert.DeserializeObject<List<SuccessFailAllRequest>>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().GetDMTTransctionStatusCheckAll(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "StatusCheckAll", "StatusCheckAll all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.StatusCheckAllSuccess,
                 keyDataModel.UserID, "StatusCheckAll", "StatusCheckAll all pending");
        }

        [HttpPost]
        public APIResponse GetRechargeTransctionStatus([FromForm] APIRequest aPIRequest)
        {
            TransctionStatusRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TransctionStatusRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string Result = "";
            try
            {
                Result = new StatusCheckProcessing().RechargeTransactionStatusCheck(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (Result == null || Result == "")
                return ResponseProcess.GetFailReason(Message.TransactionMessage.TransactionNotFound);

            return ResponseProcess.GetSucessReason(Result);
        }

        [HttpPost]
        public APIResponse DMTTransactionResend([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            DMTTransactionResendRequest dmtRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                dmtRequest = JsonConvert.DeserializeObject<DMTTransactionResendRequest>(ed.Decrypt(aPIRequest.Request));

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));

                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.TransactionMaster.DMTTransactionResponse response;
            try
            {
                response = new DMTProcessing().DMTTransactionResend(dmtRequest, IPAddress, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse DMTTransactionResendAll([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<DMTTransactionResendRequest> dmtRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                dmtRequest = JsonConvert.DeserializeObject<List<DMTTransactionResendRequest>>(ed.Decrypt(aPIRequest.Request));

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));

                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.TransactionMaster.DMTTransactionResponse response;
            try
            {
                response = new DMTProcessing().DMTTransactionResendAll(dmtRequest, IPAddress, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse SelectTransferModeByIFSC([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            SelectTransferModeByIFSCRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectTransferModeByIFSCRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<Response.DMTSettings.TransferModeModel> response = null;
            try
            {
                response = new DMTProcessing().SelectTransferModeByIFSC(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse VerifyOTPRefund([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Requests.ePocketDMT.VerifyOTPRefundRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.ePocketDMT.VerifyOTPRefundRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            TransactionResponse response;
            try
            {
                response = new BeneficiaryBAL().VerifyOTPRefund(mainRequest, keyDataModel, out failureMessage);
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

        [HttpPost]
        public APIResponse VerifyOTPRefundForCreditCard([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Requests.ePocketDMT.VerifyOTPRefundRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.ePocketDMT.VerifyOTPRefundRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            TransactionResponse response;
            try
            {
                response = new CreditCardPaymentBAL().VerifyOTPRefundForCreditCard(mainRequest, keyDataModel, out failureMessage);
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

        [HttpPost]
        public APIResponse SelectOperatorByConsumerNumber([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            SelectOperatorByConsumerNumberRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectOperatorByConsumerNumberRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            OperatorViewModel response = null;
            try
            {
                response = new TransactionBal().SelectOperatorByConsumerNumber(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse CMSRefundAll([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<SuccessFailAllRequest> list;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                list = JsonConvert.DeserializeObject<List<SuccessFailAllRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new TransactionBal().CMSRefundAll(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "CMSRefundAll", "Refund all pending");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "CMSRefundAll", "Refund all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.RefundAllSuccess,
                 keyDataModel.UserID, "CMSRefundAll", "Refund all pending");
        }

        [HttpPost]
        public APIResponse DMTCMSTransactionResendAll([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<DMTTransactionResendRequest> dmtRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                dmtRequest = JsonConvert.DeserializeObject<List<DMTTransactionResendRequest>>(ed.Decrypt(aPIRequest.Request));

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));

                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.TransactionMaster.DMTTransactionResponse response;
            try
            {
                response = new DMTProcessing().DMTCMSTransactionResendAll(dmtRequest, IPAddress, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse GetTransactionRefund([FromForm] APIRequest aPIRequest)
        {
            GetTransactionRefundRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<GetTransactionRefundRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.TransactionMaster.DMTTransactionResponse obj;

            try
            {
                obj = new DMTProcessing().GetTransactionRefundRequest(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.SenderMessage.InvalidSender);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse GetUPITransactionStatus([FromForm] APIRequest aPIRequest)
        {
            UPITransctionStatusRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UPITransctionStatusRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.UPITransfer.UPIMoneyTransferResponse obj;
            try
            {
                obj = new UPITransferProcessing().UPITransactionStatusCheck(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReason(Message.SenderMessage.InvalidSender);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse UPITransactionResendAll([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<UPITransactionResendRequest> upiRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                upiRequest = JsonConvert.DeserializeObject<List<UPITransactionResendRequest>>(ed.Decrypt(aPIRequest.Request));

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));

                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.UPITransfer.UPIMoneyTransferResponse response;
            try
            {
                response = new UPITransferProcessing().UPITransactionResendAll(upiRequest, IPAddress, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(response));
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse UPIStatusCheckAll([FromForm] APIRequest aPIRequest)
        {
            List<SuccessFailAllRequest> list;
            KeyDataModel keyDataModel;
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                list = JsonConvert.DeserializeObject<List<SuccessFailAllRequest>>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new UPITransferProcessing().GetUPITransctionStatusCheckAll(list, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "StatusCheckAll", "StatusCheckAll all pending");
            }

            return ResponseProcess.GetSucessReason(TransactionMessage.StatusCheckAllSuccess,
                 keyDataModel.UserID, "StatusCheckAll", "StatusCheckAll all pending");
        }

        [HttpPost]
        public APIResponse VendorPay([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            VendorPayRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<VendorPayRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            TransactionResponse response;
            try
            {
                response = new Processing.VendorPayProcessing.VendorPayProcessing().Transfer(mainRequest, IPAddress, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse GetLeadGenerationTransactionStatus([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            Requests.Transaction.SelectByIDRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Transaction.SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Response.LeadGeneration.LeadGenerationTransactionResponse response = new Response.LeadGeneration.LeadGenerationTransactionResponse();
            try
            {
                response = new LeadGenerationBal().StatusCheck(mainRequest, keyDataModel.UserID, out failureMessage);
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




    }
}