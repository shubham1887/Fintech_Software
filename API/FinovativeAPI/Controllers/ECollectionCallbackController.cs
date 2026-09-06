using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.Processing;
using Project.Response.ECollection;
using System.IO;
using System.Threading.Tasks;
using static Project.Response.ECollection.ValidationResponse;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class ECollectionCallbackController : ControllerBase
    {
        [HttpPost]
        public  async Task<string> Validate_Response()
        {
            string response = "";

            using (var reader = new StreamReader(Request.Body))
            {
                response = await reader.ReadToEndAsync();

            }
            new Logger().Info("ValidateCallback", "post_Response", response);
            ECollectionProcessing eCollection = new ECollectionProcessing();
            ValidationResponse validateResponse = eCollection.Checkvalidate(response);
            return JsonConvert.SerializeObject(validateResponse);
        }
        
        [HttpPost]
        public async Task<string> Notify_Response()
        {
            string response = "";

            using (var reader = new StreamReader(Request.Body))
            {
                response = await reader.ReadToEndAsync();

            }
            new Logger().Info("NotifyCallback", "post_Response", response);
            ECollectionProcessing eCollection = new ECollectionProcessing();
            NotifyResponse notifyResponse = eCollection.YesBank_Response_CallBack(response);
            return JsonConvert.SerializeObject(notifyResponse);

        }

    }
}
