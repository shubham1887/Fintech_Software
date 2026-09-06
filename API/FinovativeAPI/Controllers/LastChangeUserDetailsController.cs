using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response;
using Project.StaticModel;
using System;
using System.Collections.Generic;
namespace Project.Controllers
{
    [CustomAuthorization]

    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class LastChangeUserDetailsController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();
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

            (List<Project.Response.Onboarding.LastChangeUserDetailsViewModel> response, failureMessage) = new LastChangeUserDetailsBAL().SelectByID(mainRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(JsonConvert.SerializeObject(failureMessage));
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}
