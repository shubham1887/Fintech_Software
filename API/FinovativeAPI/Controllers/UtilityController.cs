using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.Processing;
using Project.Requests;
using Project.Requests.Utility;
using Project.Response;
using Project.Response.Utility;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse BillFetch([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = aPIRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            BillFetchRequest mainRequest;
            KeyDataModel keyDataModel; string IPAddress = "";
            try
            {
                mainRequest = JsonConvert.DeserializeObject<BillFetchRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            FetchViewModel response;
            try
            {
                response = UtilityProcessing.Fetch(mainRequest, keyDataModel, IPAddress, out failureMessage);
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