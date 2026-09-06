using Microsoft.AspNetCore.Mvc;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.CircleAPI;
using Project.Response.CircleAPIUser;
using Project.StaticModel;
using System;

namespace Project.Controllers
{
    [Route("[controller]/v1/[action]")]
    [ApiController]
    public class CircleApiUserController : ControllerBase
    {
        [HttpGet]
        public CircleResponse GetCircleApi([FromQuery] KeyDataModelForAPIUser KeyData, [FromQuery] SelectCircleRequest mainRequest)
        {
            return CircleApi(KeyData, mainRequest);
        }
        [HttpPost]
        public CircleResponse CircleAPI([FromHeader] KeyDataModelForAPIUser KeyData, [FromBody] SelectCircleRequest mainRequest)
        {
            return CircleApi(KeyData, mainRequest);
        }
        private CircleResponse CircleApi(KeyDataModelForAPIUser KeyData, SelectCircleRequest mainRequest)
        {
            string IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            CircleResponse response = new CircleResponse();

            if (KeyData == null)
            {
                response.Message = TransactionMessage.InvalidHeader;
                response.Status = StatusType.Failure;
                return response;
            }
            string failureMessage = KeyData.IsValidModalState(IPAddress);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                response.Status = StatusType.Failure;
                return response;
            }

            try
            {
                response = new CircleAPIUserBal().CircleApi(mainRequest, KeyData.Mobile, out failureMessage);
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
