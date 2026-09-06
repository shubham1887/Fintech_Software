using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response;
using Project.Response.GSTState;
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
    public class ZoneMasterController : ControllerBase
    {
        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIrequest)
        {
            List<ZoneViewModel> states = null;
            try
            {
                states = new ZoneMasterBal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(states));
        }
    }
}