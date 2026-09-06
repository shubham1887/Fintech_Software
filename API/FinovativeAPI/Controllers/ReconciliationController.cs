using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.Reconciliation;
using Project.Response;
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
    public class ReconciliationController : ControllerBase
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

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<Project.Response.Reconciliation.ViewModel> Result = null;

            try
            {
                Result = new ReconciliationBal().Upload(mainRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Result));
        }
    }
}