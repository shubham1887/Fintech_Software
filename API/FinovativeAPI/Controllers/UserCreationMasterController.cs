using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.UserCreationMaster;
using Project.Response;
using Project.Response.UserCreationMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace FinovativeAPI.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class UserCreationMasterController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            InsertRequest insertRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                insertRequest = JsonConvert.DeserializeObject<InsertRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            try
            {
                failureMessage = new UserCreationMasterBal().Insert(insertRequest, keyDataModel.UserID);
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
            return ResponseProcess.GetSucessReason(UserCreationMessage.PurchaseSuccess);
        }

        [HttpPost]
        public APIResponse FreeTransfer([FromForm] APIRequest aPIRequest)
        {
            TransferRequest request;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                request = JsonConvert.DeserializeObject<TransferRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            try
            {
                failureMessage = new UserCreationMasterBal().FreeTransfer(request);
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
            return ResponseProcess.GetSucessReason(UserCreationMessage.TrnasferSuccess);
        }

        [HttpPost]
        public APIResponse Last10Report([FromForm] APIRequest aPIRequest)
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
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<ViewModel> viewModel;
            try
            {
                viewModel = new UserCreationMasterBal().Last10Report(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (viewModel == null || viewModel.Count <= 0)
            {
                return ResponseProcess.GetFailReason(Messages.DataNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
        }

        [HttpPost]
        public APIResponse SelectForFilter([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            UserFilterWrapper wrapper = new UserFilterWrapper();
            string failureMessage;

            if (keyDataModel == null || mainRequest == null) return ResponseProcess.GetFailReason(Messages.InValidRequest);
            try
            {
                wrapper = new UserCreationMasterBal().SelectForFilter(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (wrapper.list == null || wrapper.list.Count <= 0)
                return ResponseProcess.GetFailReason(failureMessage);

            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("User_Filter", wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
    }
}