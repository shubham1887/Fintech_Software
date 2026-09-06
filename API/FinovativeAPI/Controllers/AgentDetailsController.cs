using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.AgentDetails;
using Project.Response;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class AgentDetailsController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Upload([FromForm] APIRequest aPIRequest)
        {
            UploadRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                string decriptReq = ed.Decrypt(aPIRequest.Request);
                mainRequest = JsonConvert.DeserializeObject<UploadRequest>(decriptReq);

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            int successfulUpdates = 0;
            int totalRecords = 0;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                failureMessage = new AgentDetailsBAL().Upload(mainRequest, keyDataModel, out failureMessage, out successfulUpdates, out totalRecords);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            failureMessage = $"{successfulUpdates} records updated out of {totalRecords}.";

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(failureMessage));
        }

        [HttpPost]
        public APIResponse SelectByPinCode([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectPincodeRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectPincodeRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);


            Response.Agent.UserWrapper wrapper = null;
            try
            {
                wrapper = new AgentDetailsBAL().SelectByPincode(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (wrapper == null)
            {
                failureMessage = LogMessage.NoRecord;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(failureMessage));
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
    }
}
