using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.User;
using Project.Response;
using Project.Response.RefreshJWTToken;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class CredentialController : Controller
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();
        private readonly IConfiguration _config;

        public CredentialController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> Login([FromForm] APIRequest aPIRequest)
        {
            LoginRequest loginRequest;
            string IPAddress;
            try
            {
                string decryptedRequest = ed.Decrypt(aPIRequest.Request);
                loginRequest = JsonConvert.DeserializeObject<LoginRequest>(decryptedRequest);

                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage;

            LoginResponse loginResponse;
            try
            {
                
                // CHECK CAPTCHA HERE
                var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(loginRequest.CaptchaToken, IPAddress, loginRequest.Device);
               
                var LoginTask = new UserBal(_config).Login(loginRequest, IPAddress);

                await Task.WhenAll(CaptchaTask, LoginTask);

                failureMessage = LoginTask.Result.Item1;
                loginResponse = LoginTask.Result.Item2;

                if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            if (loginResponse == null) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidUserNameOrPassword);

            if (loginResponse.UserID != 0)
            {
                loginResponse.SecureToken = new TokenManager(_config).GenerateJSONWebToken(loginResponse);
                loginResponse.LoginIP = IPAddress;
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(loginResponse));
        }

        [AllowAnonymous]
        [HttpPost]
        public APIResponse VerifyTwoFAatLogin([FromForm] APIRequest aPIRequest)
        {
            VerifyTwoFARequest VerifyRequest;
            string IPAddress = string.Empty;
            try
            {
                string decryptedRequest = ed.Decrypt(aPIRequest.Request);
                VerifyRequest = JsonConvert.DeserializeObject<VerifyTwoFARequest>(decryptedRequest);
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage;
            LoginResponse response;
            try
            {
                response = new UserBal().VerifyTwoFAatLogin(VerifyRequest, IPAddress, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (response.UserID != 0)
            {
                response.SecureToken = new TokenManager(_config).GenerateJSONWebToken(response);
            }

            response.LoginIP = IPAddress;

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> ForgotPassword([FromForm] APIRequest aPIrequest)
        {
            ForgotPasswordRequest forgotPasswordRequest;
            string IPAddress;
            try
            {
                forgotPasswordRequest = JsonConvert.DeserializeObject<ForgotPasswordRequest>(ed.Decrypt(aPIrequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            // CHECK CAPTCHA HERE
            var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(forgotPasswordRequest.CaptchaToken, IPAddress, forgotPasswordRequest.Device);
            if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);

            string failureMessage;
            try
            {
                failureMessage = await new UserBal().ForgotPassword(forgotPasswordRequest, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.UserMessage.ForgotPasswordSuccess);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> ForgotPasswordGenerateOTP([FromForm] APIRequest aPIrequest)
        {
            ForgotPasswordRequest forgotPasswordRequest;
            string IPAddress;
            try
            {
                forgotPasswordRequest = JsonConvert.DeserializeObject<ForgotPasswordRequest>(ed.Decrypt(aPIrequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            // CHECK CAPTCHA HERE
            var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(forgotPasswordRequest.CaptchaToken, IPAddress, forgotPasswordRequest.Device);
            if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);

            string failureMessage;
            try
            {
                failureMessage = await new UserBal().ForgotPasswordGenerateOTP(forgotPasswordRequest, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.UserMessage.ForgotPasswordOTP);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<APIResponse> VerifyForgotPasswordOTP([FromForm] APIRequest aPIrequest)
        {
            ForgotPasswordOTPRequest mainRequest;
            string IPAddress;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<ForgotPasswordOTPRequest>(ed.Decrypt(aPIrequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            // CHECK CAPTCHA HERE
            var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(mainRequest.CaptchaToken, IPAddress, mainRequest.Device);
            if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);

            string failureMessage;
            try
            {
                failureMessage = await new UserBal().VerifyForgotPasswordOTP(mainRequest, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.UserMessage.ForgotPasswordSuccess);
        }

        [HttpPost]
        public APIResponse NewDeviceLogin([FromForm] APIRequest aPIRequest)
        {
            NewDeviceLoginRequest mainRequest;
            string IPAddress = string.Empty;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<NewDeviceLoginRequest>(ed.Decrypt(aPIRequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            LoginResponse response;
            try
            {
                response = new UserBal().NewDeviceLogin(mainRequest, IPAddress, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (response.UserID != 0)
            {
                response.SecureToken = new TokenManager(_config).GenerateJSONWebToken(response);
            }

            response.LoginIP = IPAddress;

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [AllowAnonymous]
        [HttpPost]
        public APIResponse RefreshJWTToken([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel; string IPAddress;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            ViewModel view = new ViewModel();
            LoginResponse member;
            try
            {
                member = new UserBal().RefreshJWTToken(keyDataModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (member == null)
                return ResponseProcess.GetFailReason(Messages.InvalidUser);

            if (member != null || member.UserID != 0)
            {
                view.RefreshToken = member.RefreshToken;
                view.SecureToken = new TokenManager(_config).GenerateJSONWebToken(member);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(view));
        }

        [HttpPost]
        public APIResponse InitialChangePassword([FromForm] APIRequest encRequest)
        {
            UpdatePasswordRequest updateRequest;
            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string IPAddress = "";
            try
            {
                updateRequest = JsonConvert.DeserializeObject<UpdatePasswordRequest>(ed.Decrypt(encRequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new UserBal().UpdatePassword(updateRequest, 0, IPAddress);
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

            return ResponseProcess.GetSucessReason(UserMessage.UpdatePasswordSuccess);
        }

        [HttpPost]
        public APIResponse Logout([FromForm] APIRequest aPIrequest)
        {
            KeyDataModelForLogout keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModelForLogout>(ed.Decrypt(aPIrequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            try
            {
                if (keyDataModel.UserID.Length <= 0)
                {
                    return ResponseProcess.GetSucessReason("");
                }

                new RefreshTokenBal().Remove(Convert.ToInt32(keyDataModel.UserID));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return ResponseProcess.GetSucessReason("");
        }

        [HttpPost]
        public APIResponse ChangePasswordAfterExpiry([FromForm] APIRequest encRequest)
        {
            ChangePasswordAfterExpiryRequest updateRequest;
            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string IPAddress = "";
            try
            {
                updateRequest = JsonConvert.DeserializeObject<ChangePasswordAfterExpiryRequest>(ed.Decrypt(encRequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                failureMessage = new UserBal().ChangePasswordAfterExpiry(updateRequest, IPAddress);
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

            return ResponseProcess.GetSucessReason(UserMessage.UpdatePasswordSuccess);
        }

        [HttpPost]
        public APIResponse FetchDetailsForPincode([FromForm] APIRequest encRequest)
        {
            FetchDetailsForPincodeRequest userRequest;
            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string IPAddress = "";
            try
            {
                userRequest = JsonConvert.DeserializeObject<FetchDetailsForPincodeRequest>(ed.Decrypt(encRequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

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

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

         
    }
}