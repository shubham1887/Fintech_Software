using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.PlanAPI;
using Project.StaticModel;
using System;
using System.Text.Json;

namespace Project.Controllers
{
    [Route("[controller]/v1/[action]")]
    [ApiController]
    public class PlanAPIController : ControllerBase
    {
        public class CapitalizeFirstCharPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if(name.EqualsIgnoreCase("FULLTT"))
                {
                    return "FullTalkTime";
                }
                if (name.EqualsIgnoreCase("_3G4G"))
                {
                    return "3G4G";
                }

                if (name.EqualsIgnoreCase("_2G"))
                {
                    return "2G";
                }

                if (name.EqualsIgnoreCase("Romaing"))
                {
                    return "Roaming";
                }


                return name.CapitalizeFirstChar();  
            }
        }

        [HttpGet]
        public IActionResult GetSelectPlan([FromQuery] KeyDataModelForAPIUser KeyData, [FromQuery] PlanRequest planRequest)
        {
            var model = SelectPlanCore(KeyData, planRequest);

            var options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = new CapitalizeFirstCharPolicy();

            var json = System.Text.Json.JsonSerializer.Serialize(model, options);

            return Ok(json);
        }
        [HttpPost]
        public IActionResult SelectPlan([FromHeader] KeyDataModelForAPIUser KeyData, [FromBody] PlanRequest planRequest)
        {
            var model = SelectPlanCore(KeyData, planRequest);

            var options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = new CapitalizeFirstCharPolicy();

            var json = System.Text.Json.JsonSerializer.Serialize(model, options);

            return Ok(json);
        }
        private MplanCore.Response.Records SelectPlanCore(KeyDataModelForAPIUser KeyData, PlanRequest planRequest)
        {
            string IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

           
            MplanCore.Response.Records response = new MplanCore.Response.Records();


            response.Message = TransactionMessage.PlanNotFound;

            if (KeyData == null)
            {
                response.Message = TransactionMessage.InvalidHeader;
                return response;
            }
            string failureMessage = KeyData.IsValidModalState(IPAddress);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            try
            {
                response = new PlanAPIBal().SelectPlan(planRequest, KeyData.Mobile, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            return response;

        }

        [HttpGet]
        public Response.PlanAPI.ROfferResponse GetSelectOffer([FromQuery] KeyDataModelForAPIUser KeyData, [FromQuery] OfferRequest offerRequest)
        {
            return SelectOfferCore(KeyData, offerRequest);
        }
        [HttpPost]
        public Response.PlanAPI.ROfferResponse SelectOffer([FromHeader] KeyDataModelForAPIUser KeyData, [FromBody] OfferRequest offerRequest)
        {
            return SelectOfferCore(KeyData, offerRequest);
        }
        Response.PlanAPI.ROfferResponse SelectOfferCore(KeyDataModelForAPIUser KeyData, OfferRequest offerRequest)
        {
            string IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            Response.PlanAPI.ROfferResponse response = new Response.PlanAPI.ROfferResponse();

            if (KeyData == null)
            {
                response.Message = TransactionMessage.InvalidHeader;
                return response;
            }
            string failureMessage = KeyData.IsValidModalState(IPAddress);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }
            try
            {
                response = new PlanAPIBal().SelectOffer(offerRequest,KeyData.Mobile, out failureMessage);
            }

            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }
            return response;
        }

        [HttpGet]
        public MplanCore.Response.DTHRecords GetSelectDTHPlan([FromQuery] KeyDataModelForAPIUser KeyData, [FromQuery] DTHPlanRequest dthPlanRequest)
        {
            return SelectDTHPlanCore(KeyData, dthPlanRequest);
        }
        [HttpPost]
        public MplanCore.Response.DTHRecords SelectDTHPlan([FromHeader] KeyDataModelForAPIUser KeyData, [FromBody] DTHPlanRequest dthPlanRequest)
        {
            return SelectDTHPlanCore(KeyData, dthPlanRequest);
        }
        private MplanCore.Response.DTHRecords SelectDTHPlanCore(KeyDataModelForAPIUser KeyData, DTHPlanRequest dthPlanRequest)
        {
            string IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            MplanCore.Response.DTHRecords response = new MplanCore.Response.DTHRecords();

            
            response.Message = TransactionMessage.PlanNotFound;


            if (KeyData == null)
            {
                response.Message = TransactionMessage.InvalidHeader;
                return response;
            }
            string RequestUrl = JsonConvert.SerializeObject(dthPlanRequest).ToString();

            string failureMessage = KeyData.IsValidModalState(IPAddress);

          

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            try
            {
                response = new PlanAPIBal().SelectDTHPlan(dthPlanRequest, KeyData.Mobile, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }


            return response;
        }

        [HttpGet]
        public MplanCore.Response.SelectCustomerInfoResponse GetSelectDTHCustInfo([FromQuery] KeyDataModelForAPIUser KeyData, [FromQuery] DTHCustInfoRequest dthCustInfoRequest)
        {
            return SelectDTHCustInfoCore(KeyData, dthCustInfoRequest);
        }
        [HttpPost]
        public MplanCore.Response.SelectCustomerInfoResponse SelectDTHCustInfo([FromHeader] KeyDataModelForAPIUser KeyData, [FromBody] DTHCustInfoRequest dthCustInfoRequest)
        {
            return SelectDTHCustInfoCore(KeyData, dthCustInfoRequest);
        }
        private MplanCore.Response.SelectCustomerInfoResponse SelectDTHCustInfoCore(KeyDataModelForAPIUser KeyData, DTHCustInfoRequest dthCustInfoRequest)
        {
            string IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            MplanCore.Response.SelectCustomerInfoResponse response = new MplanCore.Response.SelectCustomerInfoResponse();

            response.Message = TransactionMessage.DTHNotFound;

            if (KeyData == null)
            {
                response.Message = TransactionMessage.InvalidHeader;
                return response;
            }

            string failureMessage = KeyData.IsValidModalState(IPAddress);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            try
            {
                response = new PlanAPIBal().SelectDTHCustInfo(dthCustInfoRequest, KeyData.Mobile, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

           
            return response;

        }

    }
}