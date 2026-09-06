using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response;
using Project.Response.VersionHistory;
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
    public class VersionHistoryController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse VersionHistorySelect([FromForm] APIRequest aPIRequest)
        {
            List<VersionHistoryViewModel> Versionhistory = null;
            string failureMessage = "";

            try
            {
                Versionhistory = new VersionHistoryBal().VersionHistorySelect(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (Versionhistory == null || Versionhistory.Count <= 0)
                return ResponseProcess.GetFailReason(Messages.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Versionhistory));
        }
    }
}