using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.RangeWiseSequence;
using Project.Response;
using Project.Response.RangeWiseSequence;
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
    public class RangeWiseSequenceController : Controller
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            RangeWiseSequenceInsertRequest InsertRequest;
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
                InsertRequest = JsonConvert.DeserializeObject<RangeWiseSequenceInsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string Result = "";
            try
            {
                Result = new RangeWiseSequenceBal().Insert(InsertRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "RangeWiseSequence Insert", InsertRequest.ToString());
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
            List<ViewModel> Details;
            try
            {
                Details = new RangeWiseSequenceBal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (Details == null || Details.Count <= 0)
            {
                return ResponseProcess.GetFailReason(RangeWiseSequenceMessage.DetailFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Details));
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest aPIRequest)
        {
            SelectByIDRequest selectRequest;
            try
            {
                selectRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            ViewModel Details = new ViewModel();
            string failureMessage;
            try
            {
                Details = new RangeWiseSequenceBal().SelectByProviderDetailID(selectRequest, out failureMessage);
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

            if (Details == null)
            {
                return ResponseProcess.GetFailReason(ProviderMessage.ProviderNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Details));
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectByIDRequest deleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new RangeWiseSequenceBal().Delete(deleteRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "RangeWiseSequence Delete", deleteRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "RangeWiseSequence Delete", deleteRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(RangeWiseSequenceMessage.DeleteSuccess,
                 keyDataModel.UserID, "RangeWiseSequence Delete", deleteRequest.ToString());
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIRequest)
        {
            RangeWiseSequenceInsertRequest InsertRequest;
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
                InsertRequest = JsonConvert.DeserializeObject<RangeWiseSequenceInsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string Result = "";
            try
            {
                Result = new RangeWiseSequenceBal().Update(InsertRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "RangeWiseSequence ", InsertRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Result, InsertRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(Result,
                 keyDataModel.UserID, Result, InsertRequest.ToString());
        }
    }
}