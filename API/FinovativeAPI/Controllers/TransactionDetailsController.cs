using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.TransactionDetails;
using Project.Response;
using Project.Response.TransactionDetails;

using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class TransactionDetailsController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectForFilter([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            ReportRequest txReportRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                txReportRequest = JsonConvert.DeserializeObject<ReportRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage;
            ReportWrapper wrapper = new ReportWrapper();
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                wrapper = new TransactionDetailsBal().SelectForFilter(txReportRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (txReportRequest.IsPDFDownload)
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf("Account_Statement", wrapper.list, keyDataModel.UserID);

            if (txReportRequest.IsExcelDownload)
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Account_Statement", wrapper.list, keyDataModel.UserID);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public async Task<APIResponse> SelectAdminAccounting([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            AdminAccountingRequest reportRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                reportRequest = JsonConvert.DeserializeObject<AdminAccountingRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            List<AdminAccountingViewModel> list = new List<AdminAccountingViewModel>();

            try
            {
                list = await new TransactionDetailsBal().SelectAdminAccounting(reportRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public async Task<APIResponse> SelectMarketAccounting([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            AdminAccountingRequest reportRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                reportRequest = JsonConvert.DeserializeObject<AdminAccountingRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<AdminAccountingViewModel> list = new List<AdminAccountingViewModel>();
            try
            {
                list = await new TransactionDetailsBal().SelectMarketAccounting(reportRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }
    }
}