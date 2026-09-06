using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.ROfferCommission;
using Project.Response;
using Project.Response.ROfferCommission;
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
    public class ROfferCommissionController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            InsertRequest InsertRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                InsertRequest = JsonConvert.DeserializeObject<InsertRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new ROfferCommissionBAL().Insert(InsertRequest, false);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Commission insert", InsertRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "Commission insert", InsertRequest.ToString());

            return ResponseProcess.GetSucessReason(Message.SlabDetailMessage.InsertSuccess,
                 keyDataModel.UserID, "Commission insert", InsertRequest.ToString());
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIrequest)
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

            string Action = "Commission update";
            try
            {
                failureMessage = new ROfferCommissionBAL().Update(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, Action, updateRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Action, updateRequest.ToString());

            return ResponseProcess.GetSucessReason(SlabDetailMessage.UpdateSuccess,
                 keyDataModel.UserID, Action, updateRequest.ToString());
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByIDRequest DeleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                DeleteRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new ROfferCommissionBAL().Delete(DeleteRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Commission Deleted", DeleteRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Commission Deleted", DeleteRequest.ToString());

            return ResponseProcess.GetSucessReason(SlabDetailMessage.DeleteSuccess,
                keyDataModel.UserID, "Commission Deleted", DeleteRequest.ToString());
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            List<DetailViewModel> SlabDetaiList;
            try
            {
                SlabDetaiList = new ROfferCommissionBAL().Select(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (SlabDetaiList == null || SlabDetaiList.Count <= 0)
                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SlabDetaiList));
        }

        [HttpPost]
        public APIResponse SelectFRCAmount([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectFRCAmountRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectFRCAmountRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            List<SelectFRCAmountResponse> SlabDetaiList;
            try
            {
                SlabDetaiList = new ROfferCommissionBAL().SelectFRCAmount(selectRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (SlabDetaiList == null || SlabDetaiList.Count <= 0)
                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SlabDetaiList));
        }

        [HttpPost]
        public APIResponse Activate([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByIDRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new ROfferCommissionBAL().Activate(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string returnMessage = updateRequest.IsActive ? Message.OperatorMessage.ActiveSuccess : Message.OperatorMessage.DeactiveSuccess;
            return ResponseProcess.GetSucessReason(returnMessage);
        }
    }
}