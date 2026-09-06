using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        //[HttpGet]
        //public async Task Test()
        //{
        //}

        [Route("{providerID}/")]
        [HttpPost]
        public async Task postResponse(string providerID)
        {
            string response = "";
            using (var reader = new StreamReader(Request.Body))
            {
                response = await reader.ReadToEndAsync();
            }

            new Logger().Info("CallbackController ", "postResponse", "PID: " + providerID + ", callback called ");
            if (response == null || response.Trim().Length <= 0)
            {
                response = readFormData(Request.Form);

                if (response == null || response.Trim().Length <= 0)
                {
                    new Logger().Info("CallbackController ", "postResponse", "PID: " + providerID + ", Data not received");
                    return;
                }
            }

            new Logger().Info("CallbackController ", "postResponse", "PID: " + providerID + ", Data:" + response);

            ProcessCallback(providerID, response);
        }

        private string readFormData(Microsoft.AspNetCore.Http.IFormCollection from)
        {
            var param_keyValue = new List<string>();

            foreach (var item in from)
            {
                if (item.Key.Trim().Length <= 0)
                {
                    continue;
                }
                param_keyValue.Add($"{item.Key.Trim()}:{item.Value.ToString().Trim()}");
            }

            string response = string.Join(",", param_keyValue);
            return response;
        }

        [Route("{providerID}/")]
        [HttpGet]
        public string response(string providerID)
        {
            string message = "";
            try
            {
                string getData = Request.QueryString.ToString();

                if (getData == null || getData.Length <= 0)
                {
                    return "Blank data";
                }

                string urlwithHostname = providerID + "-->" + getData;
                string url = getData;
                url = url.Remove(0, 1);

                message = ProcessCallback(providerID.Trim(), url);

                new Logger().Info("CallbackController ", "GET Response", urlwithHostname + "::" + message);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }
            return message;
        }

        private string ProcessCallback(string providerID, string data)
        {
            int pid = new ProviderMasterBAL().SelectProviderIDByUniqueNo(providerID);
            CallbackProcessing callback = new CallbackProcessing();
            RandomNumberProcessing.RandomDelay(20, 302);

            if (pid <= 0)
            {
                return "Provider not found, please check callback url";
            }

            callback.PreExtractCallBack(pid, data.Trim());
            return "Callback received";
        }


        [HttpPost]
        public async Task<string> post_yesbank_upi_collection_response()
        {
            string response = "";
            using (var reader = new StreamReader(Request.Body))
            {
                response = await reader.ReadToEndAsync();
            }

            new Logger().Info("CallbackController ", "post_yesbank_upi_collection_response", response);

            if (response == null)
            {
                return StatusType.Failure;
            }

            return StatusType.Success;
        }



        [HttpPost]
        public async Task<string> post_response_yes_bank()
        {
            string response = "";
            using (var reader = new StreamReader(Request.Body))
            {
                response = await reader.ReadToEndAsync();
            }

            if (response.TrimIfNotNull().Length <= 0)
            {
                response = readFormData(Request.Form);
            }

            if (response == null || response.Length <= 0)
            {
                return StatusType.Failure;
            }

            new Logger().Info("CallbackController", "post_response_yes_bank", response.TrimIfNotNull());

            Processing.YesBank.PaymentGatewayProcessing pgProcessing = new Processing.YesBank.PaymentGatewayProcessing();
            YESBankCore.Response.StatusCheckResponse callbackResponse = pgProcessing.CallbackResponse(response);

            string orderID = "OrderID=" + callbackResponse.OrderNo;

            new Logger().Info("CallbackController ", "post_response_yes_bank-->", JsonConvert.SerializeObject(callbackResponse));

            if (callbackResponse.Status.ToLower().Contains(StatusType.Process.ToLower()))// process txn no need to update
            {
                return StatusType.Success;
            }

            PaymentGatewayProcessing paymentGatewayProcessing = new PaymentGatewayProcessing();

            using (ConcurrencyControl.AcquireLock(1, callbackResponse.OrderNo))
            {
                paymentGatewayProcessing.UpdateTransaction(callbackResponse.OrderNo, callbackResponse.Status, callbackResponse.TransactionID,
                    callbackResponse.Amount, PaymentModeCode.UPI, GatewayCode.YesBank, response);
            }

            new Logger().Info("CallbackController ", "post_response_yes_bank PGSuccess", Company.UIURL + "PGSuccess?" + orderID);

            return StatusType.Success;
        }
    }
}