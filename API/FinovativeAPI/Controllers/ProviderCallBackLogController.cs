using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.Transaction;
using Project.Response;
using Project.Response.ProviderCallBackResponse;
using Project.Response.ProviderRequestResponseLogResponse;
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
    public class ProviderCallBackLogController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectByConsumerNumber([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SearchRequest searchRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                searchRequest = JsonConvert.DeserializeObject<SearchRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            List<ProviderCallbackViewModel> providerCallbackViewModel = new List<ProviderCallbackViewModel>();

            try
            {
                providerCallbackViewModel = new ProviderCallBackLogBal().SelectByConsumerNumber(searchRequest, out failureMessage);
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

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(providerCallbackViewModel));
        }

        [HttpPost]
        public APIResponse SelectByTansactionMasterID([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByIDRequest selectByIDRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectByIDRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            List<ProviderCallbackViewModel> providerCallbackViewModel = new List<ProviderCallbackViewModel>();
            try
            {
                providerCallbackViewModel = new ProviderCallBackLogBal().SelectByTansactionMasterID(selectByIDRequest, out failureMessage);
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

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(providerCallbackViewModel));
        }

        [HttpPost]
        public APIResponse SelectByFromDateTodate([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.APILog.SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<Project.Requests.APILog.SelectRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            ReportWrapper Wrapper = null;

            try
            {
                Wrapper = new ProviderCallBackLogBal().SelectByFromDateTodate(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (Wrapper.list == null)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);
            if (selectRequest.IsExcelDownload)
            {
                Wrapper.ReportPath = new ExcelProcessing().GenerateExcel("ProviderCallbacklog", Wrapper.list, keyDataModel.UserID);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Wrapper));
        }
    }
}