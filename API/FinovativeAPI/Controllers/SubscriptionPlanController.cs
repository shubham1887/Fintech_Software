using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.Processing;
using Project.Requests;
using Project.Requests.ServiceSubscription;
using Project.Response.ServiceSubscription;
using Project.Response;
using Project.StaticModel;
using System;
using Project.BAL;
using Project.Message;
using System.Collections.Generic;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Create([FromForm] APIRequest aPIRequest)
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
                  
            try
            {
                failureMessage = new SubscriptionPlanBal().Insert(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }         

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Messages.InsertSuccess);
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            UpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<UpdateRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SubscriptionPlanBal().Update(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(PaymentGatewayMessage.UpdateSuccess);
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
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            List<ViewModel> viewModel;
            try
            {
                viewModel = new SubscriptionPlanBal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (viewModel == null || viewModel.Count <= 0)
                return ResponseProcess.GetFailReason(PaymentGatewayMessage.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
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
                response = new SubscriptionPlanBal().SelectByID(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse SelectByDetailID([FromForm] APIRequest aPIRequest)
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

            AssignPlanViewModel response = null;
            try
            {
                response = new SubscriptionPlanBal().SelectPlanDetail(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);
              
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
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
                failureMessage = new SubscriptionPlanBal().Delete(deleteRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured, keyDataModel.UserID, deleteRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage, keyDataModel.UserID, deleteRequest.ToString());
            }
            return ResponseProcess.GetSucessReason(Messages.DeleteSuccess, keyDataModel.UserID, deleteRequest.ToString());
        }

        [HttpPost]
        public APIResponse IsServiceAndEKYCActive([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            KeyDataModel keyDataModel;
            SelectByServiceIDRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectByServiceIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            
            try
            {
                failureMessage = new UserServiceMappingBAL().IsServiceActivated(keyDataModel.UserID, mainRequest.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(failureMessage));
        }

        [HttpPost]
        public APIResponse SelectPlanList([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByServiceIDRequest Request;

            try
            {
                Request = JsonConvert.DeserializeObject<SelectByServiceIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
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
                viewModel = new SubscriptionPlanBal().SelectPlanList(keyDataModel.UserID, Request, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (viewModel == null || viewModel.Count <= 0)
                return ResponseProcess.GetFailReason(PaymentGatewayMessage.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
        }

        [HttpPost]
        public APIResponse Subscribe([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            AssignPlanInsertRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<AssignPlanInsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            Project.Response.ServiceAndUserwiseSubscription.InsertResponse response = new Project.Response.ServiceAndUserwiseSubscription.InsertResponse();
            try
            {
                response = new AssignSubscriptionPlanBal().Insert(keyDataModel.UserID, mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!response.Status.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(response.Message);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }


        [HttpPost]
        public APIResponse IsSubscribed([FromForm] APIRequest aPIRequest)
        {
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
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                failureMessage = new AssignSubscriptionPlanBal().SelectByUserID(keyDataModel.UserID);
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
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Messages.NotFound));
        }
    }
}