using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.Provider;
using Project.Response;
using Project.Response.Provider;
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
    public class ProviderDetailsController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            ProviderDetailsInsertRequest InsertRequest;
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

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                string request = ed.Decrypt(aPIRequest.Request);
                InsertRequest = JsonConvert.DeserializeObject<ProviderDetailsInsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string Result = "";
            try
            {
                Result = new ProviderDetailsBAL().Insert(InsertRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Provider Detail Insert", InsertRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Result, InsertRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(Result,
                 keyDataModel.UserID, Result, InsertRequest.ToString());
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            List<DetailsViewModel> ProviderDetails;
            try
            {
                ProviderDetails = new ProviderDetailsBAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (ProviderDetails == null || ProviderDetails.Count <= 0)
            {
                return ResponseProcess.GetFailReason(ProviderMessage.ProviderNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ProviderDetails));
        }

        [HttpPost]
        public APIResponse SelectByProviderDetailID([FromForm] APIRequest aPIRequest)
        {
            ProviderDetailsSelectRequest selectRequest;
            try
            {
                selectRequest = JsonConvert.DeserializeObject<ProviderDetailsSelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            DetailsViewModel ProviderDetails = new DetailsViewModel();
            string failureMessage;
            try
            {
                ProviderDetails = new ProviderDetailsBAL().SelectByProviderDetailID(selectRequest, out failureMessage);
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

            if (ProviderDetails == null)
            {
                return ResponseProcess.GetFailReason(ProviderMessage.ProviderNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ProviderDetails));
        }

        [HttpPost]
        public APIResponse ActiveDeactive([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            ProviderDetailsSelectRequest providerDetailsRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                providerDetailsRequest = JsonConvert.DeserializeObject<ProviderDetailsSelectRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new ProviderDetailsBAL().ActiveDeactive(providerDetailsRequest);
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
            string returnMessage = providerDetailsRequest.IsActive ? ProviderMessage.ActiveSuccess : ProviderMessage.DeactiveSuccess;
            return ResponseProcess.GetSucessReason(returnMessage);
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            ProviderDetailsUpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<ProviderDetailsUpdateRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new ProviderDetailsBAL().Update(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Provider Detail Update", updateRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Provider Detail Update", updateRequest.ToString());

            return ResponseProcess.GetSucessReason(ProviderMessage.ProviderUpdateSuccess);
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            ProviderDetailsSelectRequest deleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<ProviderDetailsSelectRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new ProviderDetailsBAL().Delete(deleteRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Provider Detail Delete", deleteRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "Provider Detail Delete", deleteRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(ProviderMessage.ProviderDeailsDeleteSuccess,
                 keyDataModel.UserID, "Provider Detail Delete", deleteRequest.ToString());
        }

        [HttpPost]
        public APIResponse SelectByProviderID([FromForm] APIRequest aPIRequest)
        {
            ProviderRequest SelectRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SelectRequest = JsonConvert.DeserializeObject<ProviderRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<DetailsViewModel> ProviderDetaiList;
            try
            {
                ProviderDetaiList = new ProviderDetailsBAL().SelectByProviderID(SelectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (ProviderDetaiList == null || ProviderDetaiList.Count <= 0)
            {
                return ResponseProcess.GetFailReason(ProviderMessage.ProviderNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ProviderDetaiList));
        }
    }
}