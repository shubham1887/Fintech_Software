using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.GSTHold;
using Project.Response;
using Project.Response.GSTHold;
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
    public class GSTHoldController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectGSTHold([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            GSTHoldSearchRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<GSTHoldSearchRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            GSTHoldWrapper wrapper;

            try
            {
                wrapper = new GSTHoldBal().SelectGSTHold(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (selectRequest.IsExcelDownload)
            {
                string FileName = "GST_Hold";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectForGSTHold([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            Requests.TransactionReport.GSTSummaryRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<Requests.TransactionReport.GSTSummaryRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            CheckGSTHoldWrapper wrapper;

            try
            {
                wrapper = new GSTHoldBal().SelectForGSTHold(selectRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (selectRequest.IsExcelDownload)
            {
                string FileName = "GST_Hold";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse UpdateStatus([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            UpdateStatusRequest UpdateRequest;
            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                UpdateRequest = JsonConvert.DeserializeObject<UpdateStatusRequest>(ed.Decrypt(encRequest.Request));
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
                failureMessage = new GSTHoldBal().UpdateStatus(UpdateRequest, keyDataModel.UserID);
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

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Message.GSTMessage.Statussuccessupdate));
        }

        [HttpPost]
        public APIResponse BulkUpdateGST([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<GSTBulkUpdateUsersRequest> mainRequest;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<List<GSTBulkUpdateUsersRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string Result = "";
            try
            {
                Result = new GSTHoldBal().BulkUpdateGST(mainRequest, out string failureMessge);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "BulkUpdateGST", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "BulkUpdateGST", mainRequest.ToString());

            return ResponseProcess.GetSucessReason(Result.Trim().ToString());
        }
    }
}