using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.Operator;
using Project.Response;
using Project.Response.Operator;
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
    public class OperatorController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            OperatorInsertRequest ObjOperatorInsertRequest;
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
                ObjOperatorInsertRequest = JsonConvert.DeserializeObject<OperatorInsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            try
            {
                failureMessage = new OperatorMasterBAL().Insert(ObjOperatorInsertRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(OperatorMessage.InsertOperatorSuccess);
        }

        [HttpPost]
        public APIResponse SelectByServiceID([FromForm] APIRequest aPIRequest)
        {
            OperatorSelectByServiceIDRequest operatorSelectByService;
            KeyDataModel keyDataModel;
            try
            {
                operatorSelectByService = JsonConvert.DeserializeObject<OperatorSelectByServiceIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<ViewModelForDropDown> operators;
            string failureMessage;

            try
            {
                operators = new OperatorMasterBAL().SelectByServiceID(operatorSelectByService, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (operators == null || operators.Count <= 0)
                return ResponseProcess.GetFailReason(OperatorMessage.OperatorsNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }
        [HttpPost]
        public APIResponse SelectAEPSOperator([FromForm] APIRequest aPIRequest)
        {
            OperatorSelectByServiceIDRequest operatorSelectByService;
            KeyDataModel keyDataModel;
            try
            {
                operatorSelectByService = JsonConvert.DeserializeObject<OperatorSelectByServiceIDRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<ViewModelForDropDown> operators;
            string failureMessage;

            try
            {
                operators = new OperatorMasterBAL().SelectAEPSOperator(operatorSelectByService, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (operators == null || operators.Count <= 0)
                return ResponseProcess.GetFailReason(OperatorMessage.OperatorsNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }

        [HttpPost]
        public APIResponse SelectByOperatorID([FromForm] APIRequest aPIRequest)
        {
            OperatorDeleteRequest selectrequest;
            try
            {
                selectrequest = JsonConvert.DeserializeObject<OperatorDeleteRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            ViewModel operators;
            string failureMessage;
            try
            {
                operators = new OperatorMasterBAL().SelectByOperatorID(selectrequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (operators == null)
            {
                return ResponseProcess.GetFailReason(failureMessage);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }

        [HttpPost]
        public APIResponse SelectOperator([FromForm] APIRequest aPIRequest)
        {
            SelectOperatorRequest mainRequest;
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
                mainRequest = JsonConvert.DeserializeObject<SelectOperatorRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<ViewModel> operators;

            try
            {
                operators = new OperatorMasterBAL().SelectOperatorByServiceID(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (operators == null || operators.Count <= 0) return ResponseProcess.GetFailReason(OperatorMessage.OperatorsNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }

        [HttpPost]
        public APIResponse SelectOperatorForExcel([FromForm] APIRequest aPIRequest)
        {
            OperatorRequest mainRequest;
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
                mainRequest = JsonConvert.DeserializeObject<OperatorRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            OperatorWrapper wrapper = null;
            try
            {
                wrapper = new OperatorMasterBAL().SelectOperatorForExcel(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (wrapper.list == null || wrapper.list.Count <= 0)
                return ResponseProcess.GetFailReason(failureMessage);

            if (mainRequest.IsExcelDownload)
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Operator", wrapper.list, keyDataModel.UserID);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            OperatorDeleteRequest deleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<OperatorDeleteRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new OperatorMasterBAL().Delete(deleteRequest);
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
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(OperatorMessage.OperatorDeleteSuccess));
        }

        [HttpPost]
        public APIResponse ActiveDeactive([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            OperatorDeleteRequest deleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<OperatorDeleteRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new OperatorMasterBAL().ActiveDeactive(deleteRequest);
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

            string returnMessage = deleteRequest.IsActive ? Message.OperatorMessage.ActiveSuccess : Message.OperatorMessage.DeactiveSuccess;
            return ResponseProcess.GetSucessReason(returnMessage);
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            OperatorUpdateRequest UpdateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                UpdateRequest = JsonConvert.DeserializeObject<OperatorUpdateRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new OperatorMasterBAL().Update(UpdateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(OperatorMessage.OperatorUpdateSuccess);
        }

        [HttpPost]
        public APIResponse SelectBBPSOperator([FromForm] APIRequest aPIRequest)
        {
            OperatorSelectByServiceIDRequest operatorSelectByService;
            try
            {
                operatorSelectByService = JsonConvert.DeserializeObject<OperatorSelectByServiceIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            List<ViewModelForDropDown> operators;
            string failureMessage;

            try
            {
                operators = new OperatorMasterBAL().SelectBBPSOperator(operatorSelectByService, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (operators == null || operators.Count <= 0)
                return ResponseProcess.GetFailReason(OperatorMessage.OperatorsNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }

        [HttpPost]
        public APIResponse SelectOperatorForDropDown([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            SelectOperatorForDropDownRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectOperatorForDropDownRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<ViewModelDropDownResponse> operators;
            try
            {
                if (mainRequest == null)
                {
                    mainRequest = new SelectOperatorForDropDownRequest();
                }

                operators = new OperatorMasterBAL().SelectForDropDown(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }

        [HttpPost]
        public APIResponse SelectOperatorPrepaidAndDTH([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<ViewModelDropDownResponse> operators;
            try
            {
                operators = new OperatorMasterBAL().SelectOperatorPrepaidAndDTH();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (operators == null || operators.Count <= 0)
                return ResponseProcess.GetFailReason(LapuMasterMessage.DataNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }

        [HttpPost]
        public APIResponse SelectOperatorWithTollFreeDetails([FromForm] APIRequest aPIRequest)
        {
            OperatorSelectByServiceIDRequest operatorSelectByService;
            try
            {
                operatorSelectByService = JsonConvert.DeserializeObject<OperatorSelectByServiceIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
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

            List<TollFreeDetailsViewModel> operators;

            try
            {
                operators = new OperatorMasterBAL().SelectOperatorWithTollFreeDetails(operatorSelectByService);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (operators == null || operators.Count <= 0)
                return ResponseProcess.GetFailReason(OperatorMessage.OperatorsNotFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(operators));
        }
    }
}