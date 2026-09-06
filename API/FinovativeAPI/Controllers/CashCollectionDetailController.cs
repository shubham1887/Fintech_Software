using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.CashCollection;
using Project.Response;
using Project.Response.CashCollection;
using Project.StaticModel;
using System;

namespace FinovativeAPI.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class CashCollectionDetailController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectByFilter([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            ReportRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<ReportRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            CollectionReportWrapper wrapper;
            try
            {
                wrapper = new CashCollectionDetailBal().SelectByFilter(mainRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            if (mainRequest.IsPDFDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf("Collection", wrapper.data, keyDataModel.UserID);
                wrapper.data = null;
            }
            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Collection", wrapper.data, keyDataModel.UserID);
                wrapper.data = null;
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
    }
}