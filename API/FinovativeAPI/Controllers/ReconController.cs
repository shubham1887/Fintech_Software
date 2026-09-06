using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Porject.BAL;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.AgentDetails;
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
    public class ReconController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();


        [HttpPost]
        public APIResponse Upload([FromForm] APIRequest aPIRequest)
        {
            UploadRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                string decriptReq = ed.Decrypt(aPIRequest.Request);
                mainRequest = JsonConvert.DeserializeObject<UploadRequest>(decriptReq);

                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(StaticModel.Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            int successfulUpdates = 0;
            int totalRecords = 0;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                failureMessage = new ReconBal().Upload(mainRequest, out failureMessage, out successfulUpdates, out totalRecords);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            failureMessage = $"{successfulUpdates} records updated out of {totalRecords}";

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(failureMessage));
        }
    }
}
