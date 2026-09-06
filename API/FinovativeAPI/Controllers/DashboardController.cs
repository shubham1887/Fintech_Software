using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.TodaySummary;
using Project.Response;
using Project.Response.Dashboard;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public async Task<APIResponse> Select([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            DashboardViewModel list = null;

            try
            {
                (failureMessage, list) = await new DashboardBal().Select(keyDataModel.UserID, selectRequest.Date);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (list == null)
                return ResponseProcess.GetFailReason("No data found");

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public async Task<APIResponse> SelectForRetailer([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            DashboardViewModel list = null;

            try
            {
                (failureMessage, list) = await new DashboardBal().SelectForRetailer(keyDataModel.UserID, selectRequest.Date);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (list == null)
                return ResponseProcess.GetFailReason("No data found");

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        public async Task<APIResponse> SelectForDistributor([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(encRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            DashboardViewModel list = null;

            try
            {
                (failureMessage, list) = await new DashboardBal().SelectForDistributor(keyDataModel.UserID, selectRequest.Date);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (list == null)
                return ResponseProcess.GetFailReason("No data found");

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }
    }
}