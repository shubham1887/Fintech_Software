using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.SettingMaster;
using Project.Response;
using Project.Response.SettingMaster;
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
    public class SettingMasterController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<ViewModel> settingMaster;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            try
            {
                settingMaster = new SettingMasterBal().Select(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (settingMaster == null || settingMaster.Count <= 0)
            {
                return ResponseProcess.GetFailReason(SettingMessage.RecordFound);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(settingMaster));
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest encRequest)

        {
            KeyDataModel keyDataModel;
            SelectRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            ViewModel viewModel = new ViewModel();

            try
            {
                viewModel = new SettingMasterBal().SelectByID(mainRequest);
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

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            UpdateRequest mainRequest;
            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<UpdateRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                failureMessage = new SettingMasterBal().Update(mainRequest);
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

            return ResponseProcess.GetSucessReason(SettingMessage.UpdateSuccess);
        }

        [HttpPost]
        public APIResponse IsSecureOnOff([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new SettingMasterBal().IsSecureOnOff(selectRequest);
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

            string returnMessage = selectRequest.IsSecure ? SettingMessage.IsSecure : SettingMessage.IsUnSecure;

            return ResponseProcess.GetSucessReason(returnMessage);
        }

        [HttpPost]
        public APIResponse SelectByKey([FromForm] APIRequest encRequest)

        {
            KeyDataModel keyDataModel;
            SelectByKeyRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectByKeyRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            string Value = "";

            try
            {
                Value = new SettingMasterBal().SelectByUnSecureKey(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (Value.EqualsIgnoreCase("Invalid key"))
            {
                return ResponseProcess.GetFailReason(Value);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Value));
        }

        [HttpPost]
        public APIResponse SelectValueByKey([FromForm] APIRequest encRequest)

        {
            KeyDataModel keyDataModel;
            SelectByKeyRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectByKeyRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            string Value = "";

            try
            {
                Value = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(mainRequest.Key, "No");
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (Value.EqualsIgnoreCase("Invalid key"))
            {
                return ResponseProcess.GetFailReason(Value);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Value));
        }

        [HttpPost]
        public APIResponse SelectOnlineDMTValue([FromForm] APIRequest encRequest)

        {
            KeyDataModel keyDataModel;
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
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            SelectOnlineDMTValueModel viewModel = null;
            try
            {
                viewModel = new SettingMasterBal().SelectOnlineDMTValue();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
        }

        [HttpPost]
        public APIResponse UpdateOnlineDMTValue([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            UpdateOnlineDMTValueRequest mainRequest;
            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<UpdateOnlineDMTValueRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                failureMessage = new SettingMasterBal().UpdateOnlineDMTValue(mainRequest);
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

            return ResponseProcess.GetSucessReason(SettingMessage.UpdateSuccess);
        }
    }
}