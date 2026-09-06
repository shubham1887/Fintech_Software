using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.Operator;
using Project.Requests.PublicAPI;
using Project.Requests.ROffer;
using Project.Response;
using Project.Response.Operator;
using Project.Response.PublicAPI;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class PublicAPIController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectOperator([FromForm] APIRequest aPIRequest)
        {
            SelectOperatorRequest selectRequest;
            try
            {
                selectRequest = JsonConvert.DeserializeObject<SelectOperatorRequest>((aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<ViewModel> obj;
            try
            {
                obj = new OperatorMasterBAL().SelectOperatorByServiceID(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReasonForAPIUser(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReasonForAPIUser(Message.OperatorMessage.InvalidServiceID);

            return ResponseProcess.GetSuccessReasonForAPIUser(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse GetOperatorByMobileNumber([FromForm] APIRequest aPIRequest)
        {
            GetOperatorByMobileNumberRequest selectRequest;
            try
            {
                selectRequest = JsonConvert.DeserializeObject<GetOperatorByMobileNumberRequest>((aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage = "";
            GetOperatorByMobileNumberResponse obj;
            try
            {
                obj = new OperatorMasterBAL().GetOperatorByMobileNumber(selectRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReasonForAPIUser(Messages.ExceptionOccured);
            }

            if (obj == null)
                return ResponseProcess.GetFailReasonForAPIUser(failureMessage);

            return ResponseProcess.GetSuccessReasonForAPIUser(JsonConvert.SerializeObject(obj));
        }

        [HttpPost]
        public APIResponse SelectROffer([FromForm] APIRequest aPIRequest)
        {
            SelectOfferRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectOfferRequest>((aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReasonForAPIUser(Messages.ExceptionOccured + Messages.InValidRequest);
            }

            string failureMessage;

            List<MplanCore.Response.ROfferResponse.Record> records;
            try
            {
                records = new ROfferBal().SelectOffer(mainRequest, out failureMessage);
            }
            catch (NullReferenceException ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReasonForAPIUser(Messages.ExceptionOccured);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReasonForAPIUser(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReasonForAPIUser(Message.ROffer.PlanNotFound);

            if (records == null)
                return ResponseProcess.GetFailReasonForAPIUser(Message.ROffer.PlanNotFound);

            return ResponseProcess.GetSuccessReasonForAPIUser(JsonConvert.SerializeObject(records));
        }

        [HttpPost]
        public APIResponse SelectPlan([FromForm] APIRequest aPIRequest)
        {
            SelectPlanRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectPlanRequest>((aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReasonForAPIUser(Messages.ExceptionOccured + Messages.InValidRequest);
            }

            string failureMessage;

            MplanCore.Response.Records records;
            try
            {
                records = new ROfferBal().SelectPlan(mainRequest, out failureMessage);
            }
            catch (NullReferenceException ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReasonForAPIUser(Messages.ExceptionOccured);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReasonForAPIUser(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReasonForAPIUser(Message.ROffer.PlanNotFound);

            if (records == null)
                return ResponseProcess.GetFailReasonForAPIUser(Message.ROffer.PlanNotFound);

            return ResponseProcess.GetSuccessReasonForAPIUser(JsonConvert.SerializeObject(records));
        }
    }
}