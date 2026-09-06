using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response;
using Project.Response.VoucherDetails;
using Project.StaticModel;
using System;

namespace Project.Controllers
{

    [CustomAuthorization]
        [Route("[controller]/[action]")]
        [EnableCors("CORSProject")]
        [ApiController]
        public class VoucherDetailsController : Controller
        {
            private readonly EncryptDecrypt ed = new EncryptDecrypt();


        [HttpPost]
        public APIResponse SelectReport([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.VoucherDetails.VoucherDetailsSelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.VoucherDetails.VoucherDetailsSelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            VoucherDetailsWrapper wrapper = new VoucherDetailsWrapper();
            string failureMessage;
            try
            {
                wrapper = new VoucherDetailsBal().SelectVoucherDetailsReport(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }


            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Voucher_Details_Report", wrapper.list, keyDataModel.UserID);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));

        }
    }
}
