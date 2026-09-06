using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.Sequence;
using Project.Response;
using Project.Response.Sequence;
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
    public class OperatorSequenceUserWiseController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest encRequest)
        {
            InsertRequest mainRequest;
            KeyDataModel keyDataModel;
            string tag = "Userwise Sequence change";
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
                mainRequest = JsonConvert.DeserializeObject<InsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string returnresponse;
            try
            {
                returnresponse = new OperatorSequenceUserWiseBal().Insert(mainRequest);
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
            return ResponseProcess.GetSucessReason(SequenceMessage.InsertSuccess, keyDataModel.UserID, tag, request);
        }

        [HttpPost]
        public APIResponse SelectByOperatorID([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectByUserIDWithOperatorID selectByUserIDWithOperatorID;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                selectByUserIDWithOperatorID = JsonConvert.DeserializeObject<SelectByUserIDWithOperatorID>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            OperatorSequenceResponse SequenceResponse;
            try
            {
                SequenceResponse = new OperatorSequenceUserWiseBal().SelectByOperatorID(selectByUserIDWithOperatorID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (SequenceResponse == null)
                return ResponseProcess.GetSucessReason(SequenceMessage.RecordNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SequenceResponse));
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByUserID selectByUserID;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectByUserID = JsonConvert.DeserializeObject<SelectByUserID>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<OperatorSequenceResponse> SequenceResponse;
            try
            {
                SequenceResponse = new OperatorSequenceUserWiseBal().Select(selectByUserID, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (SequenceResponse == null || SequenceResponse.Count <= 0)
            {
                return ResponseProcess.GetFailReason(SequenceMessage.RecordNotFound);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SequenceResponse));
        }

        [HttpPost]
        public APIResponse SelectAllUserwiseSequenceList([FromForm] APIRequest aPIRequest)
        {
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

            List<OperatorSequenceResponse> SequenceResponse;
            try
            {
                SequenceResponse = new OperatorSequenceUserWiseBal().SelectAllUserwiseSequenceList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (SequenceResponse == null || SequenceResponse.Count <= 0)
            {
                return ResponseProcess.GetFailReason(SequenceMessage.UserwiseNotFound);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SequenceResponse));
        }

        [HttpPost]
        public APIResponse DeleteByUserID([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectByUserID deleteRequest;
            string tag = "Reset userwise sequence";
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<SelectByUserID>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new OperatorSequenceUserWiseBal().DeleteByUserID(deleteRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured, keyDataModel.UserID, tag, deleteRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage, keyDataModel.UserID, tag, deleteRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(SequenceMessage.DefaultsetSuccess, keyDataModel.UserID, tag, deleteRequest.ToString());
        }

        [HttpPost]
        public APIResponse DeleteAll([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
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
                failureMessage = new OperatorSequenceUserWiseBal().DeleteAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "ResetAll", "Reset all user userwise sequence");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "ResetAll", "Reset all user userwise sequence");
            }
            return ResponseProcess.GetSucessReason(SequenceMessage.ResetAllSuccess,
                keyDataModel.UserID, "ResetAll userwise sequence", "Reset all user userwise sequence");
        }
    }
}