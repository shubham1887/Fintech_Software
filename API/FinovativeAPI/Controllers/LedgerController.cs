using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.Ledger;
using Project.Response;
using Project.Response.Ledger;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class LedgerController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            LedgerViewModel list = null;

            if (selectRequest.UserID == 0)
                selectRequest.UserID = keyDataModel.UserID;
            try
            {
                list = new LedgerBal().Select(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.DataNotFound);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (selectRequest.IsPDFDownload)
            {
                list.ReportPath = new ExcelProcessing().GeneratePdf("User_Ledger", list.list, keyDataModel.UserID);
            }

            if (selectRequest.IsExcelDownload)
            {
                list.ReportPath = new ExcelProcessing().GenerateExcel("User_Ledger", list.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse AdminLedger([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            AdminRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<AdminRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            AdminLedgerWrapper wrapper;
            try
            {
                wrapper = new LedgerBal().AdminLedger(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (selectRequest.IsExcelDownload)
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Admin_Ledger", wrapper.data, keyDataModel.UserID);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
    }
}