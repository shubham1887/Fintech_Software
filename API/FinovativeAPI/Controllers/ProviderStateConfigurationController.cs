using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.ProviderStateConfiguration;
using Project.Response;
using Project.Response.ProviderStateConfiguration;
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
    public class ProviderStateConfigurationController : ControllerBase
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

            List<ViewModel> viewModel;
            try
            {
                viewModel = new ProviderStateConfigurationBal().SelectByProviderID(selectByProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (viewModel == null)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
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
                failureMessage = new ProviderStateConfigurationBal().Update(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(ProviderConfigMessage.StatecodeUpdateSuccess);
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
                failureMessage = new ProviderStateConfigurationBal().UpdateAll(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(ProviderConfigMessage.StatecodeUpdateSuccess);
        }
    }
}