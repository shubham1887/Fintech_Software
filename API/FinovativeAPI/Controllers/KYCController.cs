using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.KYC;
using Project.Response;
using Project.Response.KYC;
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
    public class KYCController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Upload([FromForm] APIRequest aPIRequest)
        {
            UploadRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UploadRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string response = string.Empty;

            try
            {
                response = new UserDocumentMasterBal().Upload(mainRequest, keyDataModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(failureMessage);
            }

            if (!response.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(response);

            return ResponseProcess.GetSucessReason(Message.KYCMessage.UploadSuccess);
        }

        [HttpPost]
        public APIResponse Approve([FromForm] APIRequest aPIRequest)
        {
            ApproveRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<ApproveRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                failureMessage = new UserDocumentMasterBal().Approve(mainRequest, keyDataModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.KYCMessage.ApproveSuccess);
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
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
                return ResponseProcess.GetFailReason(StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            KYCWrapper wrapper = null;
            try
            {
                wrapper = new UserDocumentMasterBal().Select(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (wrapper.list == null || wrapper.list.Count<=0)
                return ResponseProcess.GetFailReason(Message.UserMessage.UserNotFound);

            if (mainRequest.IsExcelDownload)
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("KYC_Report", wrapper.list, keyDataModel.UserID);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectPending([FromForm] APIRequest aPIRequest)
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
                return ResponseProcess.GetFailReason(StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<Response.KYC.ViewModel> users = new List<Response.KYC.ViewModel>();

            try
            {
                users = new UserDocumentMasterBal().SelectPending();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (users.Count <= 0)
                return ResponseProcess.GetFailReason(Message.UserMessage.NoKycPending);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(users));
        }

        [HttpPost]
        public APIResponse selectDocumentsByUserID([FromForm] APIRequest aPIRequest)
        {
            SelectDocumentRequest mainRequest;

            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectDocumentRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<Response.KYC.ViewModel> users = new List<Response.KYC.ViewModel>();

            try
            {
                users = new UserDocumentMasterBal().selectDocumentsByUserID(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (users.Count <= 0)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(users));
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest aPIRequest)
        {
            SelectByIDRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
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
                string request = ed.Decrypt(aPIRequest.Request);
                mainRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            ViewModel Response;
            try
            {
                Response = new UserDocumentMasterBal().SelectByID(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Response));
        }

        [HttpPost]
        public APIResponse SelectActiveDocUserwise([FromForm] APIRequest aPIRequest)
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

            List<KYCViewModel> list;
            try
            {
                list = new UserDocumentMasterBal().SelectActiveDocUserwise(keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }
    }
}