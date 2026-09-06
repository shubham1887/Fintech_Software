using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.ServiceAndUserwiseSubscription;
using Project.Response;
using Project.Response.User;
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
    public class ServiceSubscriptionPlanMasterController : ControllerBase
    {
        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIrequest)
        {
            List<ServiceSubscriptionPlan> states = null;
            try
            {
                states = new ServiceSubscriptionPlanMasterBal().Select();
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
