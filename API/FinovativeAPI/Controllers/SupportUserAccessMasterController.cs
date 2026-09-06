using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.SupportUserAccessDetail;
using Project.Response;
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
    public class SupportUserAccessMasterController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            UpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<UpdateRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new SupportUserAccessMasterBal().Insert(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string returnMessage = updateRequest.IsActive ? SupportUserMessage.ActiveSuccess : SupportUserMessage.DeactiveSuccess;
            return ResponseProcess.GetSucessReason(returnMessage);
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            List<Response.SupportUserAccessDetail.Parent> response;
            try
            {
                response = new SupportUserAccessMasterBal().GetMenu(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}