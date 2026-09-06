using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.Tax;
using Project.Response;
using Project.Response.Tax;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class TaxController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();
        private IConverter _converter;

        public TaxController(IConverter converter)
        {
            _converter = converter;
        }

        [HttpPost]
        public APIResponse SelectTDS([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            TDSReportRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<TDSReportRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            TDSReportWrapper wrapper;
            try
            {
                wrapper = new TaxBAL().SelectTDS(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (selectRequest.IsPDFDownload)
            {
                string FileName = "TDS";
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf(FileName, wrapper.list, keyDataModel.UserID);
            }
            if (selectRequest.IsExcelDownload)
            {
                string FileName = "TDS";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectGSTDetailReportForAdmin([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            GSTDetailReportRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<GSTDetailReportRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            GSTDetailReportWrapper wrapper;
            try
            {
                wrapper = new TaxBAL().SelectGSTDetailReportForAdmin(selectRequest, keyDataModel.UserID, out failureMessage);
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
                string FileName = "TDS";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectGSTDetail([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            GSTDetailRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<GSTDetailRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            GSTDetailWrapper wrapper;
            try
            {
                wrapper = new TaxBAL().SelectGSTDetail(selectRequest, keyDataModel.UserID, out failureMessage);
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
                string FileName = "GST";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectGSTDetailByTransaction([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            GSTDetailRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<GSTDetailRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            GSTDetailWrapper wrapper;
            try
            {
                wrapper = new TaxBAL().SelectGSTDetailByTransaction(selectRequest, keyDataModel.UserID, out failureMessage);
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
                string FileName = "GST";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectGSTCategoryWise([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByCategoryRequest requestModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                requestModel = JsonConvert.DeserializeObject<SelectByCategoryRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string path = "";
            try
            {
                path = new TaxBAL(_converter).SelectGSTCategoryWise(requestModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(path);
        }

        [HttpPost]
        public APIResponse SelectGSTCategoryWiseV2([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByCategoryRequest requestModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                requestModel = JsonConvert.DeserializeObject<SelectByCategoryRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string path = "";
            try
            {
                path = new TaxBAL(_converter).SelectGSTCategoryWiseV2(requestModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(path);
        }
    }
}