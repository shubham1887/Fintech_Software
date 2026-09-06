using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.DMTSetting;
using Project.Response;
using Project.Response.DMTSettings;
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
    public class DMTSettingsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        public DMTSettingsController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            InsertRequest mainRequest;
            KeyDataModel keyDataModel;

            try
            {
                mainRequest = JsonConvert.DeserializeObject<InsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string response = string.Empty;
            try
            {
                response = new DMTSettingsBal().Insert(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(response);
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            UpdateRequest mainRequest;
            KeyDataModel keyDataModel;

            try
            {
                mainRequest = JsonConvert.DeserializeObject<UpdateRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string response = string.Empty;
            try
            {
                response = new DMTSettingsBal().Update(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(response);
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

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

            List<ViewModel> response = null;
            try
            {
                response = new DMTSettingsBal(_mapper).Select(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse SelectTransferMode([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            KeyDataModel keyDataModel;
            SelectTransferModeRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectTransferModeRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<TransferModeModel> response = null;
            try
            {
                response = new DMTSettingsBal().SelectTransferMode(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            KeyDataModel keyDataModel;
            SelectByIDRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            ViewModel response = null;
            try
            {
                response = new DMTSettingsBal(_mapper).SelectByID(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse SelectMultiActiveDMT([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

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

            List<Response.User.ActiveDMTList> response = new List<Response.User.ActiveDMTList>();
            try
            {
                response = new DMTSettingsBal().SelectMultiActiveDMT(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}