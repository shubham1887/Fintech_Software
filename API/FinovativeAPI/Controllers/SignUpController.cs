using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.SignUp;
using Project.Response;
using Project.Response.SignUp;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();
       
        [HttpPost]
        public APIResponse GenerateSignUpOTP([FromForm] APIRequest aPIrequest)
        {
            string failureMessage = aPIrequest.IsValidModalState();
            SignUpRequest request;
            string IPAddress;
            try
            {
                request = JsonConvert.DeserializeObject<SignUpRequest>(ed.Decrypt(aPIrequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(request.CaptchaToken, IPAddress, request.Device);
            if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);


            GenerateOTPResponse response = null;
            try
            {
                response =  new SignUpBal().GenerateSignUpOTP(request, IPAddress,out failureMessage);
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
        [HttpPost]
        public APIResponse VerifySignUpOTP([FromForm] APIRequest aPIrequest)
        {
            string failureMessage = aPIrequest.IsValidModalState();
            SignUpVerifyRequest request;
            string IPAddress;
            try
            {
                request = JsonConvert.DeserializeObject<SignUpVerifyRequest>(ed.Decrypt(aPIrequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(request.CaptchaToken, IPAddress, request.Device);
            if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);


            VerifyOTPResponse response = null;
            try
            {
                response = new SignUpBal().VerifySignUpOTP(request, IPAddress,out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            response.Message = Message.UserMessage.SignUpVerifyOTP;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse SignUp([FromForm] APIRequest aPIrequest)
        {
            string failureMessage = aPIrequest.IsValidModalState();
            SignUpVerifyRequest request;
            string IPAddress;
            try
            {
                request = JsonConvert.DeserializeObject<SignUpVerifyRequest>(ed.Decrypt(aPIrequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            var CaptchaTask = new CaptchaProcessing().IsValidCaptcha(request.CaptchaToken, IPAddress, request.Device);
            if (!CaptchaTask.Result) return ResponseProcess.GetFailReason(Message.UserMessage.InvalidCaptcha);


            VerifyOTPResponse response = null;
            try
            {
                response = new SignUpBal().SignUp(request, IPAddress, out failureMessage);
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

        [HttpPost]
        public APIResponse SelectReport([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectReportRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectReportRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<SelectReportResponse> members;
            
            Response.SignUp.UserWrapper wrapper = null;
            try
            {
                wrapper = new SignUpBal().SelectReport(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (wrapper == null)
            {
                failureMessage = LogMessage.NoRecord;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(failureMessage));
            }
            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Users", wrapper.list, keyDataModel.UserID);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectForFailedSignup([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectReportRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectReportRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<SelectFailSignUpResponse> members;

            Response.SignUp.FailedUserWrapper wrapper = null;
            try
            {
                wrapper = new FailedSignUpDataBal().SelectReport(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (wrapper == null)
            {
                failureMessage = LogMessage.NoRecord;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(failureMessage));
            }
            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("FailedSignUpUsers", wrapper.list, keyDataModel.UserID);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }
    }
}
