using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.OperatorUserwise;
using Project.Response;
using Project.Response.OperatorUserwise;
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
    public class OperatorUserwiseController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

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
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<ViewModel> viewModel;
            try
            {
                viewModel = new OperatorUserwiseBal().Select(selectRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (viewModel == null || viewModel.Count <= 0)
            {
                return ResponseProcess.GetFailReason(Messages.DataNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
        }

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            InsertRequest insertRequest;
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
            try
            {
                string request = ed.Decrypt(aPIRequest.Request);
                insertRequest = JsonConvert.DeserializeObject<InsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string Result = "";
            try
            {
                Result = new OperatorUserwiseBal().Insert(insertRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Update UserwiseOperator", insertRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Update UserwiseOperator", insertRequest.ToString());

            return ResponseProcess.GetSucessReason(Result,
                keyDataModel.UserID, "Update UserwiseOperator", insertRequest.ToString());
        }

        [HttpPost]
        public APIResponse DeleteByUserID([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest deleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new OperatorUserwiseBal().DeleteByUserID(deleteRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "UserwiseOperator delete", deleteRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "UserwiseOperator delete", deleteRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(OperatorMessage.DefaultsetSuccess,
                keyDataModel.UserID, "UserwiseOperator delete", deleteRequest.ToString());
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
                failureMessage = new OperatorUserwiseBal().DeleteAll(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "UserwiseOperator ResetAll", "ResetAll userwise operator");
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage, keyDataModel.UserID, "UserwiseOperator ResetAll", "ResetAll userwise operator");
            }
            return ResponseProcess.GetSucessReason(OperatorMessage.ResetAllSuccess,
                keyDataModel.UserID, "UserwiseOperator ResetAll", "ResetAll userwise operator");
        }

        [HttpPost]
        public APIResponse Activate([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectActiveRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<SelectActiveRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new OperatorUserwiseBal().Activate(updateRequest);
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