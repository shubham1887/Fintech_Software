using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.VirtualBalance;
using Project.Response;
using Project.Response.VirtualBalance;
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
    public class VirtualBalanceController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Add([FromForm] APIRequest aPIRequest)
        {
            AddRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<AddRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Project.StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            AddResponse Result = null;
            try
            {
                Result = new VirtualBalanceBal().Add(mainRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Result));
        }

        [HttpPost]
        public APIResponse AddWithOTP([FromForm] APIRequest aPIRequest)
        {
            AddWithOTPRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<AddWithOTPRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Project.StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            AddResponse Result = null;
            try
            {
                Result = new VirtualBalanceBal().AddWithOTP(mainRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Result));
        }

        [HttpPost]
        public APIResponse SelectLast10([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Project.StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<ViewModel> Result = null;
            try
            {
                Result = new VirtualBalanceBal().SelectLast10(keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Result));
        }

        [HttpPost]
        public APIResponse SelectForFilter([FromForm] APIRequest aPIRequest)
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
                return ResponseProcess.GetFailReason(Project.StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            ReportWrapper wrapper = null;
            try
            {
                wrapper = new VirtualBalanceBal().SelectForFilter(selectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (selectRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("VirtualBal_Report", wrapper.list, keyDataModel.UserID);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
    }
}