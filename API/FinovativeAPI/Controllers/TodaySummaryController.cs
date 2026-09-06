using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.TodaySummary;
using Project.Response;
using Project.Response.TodaySummary;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Controllers
{
    //[Authorize]
    [CustomAuthorization]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class TodaySummaryController : ControllerBase
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
            List<TodaySummaryViewModel> list = null;
            try
            {
                (failureMessage, list) = await new TodaySummaryBal().Select(keyDataModel.UserID, selectRequest.Date);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

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

            List<TodaySummaryViewModel> list = null;
            try
            {
                (failureMessage, list) = await new TodaySummaryBal().SelectForRetailer(keyDataModel.UserID, selectRequest.Date);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

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

            List<TodaySummaryViewModel> list = null;
            try
            {
                (failureMessage, list) = await new TodaySummaryBal().SelectForDistributor(keyDataModel.UserID, selectRequest.Date);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(list));
        }
    }
}