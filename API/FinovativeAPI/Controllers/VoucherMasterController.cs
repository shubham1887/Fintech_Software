using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Response;
using Project.Response.VoucherMaster;
using Project.StaticModel;
using System;

namespace FinovativeAPI.Controllers
{
    [CustomAuthorization]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class VoucherMasterController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            Project.Requests.VoucherMaster.InsertRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.VoucherMaster.InsertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            try
            {
                failureMessage = new VoucherMasterBal().Insert(mainRequest,keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Project.Message.VoucherMasterMessage.VoucherMasterAdded);
        }
        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIRequest)
        {
            Project.Requests.VoucherMaster.UpdateRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.VoucherMaster.UpdateRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            try
            {
                failureMessage = new VoucherMasterBal().Update(mainRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Project.Message.VoucherMasterMessage.VoucherMasterUpdate);
        }
        [HttpPost]
        public APIResponse Reverse([FromForm] APIRequest aPIRequest)
        {
            Project.Requests.VoucherMaster.SelectByIDForVouchertRequest mainRequest;
            KeyDataModel keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.VoucherMaster.SelectByIDForVouchertRequest>(ed.Decrypt(aPIRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            string failureMessage;
            try
            {
                failureMessage = new VoucherMasterBal().Reverse(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Project.Message.VoucherMasterMessage.VoucherMasterDelete);
        }
        [HttpPost]
        public APIResponse SelectReport([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.VoucherMaster.VoucherMasterSelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.VoucherMaster.VoucherMasterSelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            VoucherMasterWrapper wrapper = new VoucherMasterWrapper();
            string failureMessage;

            if (keyDataModel == null || mainRequest == null) return ResponseProcess.GetFailReason(Messages.InValidRequest);
            try
            {
                wrapper = new VoucherMasterBal().SelectForVoucherReport(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }


            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Voucher_Report", wrapper.list, keyDataModel.UserID);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));

        }
        [HttpPost]
        public APIResponse VoucherMasterApproveReport([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.VoucherMaster.VoucherMasterSelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.VoucherMaster.VoucherMasterSelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            VoucherMasterWrapper wrapper = new VoucherMasterWrapper();
            string failureMessage;

            if (keyDataModel == null || mainRequest == null) return ResponseProcess.GetFailReason(Messages.InValidRequest);
            try
            {
                wrapper = new VoucherMasterBal().VoucherMasterApproveReport(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }


            if (mainRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Voucher", wrapper.list, keyDataModel.UserID);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));

        }
        [HttpPost]
        public APIResponse SelectByIDForVoucherApprove([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            Project.Requests.VoucherMaster.SelectByIDForVouchertRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<Project.Requests.VoucherMaster.SelectByIDForVouchertRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage;
            Project.Response.VoucherMaster.ViewModel response = new Project.Response.VoucherMaster.ViewModel();
            if (keyDataModel == null || mainRequest == null) return ResponseProcess.GetFailReason(Messages.InValidRequest);
            try
            {
                (response, failureMessage) = new VoucherMasterBal().SelectByIDForVoucherApprove(mainRequest);
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
