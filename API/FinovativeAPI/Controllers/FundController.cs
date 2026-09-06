using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.Fund;
using Project.Response;
using Project.Response.Fund;
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
    public class FundController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse RequestInsert([FromForm] APIRequest aPIRequest)
        {
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

            string failureMessage;
            try
            {
                failureMessage = new FundBal().RequestInsert(mainRequest, keyDataModel.UserID, out int Result);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.FundMessage.TransactionRequestSuccess);
        }

        [HttpPost]
        public APIResponse SelectRequestByFilter([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<RequestViewModel> returnresponse;
            string failureMessage;
            try
            {
                returnresponse = new FundBal().SelectByFilter(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (returnresponse == null)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            if (returnresponse.Count <= 0)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(returnresponse));
        }

        [HttpPost]
        public APIResponse SelectHeaderWithRequestByFilter([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            RequestWrapper wrapper;
            string failureMessage;
            try
            {
                wrapper = new FundBal().SelectHeaderWithRequestByFilter(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (wrapper == null)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectRequestByID([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            FundRequestSelectByIDRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<FundRequestSelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            RequestViewModel returnresponse;
            try
            {
                returnresponse = new FundBal().SelectByID(keyDataModel.UserID, mainRequest.FundTransactionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (returnresponse == null)
            {
                return ResponseProcess.GetFailReason(Messages.DataNotFound);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(returnresponse));
        }

        [HttpPost]
        public APIResponse AcceptReject([FromForm] APIRequest aPIRequest)
        {
            Requests.Fund.UpdateRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Requests.Fund.UpdateRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new FundBal().AcceptReject(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (mainRequest.Status.EqualsIgnoreCase("Reject"))
                return ResponseProcess.GetSucessReason(Message.FundMessage.RejectSuccess);

            return ResponseProcess.GetSucessReason(Message.FundMessage.AcceptSuccess);
        }

        [HttpPost]
        public APIResponse Transfer([FromForm] APIRequest aPIRequest)
        {
            TransferInsertRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TransferInsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            try
            {
                failureMessage = new FundBal().Transfer(mainRequest, keyDataModel.UserID, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.FundMessage.FundTransactionSucsess);
        }

        [HttpPost]
        public APIResponse SelectLast10ByReceiverID([FromForm] APIRequest aPIRequest)
        {
            SelectLast10ByReceiverRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectLast10ByReceiverRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<RequestViewModel> returnresponse;
            try
            {
                returnresponse = new FundBal().SelectLast10ByReceiverID(keyDataModel.UserID, mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (returnresponse == null)

                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            if (returnresponse.Count <= 0)

                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(returnresponse));
        }

        [HttpPost]
        public APIResponse SelectLast10Request([FromForm] APIRequest aPIRequest)
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

            List<RequestViewModel> returnresponse;
            try
            {
                returnresponse = new FundBal().SelectLast10Request(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (returnresponse == null)

                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            if (returnresponse.Count <= 0)

                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(returnresponse));
        }

        [HttpPost]
        public APIResponse TransferReport([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            TransferReportRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<TransferReportRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            TransferReportWrapper wrapper;
            string failureMessage;
            try
            {
                wrapper = new FundBal().SelectTransferReport(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (wrapper == null)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            if (wrapper.Count <= 0)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);
            if (mainRequest.IsPDFDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf("Fund_Report", wrapper.data, keyDataModel.UserID);
                wrapper.data = null;
            }
            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Fund_Report", wrapper.data, keyDataModel.UserID);
                wrapper.data = null;
            }
                return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse ReverseRequest([FromForm] APIRequest aPIRequest)
        {
            InsertReverseRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<InsertReverseRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new FundBal().ReverseRequestInsert(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.FundMessage.ReverseTransactionRequestSuccess);
        }

        [HttpPost]
        public APIResponse ReverseReport([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            TransferReportRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<TransferReportRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            TransferReportWrapper returnresponse;
            string failureMessage;
            try
            {
                returnresponse = new FundBal().SelectReverseReport(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (returnresponse == null)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            if (returnresponse.Count <= 0)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(returnresponse));
        }

        [HttpPost]
        public APIResponse Reverse([FromForm] APIRequest aPIRequest)
        {
            ReverseInsertRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<ReverseInsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            try
            {
                failureMessage = new FundBal().Reverse(mainRequest, keyDataModel.UserID, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.FundMessage.FundReverseSucsess);
        }

        [HttpPost]
        public APIResponse GenerateFundReverseOTP([FromForm] APIRequest aPIRequest)
        {
            GenerateFundReverseOTPRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<GenerateFundReverseOTPRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;

            GenerateFundReverseOTPResponse response;
            try
            {
                response = new FundBal().GenerateFundReverseOTP(mainRequest, keyDataModel.UserID, out failureMessage);
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
        public APIResponse ReverseWithOTP([FromForm] APIRequest aPIRequest)
        {
            ReverseInsertRequest mainRequest;
            KeyDataModel keyDataModel;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<ReverseInsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            try
            {
                failureMessage = new FundBal().ReverseWithOTP(mainRequest, keyDataModel.UserID, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.FundMessage.FundReverseSucsess);
        }

        [HttpPost]
        public APIResponse SelectLast10ForReverseTransaction([FromForm] APIRequest aPIRequest)
        {
            SelectLast10ForReverseTransactionRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectLast10ForReverseTransactionRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<RequestViewModel> returnresponse;
            try
            {
                returnresponse = new FundBal().SelectLast10ForReverseTransaction(mainRequest, keyDataModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (returnresponse == null)

                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            if (returnresponse.Count <= 0)

                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(returnresponse));
        }

        //[HttpPost]
        //public async Task<APIResponse> SelectTrnasferForDashboard([FromForm] APIRequest encRequest)
        //{
        //    KeyDataModel keyDataModel;

        //    try
        //    {
        //        keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
        //    }
        //    catch (Exception ex)
        //    {
        //        new Logger().Write(ex);
        //        return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
        //    }

        //    string failureMessage = keyDataModel.IsValidModalState();
        //    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
        //        return ResponseProcess.GetFailReason(failureMessage);

        //    DashboardFundViewModel response = null;

        //    try
        //    {
        //        (failureMessage, response) = await new FundBal().SelectTransferForDashboard(keyDataModel.UserID);
        //    }
        //    catch (Exception ex)
        //    {
        //        new Logger().Write(ex);
        //        return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
        //    }

        //    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
        //        return ResponseProcess.GetFailReason(failureMessage);

        //    return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        //}

        [HttpPost]
        public APIResponse SelectEcollection([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectEcollectionRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectEcollectionRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            EcollectionWrapper wrapper;

            try
            {
                wrapper = new EcollectionDetailsBal().SelectEcollection(selectRequest, out failureMessage);
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
            if (selectRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("E_Collection", wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            FundRequestSelectByIDRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<FundRequestSelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
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
                response = new FundBal().Delete(keyDataModel.UserID, mainRequest.FundTransactionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!response.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(response);
            }

            return ResponseProcess.GetSucessReason(ProviderPurchaseMessage.DeleteSuccess);
        }
        [HttpPost]
        public APIResponse SelectWalletTransferReport([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            TransferReportRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<TransferReportRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            TransferReportWrapper wrapper;
            string failureMessage;
            try
            {
                (wrapper,failureMessage) = new FundBal().SelectWalletTransferReport(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (wrapper == null)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            if (wrapper.Count <= 0)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);
            if (mainRequest.IsPDFDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf("Fund_Report", wrapper.data, keyDataModel.UserID);
                wrapper.data = null;
            }
            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Fund_Report", wrapper.data, keyDataModel.UserID);
                wrapper.data = null;
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
    }
}

       