using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.TransactionReport;
using Project.Response;
using Project.Response.TransactionReport;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [CustomAuthorization]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class UserAccountingController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();
        [HttpPost]
        public APIResponse UserwiseAccounting([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            UserwiseAccountingRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<UserwiseAccountingRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            UserWiseAcountingWrapper wrapper = null;
            try
            {
                wrapper = new UserAccountingBAL().UserwiseAccounting(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (selectRequest.IsPDFDownload)
            {
                string FileName = "UserWiseAccount_Report";
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf(FileName, wrapper.list, keyDataModel.UserID);
            }
            if (selectRequest.IsExcelDownload)
            {
                string FileName = "UserWiseAccount_Report";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }


        [HttpPost]
        public APIResponse SelectForFilterByOldAmountAdjustment([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            UserWiseAccountingCompRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<UserWiseAccountingCompRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            OldAmountAdjustmentReportWrapper wrapper = new OldAmountAdjustmentReportWrapper();
            string failureMessage;

            if (keyDataModel == null || mainRequest == null) return ResponseProcess.GetFailReason(Messages.InValidRequest);
            try
            {
                wrapper = new UserAccountingBAL().SelectForFilterByOldAmountAdjustment(mainRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (mainRequest.IsExcelDownload)
            {
                string FileName = "OldAmountAdjustment_Report";
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));

        }
        [HttpPost]
        public APIResponse SelectForFilterByNewAmountAdjustment([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            UserWiseAccountingCompRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<UserWiseAccountingCompRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            OldAmountAdjustmentReportWrapper wrapper = new OldAmountAdjustmentReportWrapper();
            string failureMessage;

            if (keyDataModel == null || mainRequest == null) return ResponseProcess.GetFailReason(Messages.InValidRequest);
            try
            {
                wrapper = new UserAccountingBAL().SelectForFilterByNewAmountAdjustment(mainRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (mainRequest.IsExcelDownload)
            {
                string FileName = "NewAmountAdjustment_Report";
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
        [HttpPost]
        public APIResponse ManualSuccessFail([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            UserWiseAccountingCompRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<UserWiseAccountingCompRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            ManualSuccessFailWrapper wrapper;

            try
            {
                wrapper = new UserAccountingBAL().SelectForFilterByManualUpdate(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (mainRequest.IsExcelDownload)
            {
                string FileName = "ManualSuccessFail_Report";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
    }
}
