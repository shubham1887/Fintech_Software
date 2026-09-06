using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.OTP;
using Project.Requests.Service;
using Project.Response;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectAll([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            try
            {
                list = new ServiceMasterBal().Select(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse SelectByServiceID([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            Response.Service.ViewModel viewModel = new Response.Service.ViewModel();

            try
            {
                viewModel = new ServiceMasterBal().SelectByServiceID(mainRequest);
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

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(viewModel));
        }

        [HttpPost]
        public APIResponse ActiveDeactive([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            ActivateRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<ActivateRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                failureMessage = new ServiceMasterBal().Activate(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Service ActiveDeactive", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Service ActiveDeactive", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(ServiceMessage.UpdateSuccess,
                keyDataModel.UserID, "Service ActiveDeactive", mainRequest.ToString());
        }

        [HttpPost]
        public async Task<APIResponse> AllSericesActiveDeactiveGenerateOTP([FromForm] APIRequest encRequest)
        {

            KeyDataModel keyDataModel;

            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            Project.Response.Service.ServiceAllActiveDeactiveOtpResponse serviceAllActiveDeactiveOtpResponse = null;
            try
            {
                 serviceAllActiveDeactiveOtpResponse =  new ServiceMasterBal().GenerateAllServiceActiveDeactiveOTP(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }


            if (!serviceAllActiveDeactiveOtpResponse.Message.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(serviceAllActiveDeactiveOtpResponse));
          
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(serviceAllActiveDeactiveOtpResponse));

        }

        [HttpPost]
        public async Task<APIResponse>AllSericesActiveDeactiveVeriftyOTP([FromForm] APIRequest aPIrequest)
                  {
            AllServiceActiveDeactiveOTPRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<AllServiceActiveDeactiveOTPRequest>(ed.Decrypt(aPIrequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
          
            string failureMessage;
            try
            {
                failureMessage = await new ServiceMasterBal().AllServiceActiveDeactiveOTPRequest(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Service ActiveDeactive", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Service ActiveDeactive", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(ServiceMessage.UpdateSuccess,
                keyDataModel.UserID, "Service ActiveDeactive", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest encRequest)
        {
            Requests.Service.InsertRequest mainRequest;
            KeyDataModel keyDataModel;
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
            try
            {
                string request = ed.Decrypt(encRequest.Request);
                mainRequest = JsonConvert.DeserializeObject<Requests.Service.InsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string returnresponse;
            try
            {
                returnresponse = new ServiceMasterBal().Insert(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!returnresponse.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(returnresponse);
            }
            return ResponseProcess.GetSucessReason(ServiceMessage.InsertSuccess);
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            Requests.Service.UpdateRequest mainRequest;
            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Requests.Service.UpdateRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new ServiceMasterBal().Update(mainRequest);
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

            return ResponseProcess.GetSucessReason(ServiceMessage.UpdateSuccess);
        }

        [HttpPost]
        public APIResponse SelectByRechargeServices([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectByRechargeServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse SelectByRechargeDTH([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectByRechargeDTH();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse SelectChargeServices([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectChargeServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse SelectByServicesType([FromForm] APIRequest encRequest)
        {
            {
                KeyDataModel keyDataModel;
                ServiceTypeRequest mainRequest;
                try
                {
                    keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                    mainRequest = JsonConvert.DeserializeObject<ServiceTypeRequest>(ed.Decrypt(encRequest.Request));
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
                }

                string failureMessage = keyDataModel.IsValidModalState();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return ResponseProcess.GetFailReason(failureMessage);
                }

                List<Response.Service.ViewModel> list = null;

                try
                {
                    list = new ServiceMasterBal().SelectByServicesType(mainRequest);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
                }

                return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
            }
        }

        [HttpPost]
        public APIResponse SelectPrePaidDTHFRCService([FromForm] APIRequest encRequest)
        {
            {
                KeyDataModel keyDataModel;
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
                {
                    return ResponseProcess.GetFailReason(failureMessage);
                }

                List<Response.Service.ViewModel> list = null;

                try
                {
                    list = new ServiceMasterBal().SelectPrePaidDTHFRCService();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
                }

                return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
            }
        }

        [HttpPost]
        public APIResponse SelectByOfflineServices([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectByOfflineServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse SelectByPrimeServices([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectByPrimeServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse SelectForCallBackUrl([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectForCallBackUrl();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse DisplayOnOff([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            DisplayRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<DisplayRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                failureMessage = new ServiceMasterBal().Display(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Service DisplayOnOff", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Service DisplayOnOff", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(ServiceMessage.UpdateSuccess,
                keyDataModel.UserID, "Service DisplayOnOff", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse DefaultIsOnOff([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            DefaultRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<DefaultRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                failureMessage = new ServiceMasterBal().DefaultIsOnOff(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Service DefaultIsOnOff", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Service DefaultIsOnOff", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(ServiceMessage.UpdateSuccess,
                keyDataModel.UserID, "Service DefaultIsOnOff", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse OutsideIsOnOff([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            OutsideDefaultRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<OutsideDefaultRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                failureMessage = new ServiceMasterBal().OutsideIsOnOff(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Service OutsideIsOnOff", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Service OutsideIsOnOff", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(ServiceMessage.UpdateSuccess,
                keyDataModel.UserID, "Service OutsideIsOnOff", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse SubscriptionAllowOnOFF([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SubscriptionAllowRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SubscriptionAllowRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            try
            {
                failureMessage = new ServiceMasterBal().SubscriptionAllowOnOFF(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Service SubscriptionAllowOnOFF", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Service SubscriptionAllowOnOFF", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(ServiceMessage.UpdateSuccess,
                keyDataModel.UserID, "Service SubscriptionAllowOnOFF", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse SelectSubscriptionAllowedServices([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectSubscriptionAllowedServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse SelectRangeWiseServices([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectRangeWiseServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public APIResponse SelectBBPSEnabledServices([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectBBPSServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }
        [HttpPost]
        public APIResponse SelectServiceMoneyTransferAndUPITransfer([FromForm] APIRequest aPIrequest)
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

            List<Response.Service.ViewModel> operators;
            try
            {
                operators = new ServiceMasterBal().SelectServiceMoneyTransferAndUPITransfer();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (operators == null || operators.Count <= 0)
                return ResponseProcess.GetFailReason(LapuMasterMessage.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }

        [HttpPost]
        public APIResponse SelectSubscriptionServices([FromForm] APIRequest encRequest)
        {
            List<Response.Service.ViewModel> list = null;

            try
            {
                list = new ServiceMasterBal().SelectSubscriptionServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }
    }
}