using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class OnboardingCallbackController : ControllerBase
    {
        [HttpPost]
        public async Task<string> post_Response()
        {
            string response = "";

            using (var reader = new StreamReader(Request.Body))
            {
                response = await reader.ReadToEndAsync();
            }


            Project.Requests.Onboarding.OnboardingDataRequest request = new Requests.Onboarding.OnboardingDataRequest();


            new Logger().Info("OnboardingCallback", "post_Response", response);

            try
            {
                request = JsonConvert.DeserializeObject<Project.Requests.Onboarding.OnboardingDataRequest>(response);
            }
            catch (Exception ex)
            {
                new Logger().Info("OnboardingCallback", "post_Response", response);
            }
            Task.Run(() => response = new OnboardingCallbackBal().OnboardingUserInsert(request));
          
             response = "Webhook Receive";
            return response;


        }
    }
}
