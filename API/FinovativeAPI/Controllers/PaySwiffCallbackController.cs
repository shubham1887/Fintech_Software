using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Project.BAL;
using Project.Processing;
using Project.StaticModel;
using System.IO;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class PaySwiffCallbackController : ControllerBase
    {
        [HttpPost]
        public async Task post_PaySwiff_Response()
        {
            string response = "";

            using (var reader = new StreamReader(Request.Body))
            {
                response = await reader.ReadToEndAsync();
            }

            new Logger().Info("CallbackController ", "post_PaySwiff_Response", response);

            int logID = new ProviderCallBackLogBal().Insert(Company.PaySwiffProviderID, response);

            string result = new MicroATMBal().PaySwiff_MicroATMCallbackUpdate(response);

            new Logger().Info("CallbackController ", "post_PaySwiff result", result);

            Task.Run(() => new ProviderCallBackLogBal().Insert(Company.PaySwiffProviderID, response + "  DB result:" + result));
        }
    }
}  