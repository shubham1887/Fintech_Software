using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response.Fund;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Route("[controller]/v1/[action]")]
    [ApiController]
    public class FundRequestAPIController : ControllerBase
    {
        [HttpGet]
        [ActionName("FundRequest")]
        public FundRequestAPIModel FundRequestByGET([FromQuery] KeyDataModelForAPIUser KeyData, [FromQuery] InsertRequest mainRequest)
        {
            return FundRequestCore(KeyData, mainRequest);
        }

        [HttpPost]
        public FundRequestAPIModel FundRequest([FromHeader] KeyDataModelForAPIUser KeyData, [FromBody] InsertRequest mainRequest)
        {
            return FundRequestCore(KeyData, mainRequest);
        }

        private FundRequestAPIModel FundRequestCore([FromHeader] KeyDataModelForAPIUser KeyData, [FromBody] InsertRequest mainRequest)
        {
            string IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            string failureMessage = KeyData.IsValidModalState(IPAddress);

            string RequestUrl = JsonConvert.SerializeObject(mainRequest).ToString();

            FundRequestAPIModel response = new FundRequestAPIModel();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;

                Task.Run(() =>
                {
                    new APIUserInitialRequestResponseLogBal().Insert(mainRequest.ParentID, "",
                                        KeyData.UserID, mainRequest.Amount.ToString(), RequestUrl, failureMessage);
                });

                return response;
            }

            KeyDataModel keyDataModel = new KeyDataModel
            {
                UserID = KeyData.UserID
            };
            int Result = 0;
            try
            {
                failureMessage = new FundBal().RequestInsert(mainRequest, keyDataModel.UserID, out Result);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.Status = StatusType.Process;
                response.Message = Message.FundMessage.RequestFailure;

                Task.Run(() =>
                {
                    new APIUserInitialRequestResponseLogBal().Insert(mainRequest.ParentID, "",
                                        KeyData.UserID, mainRequest.Amount.ToString(), RequestUrl, failureMessage);
                });

                return response;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = failureMessage;

                Task.Run(() =>
                {
                    new APIUserInitialRequestResponseLogBal().Insert(mainRequest.ParentID, "",
                                        KeyData.UserID, mainRequest.Amount.ToString(), RequestUrl, failureMessage);
                });

                return response;
            }

            response.Status = StatusType.Success;
            failureMessage = Message.FundMessage.TransactionRequestSuccess;
            response.Message = failureMessage;
            response.FundRequestID = Result;

            string result = JsonConvert.SerializeObject(response).ToString();

            Task.Run(() =>
            {
                new APIUserInitialRequestResponseLogBal().Insert(mainRequest.ParentID, "",
                                    keyDataModel.UserID, mainRequest.Amount.ToString(), RequestUrl, result);
            });

            return response;
        }
    }
}