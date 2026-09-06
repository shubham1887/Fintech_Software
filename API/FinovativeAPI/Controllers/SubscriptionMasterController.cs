using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class SubscriptionMasterController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse RenewSubscription([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = "";
            string RenovationURL = string.Empty;
            try
            {
                RenovationURL = "https:google.co.in";// new SubscriptionProcessing().SubscriptionRenovation(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(RenovationURL.Trim().ToString());
        }

        [HttpPost]
        public APIResponse SelectResponseMessage([FromForm] APIRequest aPIRequest)

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

            string returnresponse;

            try

            {
                returnresponse = "100";//new SubscriptionBal().SelectResponseMessage(keyDataModel.UserID);
            }
            catch (Exception ex)

            {
                new Logger().Write(ex);

                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(returnresponse));
        }
    }
}