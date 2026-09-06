using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.UserwiseSaleTarget;
using Project.Response;
using Project.Response.UserwiseSaleTarget;
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
    public class UserwiseSaleTargetController : Controller
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            InsertRequest insertRequest;
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
                insertRequest = JsonConvert.DeserializeObject<InsertRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string Result = "";
            try
            {
                Result = new UserwiseSaleTargetBal().Insert(insertRequest, keyDataModel, out failureMessage);
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
            return ResponseProcess.GetSucessReason(Result);
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<ViewModel> response;
            try
            {
                response = new UserwiseSaleTargetBal().Select(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            DeleteRequest deleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<DeleteRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new UserwiseSaleTargetBal().Delete(deleteRequest);
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
            return ResponseProcess.GetSucessReason(UserwiseSaleTargetMessages.DeleteSucess);
        }

        [HttpPost]
        public APIResponse SaleTargetSelect([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SaleTargetSelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<SaleTargetSelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            SaleTargetWrapper wrapper;

            try
            {
                wrapper = new UserwiseSaleTargetBal().SaleTargetSelect(selectRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (selectRequest.IsExcelDownload)
            {
                string FileName = "UserwiseSaleTarget";

                wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, wrapper.list, keyDataModel.UserID);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse TransferCommission([FromForm] APIRequest encRequest)
        {
            KeyDataModel keyDataModel;
            List<TransferCommissionRequest> mainRequest;
            string failureMessage = encRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string IPAddress = "";
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(encRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<List<TransferCommissionRequest>>(ed.Decrypt(encRequest.Request));
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            string Result = "";
            try
            {
                Result = new UserwiseSaleTargetBal().TransferCommission(mainRequest, keyDataModel, IPAddress, out string failureMessge);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "TransferCommission", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "TransferCommission", mainRequest.ToString());

            return ResponseProcess.GetSucessReason(Result,
                 keyDataModel.UserID, "TransferCommission", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse SaleTargetSelectUserwise([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            UserwiseSaleTargetSelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<UserwiseSaleTargetSelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<SaleViewModel> response;

            try
            {
                response = new UserwiseSaleTargetBal().SaleTargetSelectUserwise(selectRequest, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(response));
        }
    }
}