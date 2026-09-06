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
    public class OperatorSequenceController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest encRequest)
        {
            InsertRequest mainRequest;
            KeyDataModel keyDataModel;
            string Action = UserAction.Sequence_Change;

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
                returnresponse = new OperatorSequenceBal().Insert(mainRequest, keyDataModel.UserID, Action);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured, keyDataModel.UserID, Action, request);
            }

            if (!returnresponse.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(returnresponse, keyDataModel.UserID, Action, mainRequest.ToString());

            return ResponseProcess.GetSucessReason(SequenceMessage.InsertSuccess, keyDataModel.UserID, Action, mainRequest.ToString(), mainRequest);
        }

        [HttpPost]
        public APIResponse SelectByOperatorID([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectByOperatorID selectByIDRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                selectByIDRequest = JsonConvert.DeserializeObject<SelectByOperatorID>(ed.Decrypt(aPIrequest.Request));
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
                SequenceResponse = new OperatorSequenceBal().SelectByOperatorID(selectByIDRequest, out failureMessage);
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
            SelectByServiceID selectByServiceID;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectByServiceID = JsonConvert.DeserializeObject<SelectByServiceID>(ed.Decrypt(aPIRequest.Request));
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
                SequenceResponse = new OperatorSequenceBal().Select(selectByServiceID, keyDataModel.UserID);
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
        public APIResponse SelectProviderForBulkSequence([FromForm] APIRequest aPIRequest)
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

            List<ProviderList> ProviderListResponse;
            try
            {
                ProviderListResponse = new OperatorSequenceBal().SelectProviderForBulkSequence();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (ProviderListResponse == null || ProviderListResponse.Count <= 0)
            {
                return ResponseProcess.GetFailReason(SequenceMessage.RecordNotFound);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ProviderListResponse));
        }

        [HttpPost]
        public APIResponse BulkUpdate([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<BulkUpdateRequest> mainRequest;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<List<BulkUpdateRequest>>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string Result = "";
            try
            {
                Result = new OperatorSequenceBal().BulkUpdate(mainRequest, out string failureMessge);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "BulkUpdate", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "BulkUpdate", mainRequest.ToString());

            return ResponseProcess.GetSucessReason(Result,
                 keyDataModel.UserID, "BulkUpdate", mainRequest.ToString());
        }
    }
}