using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.UserwiseSequence;
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
    public class UserwiseDMTSequenceController : ControllerBase
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
                returnresponse = new UserWiseDMTSequenceBal().Insert(mainRequest);
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
        public APIResponse Select([FromForm] APIRequest aPIRequest)
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

            List<UserwiseDMTSequenceResponse> SequenceResponse = new List<UserwiseDMTSequenceResponse>();
            try
            {
                SequenceResponse = new UserWiseDMTSequenceBal().SelectAllUserwiseSequenceList();
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
                failureMessage = new UserWiseDMTSequenceBal().DeleteByUserID(deleteRequest);
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
                failureMessage = new UserWiseDMTSequenceBal().DeleteAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "DeleteAll", "Delete all user userwise sequence");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "DeleteAll", "Delete all user userwise sequence");
            }
            return ResponseProcess.GetSucessReason(SequenceMessage.DeleteAllSuccess,
                keyDataModel.UserID, "DeleteAll userwise sequence", "Delete all user userwise sequence");
        }
    }
}