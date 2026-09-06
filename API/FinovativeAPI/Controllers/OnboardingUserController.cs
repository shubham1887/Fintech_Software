using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [CustomAuthorization]

    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class OnboardingUserController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectReport([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.Onboarding.SelectRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.Onboarding.SelectRequest>(ed.Decrypt(encRequest.Request));
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

            Project.Response.Onboarding.OnboardingWrapper wrapper = new OnboardingUserBAL().SelectReport(mainRequest);

            if (!wrapper.Message.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(wrapper.Message));
            }
            if (mainRequest.IsPDFDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf("OnBoardingUserList", wrapper.list, keyDataModel.UserID);
            }
            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("OnBoardingUserList", wrapper.list, keyDataModel.UserID);
            }
            
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.Onboarding.SelectByIDRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.Onboarding.SelectByIDRequest>(ed.Decrypt(encRequest.Request));
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

            (Project.Response.Onboarding.OnboardingViewModel response, failureMessage) = new OnboardingUserBAL().SelectByID(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(failureMessage));
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.Onboarding.OnboardingRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.Onboarding.OnboardingRequest>(ed.Decrypt(encRequest.Request));
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

            failureMessage = new OnboardingUserBAL().Update(mainRequest, keyDataModel.UserID);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(failureMessage));
            }
            failureMessage = Message.OnboardingMessages.UpdateSuccess;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(failureMessage));
        }

        [HttpPost]
        public async Task<APIResponse> ApproveOnboarding([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.Onboarding.ApproveOnboardingUserRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.Onboarding.ApproveOnboardingUserRequest>(ed.Decrypt(encRequest.Request));
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

            failureMessage = await new OnboardingUserBAL().ApproveOnboarding(mainRequest, keyDataModel.UserID);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(failureMessage));
            }
            if (!mainRequest.Status.EqualsIgnoreCase(StaticModel.StatusType.Approve))
            {
                failureMessage = Message.OnboardingMessages.RejectSuccess;
                return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(failureMessage));
            }
            failureMessage = Message.OnboardingMessages.ApproveSuccess;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(failureMessage));
        }
        [HttpPost]
        public APIResponse ManualInsert([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.Onboarding.ManualInsertRequest mainRequest;

            string failureMessage = encRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.Onboarding.ManualInsertRequest>(ed.Decrypt(encRequest.Request));
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

            failureMessage = new OnboardingUserBAL().ManualInsert(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(failureMessage));
            }
            failureMessage = Message.OnboardingMessages.UpdateSuccess;
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(failureMessage));
        }

    }
}
