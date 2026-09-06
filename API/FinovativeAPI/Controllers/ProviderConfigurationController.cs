using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.ProviderConfig;
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
    public class ProviderConfigurationController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectByProviderID([FromForm] APIRequest aPIRequest)
        {
            SelectByProviderID selectByProviderID;
            KeyDataModel keyDataModel;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectByProviderID = JsonConvert.DeserializeObject<SelectByProviderID>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            ProviderConfigurationWrapper ProviderConfigList;
            try
            {
                ProviderConfigList = new ProviderConfigurationBAL().SelectByProviderID(selectByProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (ProviderConfigList == null || ProviderConfigList.Count <= 0)
                return ResponseProcess.GetFailReason(ProviderMessage.ProviderOperatorNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ProviderConfigList));
        }

        [HttpPost]
        public APIResponse SelectByProviderIDOperatorID([FromForm] APIRequest aPIRequest)
        {
            SelectByProviderIDOperatorID selectByProviderID;
            KeyDataModel keyDataModel;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectByProviderID = JsonConvert.DeserializeObject<SelectByProviderIDOperatorID>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<Project.Response.User.StateViewModel> ProviderConfigList;
            try
            {
                ProviderConfigList = new ProviderConfigurationBAL().SelectByProviderIDOperatorID(selectByProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (ProviderConfigList == null || ProviderConfigList.Count <= 0)
                return ResponseProcess.GetFailReason(ProviderMessage.ProviderOperatorNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ProviderConfigList));
        }

        [HttpPost]
        public APIResponse ActiveDeactive([FromForm] APIRequest aPIRequest)
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
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                failureMessage = new ProviderConfigurationBAL().ActiveDeactive(selectByIDRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "API operator onoff", selectByIDRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "API operator onoff", selectByIDRequest.ToString());

            string returnMessage = selectByIDRequest.IsActive ? Message.OperatorMessage.ActiveSuccess : Message.OperatorMessage.DeactiveSuccess;
            return ResponseProcess.GetSucessReason(returnMessage,
                 keyDataModel.UserID, "API operator onoff", selectByIDRequest.ToString());
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            UpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                string ent = ed.Decrypt(aPIRequest.Request);

                updateRequest = JsonConvert.DeserializeObject<UpdateRequest>(ent);
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
                failureMessage = new ProviderConfigurationBAL().Update(updateRequest, false);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "API Config Update", updateRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "API Config Update", updateRequest.ToString());

            return ResponseProcess.GetSucessReason(ProviderConfigMessage.UpdateSuccess,
                keyDataModel.UserID, "API Config Update", updateRequest.ToString());
        }

        [HttpPost]
        public APIResponse CirecleMappingInsert([FromForm] APIRequest encRequest)
        {
            CircleMappingInsertRequest mainRequest;
            KeyDataModel keyDataModel;
            string tag = "Circle Mapping Insert";
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string request = "";
            try
            {
                request = ed.Decrypt(encRequest.Request);
                mainRequest = JsonConvert.DeserializeObject<CircleMappingInsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string returnresponse;
            try
            {
                returnresponse = new ProviderConfigurationBAL().CirecleMappingInsert(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured, keyDataModel.UserID, tag, request);
            }

            if (!returnresponse.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(returnresponse, keyDataModel.UserID, tag, request);
            }

            return ResponseProcess.GetSucessReason(ProviderConfigMessage.CircleMappingInsertSuccess, keyDataModel.UserID, tag, request);
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest aPIRequest)
        {
            SelectByIDRequest selectByIDRequest;
            KeyDataModel keyDataModel;
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
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            ConfigurationViewModel ViewModel;

            try
            {
                ViewModel = new ProviderConfigurationBAL().SelectByID(selectByIDRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            if (ViewModel == null) return ResponseProcess.GetFailReason(ProviderConfigMessage.Datanotfound);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ViewModel));
        }

        [HttpPost]
        public APIResponse UpdateAll([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            List<UpdateRequest> updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<List<UpdateRequest>>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new ProviderConfigurationBAL().UpdateAll(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "API Config Update all", updateRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "API Config Update all", updateRequest.ToString());

            return ResponseProcess.GetSucessReason(ProviderConfigMessage.UpdateSuccess,
                keyDataModel.UserID, "API Config Update all", updateRequest.ToString());
        }
    }
}