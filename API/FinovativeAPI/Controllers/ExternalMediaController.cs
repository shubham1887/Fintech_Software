using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.ExternalMedia;
using Project.Response;
using Project.Response.ExternalMedia;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Controllers
{
    [CustomAuthorization]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]

    public class ExternalMediaController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();
        [HttpPost]
        public APIResponse Select([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectrequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                selectrequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            ViewModel response;
            try
            {
                response = new ExternalMediaBal().Select(selectrequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (response == null)
            {
                return ResponseProcess.GetFailReason(Messages.DataNotFound);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

    }
}
