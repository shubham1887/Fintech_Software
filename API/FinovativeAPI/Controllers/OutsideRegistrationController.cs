using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using project.StaticModel;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.User;
using Project.Response;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class OutsideRegistrationController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectState([FromForm] APIRequest aPIrequest)
        {
            List<StateViewModel> states = null;
            try
            {
                states = new StateBal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(states));
        }

        [HttpPost]
        public APIResponse SelectRoleForOutside([FromForm] APIRequest encRequest)
        {
            List<UserTypeViewModel> userTypes;
            try
            {
                userTypes = new UserTypeBal().SelectRoleForOutside();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(userTypes));
        }

        [HttpPost]
        public APIResponse SelectRoleForSignUp([FromForm] APIRequest encRequest)
        {
            List<UserTypeViewModel> userTypes;
            RoleForSignUpRequest roleRequest;

            try
            {
                roleRequest = JsonConvert.DeserializeObject<RoleForSignUpRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            try
            {
                userTypes = new UserTypeBal().SelectRoleForSignUp(roleRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(userTypes));
        }

        [HttpPost]
        public async Task<APIResponse> Insert([FromForm] APIRequest aPIRequest)
        {
            RegistrationRequest registrationRequest;
            string IPAddress;
            try
            {
                registrationRequest = JsonConvert.DeserializeObject<RegistrationRequest>(ed.Decrypt(aPIRequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            // CHECK CAPTCHA HERE
            var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(registrationRequest.CaptchaToken, IPAddress, registrationRequest.Device);
            if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);

            registrationRequest.ParentID = Company.AdminUserID;
            registrationRequest.JoiningByUserID = Company.AdminUserID;
            registrationRequest.RegistrationType = RegistrationType.OutsideSignUp;
            registrationRequest.SlabID = 0;
            registrationRequest.AllowRealAPI = false;
            registrationRequest.IsChannel = false;
            registrationRequest.IsTwoFA = false;

            string failureMessage;

            try
            {
                failureMessage = await new UserBal().Insert(registrationRequest, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.UserMessage.RegistrationSuccess);
        }

        [HttpPost]
        public APIResponse SelectHelplineNumber()
        {
            string HelplineNumber = "";
            try
            {
                HelplineNumber = new OutSideAPIBal().SelectHelplineNumber();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(HelplineNumber.ToString());
        }

        [HttpPost]
        public APIResponse SelectPlayStoreURL()
        {
            string PlayStoreURL = "";
            try
            {
                PlayStoreURL = new OutSideAPIBal().SelectPlayStoreURL();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(PlayStoreURL.ToString());
        }

        [HttpPost]
        public APIResponse FetchDetailsForPincodeForOutSide([FromForm] APIRequest aPIrequest)
        {
            FetchDetailsForPincodeRequest userRequest;
            try
            {
                userRequest = JsonConvert.DeserializeObject<FetchDetailsForPincodeRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage;
            FetchDetailsForPincodeResponse response;
            try
            {
                response = new UserBal().FetchDetailsForPincode(userRequest, out failureMessage);
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



        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> SignUpGenerateOTP([FromForm] APIRequest aPIrequest)
        {
            string failureMessage = aPIrequest.IsValidModalState();
            RegistrationRequest registrationRequest;
            string IPAddress;
            try
            {
                registrationRequest = JsonConvert.DeserializeObject<RegistrationRequest>(ed.Decrypt(aPIrequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(registrationRequest.CaptchaToken, IPAddress, registrationRequest.Device);
            if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);

           
            GenerateOTPResponse response = null;

            try
            {
                response = await new UserBal().SignUpGenerateOTP(registrationRequest, IPAddress, failureMessage);
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