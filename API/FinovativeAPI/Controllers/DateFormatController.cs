using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response;
using Project.Response.DateFormat;
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
    public class DateFormatController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

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

            List<ViewModel> response = null;
            try
            {
                response = new DateFormatMasterBAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null || response.Count <= 0)
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}