using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MplanCore.Response;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.ROffer;
using Project.Response;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Controllers
{
    [CustomAuthorization]
    //[Authorize]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class ROfferController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse SelectPlan([FromForm] APIRequest aPIRequest)
        {
            SelectPlanRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectPlanRequest>(ed.Decrypt(aPIRequest.Request));
                KeyDataModel keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured + Messages.InValidRequest);
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
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            if (records == null)
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(records));
        }

        [HttpPost]
        public APIResponse SelectOffer([FromForm] APIRequest aPIRequest)
        {
            SelectOfferRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectOfferRequest>(ed.Decrypt(aPIRequest.Request));
                KeyDataModel keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured + Messages.InValidRequest);
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
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            if (records == null)
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(records));
        }

        [HttpPost]
        public APIResponse SelectDTHPlan([FromForm] APIRequest aPIRequest)
        {
            SelectDTHPlanRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectDTHPlanRequest>(ed.Decrypt(aPIRequest.Request));
                KeyDataModel keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured + Messages.InValidRequest);
            }

            string failureMessage;

            DTHRecords records;
            try
            {
                records = new ROfferBal().SelectDTHPlan(mainRequest, out failureMessage);
            }
            catch (NullReferenceException ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            if (records == null)
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(records));
        }

        [HttpPost]
        public APIResponse SelectDTHCustInfo([FromForm] APIRequest aPIRequest)
        {
            SelectDTHCustInfoRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectDTHCustInfoRequest>(ed.Decrypt(aPIRequest.Request));
                KeyDataModel keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured + Messages.InValidRequest);
            }

            string failureMessage;

            SelectCustomerInfoResponse records;
            try
            {
                records = new ROfferBal().SelectDTHCustInfo(mainRequest, out failureMessage);
            }
            catch (NullReferenceException ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            if (records == null)
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(records));
        }

        [HttpPost]
        public APIResponse HeavyRefresh([FromForm] APIRequest aPIRequest)
        {
            SelectDTHCustInfoRequest mainRequest;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectDTHCustInfoRequest>(ed.Decrypt(aPIRequest.Request));
                KeyDataModel keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured + Messages.InValidRequest);
            }

            string failureMessage;

            HeavyRefreshResult response;
            try
            {
                response = new ROfferBal().HeavyRefresh(mainRequest, out failureMessage);
            }
            catch (NullReferenceException ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            if (response == null)
                return ResponseProcess.GetFailReason(Message.ROffer.PlanNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}