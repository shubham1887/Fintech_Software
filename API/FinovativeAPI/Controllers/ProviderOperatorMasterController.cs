using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.Provider;
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
    public class ProviderOperatorMasterController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            ProviderOperatorMasterRequest InsertRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))

                return ResponseProcess.GetFailReason(failureMessage);

            try
            {
                string request = ed.Decrypt(aPIRequest.Request);
                InsertRequest = JsonConvert.DeserializeObject<ProviderOperatorMasterRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            try
            {
                failureMessage = new ProviderOperatorMasterBAL().Insert(InsertRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }
            return ResponseProcess.GetSucessReason(ProviderMessage.InsertProviderOperatorSuccess);
        }

        [HttpPost]
        public APIResponse SelectProviderOperator([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            ProviderOperatorRequest providerOperatorRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                providerOperatorRequest = JsonConvert.DeserializeObject<ProviderOperatorRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))

                return ResponseProcess.GetFailReason(failureMessage);

            List<ProviderOperatorView> operators;
            try
            {
                operators = new ProviderOperatorMasterBAL().SelectProviderOperator(providerOperatorRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (operators == null || operators.Count <= 0)
            {
                return ResponseProcess.GetFailReason(OperatorMessage.OperatorsNotFound);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }

        [HttpPost]
        public APIResponse ActiveDeactive([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            ProviderOperatorRequest providerOperatorRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                providerOperatorRequest = JsonConvert.DeserializeObject<ProviderOperatorRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            String returnresponse;
            try
            {
                returnresponse = new ProviderOperatorMasterBAL().ActiveDeactive(providerOperatorRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!returnresponse.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(returnresponse);
            }
            return ResponseProcess.GetSucessReason(OperatorMessage.OperatorActiveDeactiveSuccess);
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            ProviderOperatorUpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<ProviderOperatorUpdateRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string returnresponse;
            try
            {
                returnresponse = new ProviderOperatorMasterBAL().Update(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!returnresponse.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(returnresponse);

            return ResponseProcess.GetSucessReason(ProviderMessage.ProviderUpdateSuccess);
        }
    }
}