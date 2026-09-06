using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.SlabDetail;
using Project.Response;
using Project.Response.Slab;
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
    public class SlabDetailController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse BulkAdd([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            BulkAddRequest InsertRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                InsertRequest = JsonConvert.DeserializeObject<BulkAddRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabDetailBAL().BulkAdd(InsertRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(Message.SlabDetailMessage.InsertSuccess);
        }

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            InsertRequest InsertRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                InsertRequest = JsonConvert.DeserializeObject<InsertRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabDetailBAL().Insert(InsertRequest, false);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Commission insert", InsertRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, "Commission insert", InsertRequest.ToString());

            return ResponseProcess.GetSucessReason(Message.SlabDetailMessage.InsertSuccess,
                 keyDataModel.UserID, "Commission insert", InsertRequest.ToString());
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            UpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<UpdateRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string Action = "Commission update";
            try
            {
                failureMessage = new SlabDetailBAL().Update(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, Action, updateRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Action, updateRequest.ToString());

            return ResponseProcess.GetSucessReason(SlabDetailMessage.UpdateSuccess,
                 keyDataModel.UserID, Action, updateRequest.ToString());
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByIDRequest DeleteRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                DeleteRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabDetailBAL().Delete(DeleteRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Commission Deleted", DeleteRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Commission Deleted", DeleteRequest.ToString());

            return ResponseProcess.GetSucessReason(SlabDetailMessage.DeleteSuccess,
                keyDataModel.UserID, "Commission Deleted", DeleteRequest.ToString());
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            List<DetailViewModel> SlabDetaiList;
            try
            {
                SlabDetaiList = new SlabDetailBAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (SlabDetaiList == null || SlabDetaiList.Count <= 0)
                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SlabDetaiList));
        }

        [HttpPost]
        public APIResponse SelectBySlabDetailID([FromForm] APIRequest aPIRequest)
        {
            SelectByIDRequest SelectRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SelectRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            DetailViewModel slabDetailViewModel;

            try
            {
                slabDetailViewModel = new SlabDetailBAL().SelectBySlabDetailID(SelectRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            if (slabDetailViewModel == null) return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(slabDetailViewModel));
        }

        [HttpPost]
        public APIResponse SelectBySlabMasterID([FromForm] APIRequest aPIRequest)
        {
            SelectBySlabMasterIDRequest SelectRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SelectRequest = JsonConvert.DeserializeObject<SelectBySlabMasterIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<DetailViewModel> SlabDetaiList;
            try
            {
                SlabDetaiList = new SlabDetailBAL().SelectBySlabMasterID(SelectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (SlabDetaiList == null || SlabDetaiList.Count <= 0)

                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SlabDetaiList));
        }

        [HttpPost]
        public APIResponse SelectByUserIDWithService([FromForm] APIRequest aPIRequest)
        {
            SelectByUserIDWithService SelectRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SelectRequest = JsonConvert.DeserializeObject<SelectByUserIDWithService>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            SlabReportWrapper wrapper;
            try
            {
                wrapper = new SlabDetailBAL().SelectByUserIDWithService(SelectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);
            if (SelectRequest.IsPDFDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GeneratePdf("Comm_Report", wrapper.list, keyDataModel.UserID);
            }
            if (SelectRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Comm_Report", wrapper.list, keyDataModel.UserID);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse BulkUpdate([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            BulkUpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<BulkUpdateRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new SlabDetailBAL().BulkUpdate(updateRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(SlabDetailMessage.UpdateSuccess);
        }

        [HttpPost]
        public APIResponse SelectByServiceID([FromForm] APIRequest aPIRequest)
        {
            SelectBySlabMasterIDRequest SelectRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SelectRequest = JsonConvert.DeserializeObject<SelectBySlabMasterIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<DetailViewModel> SlabDetaiList;
            try
            {
                SlabDetaiList = new SlabDetailBAL().SelectByServiceID(SelectRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (SlabDetaiList == null || SlabDetaiList.Count <= 0)

                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SlabDetaiList));
        }

        [HttpPost]
        public APIResponse Activate([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectByIDRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabDetailBAL().Activate(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string returnMessage = updateRequest.IsActive ? Message.OperatorMessage.ActiveSuccess : Message.OperatorMessage.DeactiveSuccess;
            return ResponseProcess.GetSucessReason(returnMessage);
        }

        [HttpPost]
        public APIResponse ServicewiseBulkUpdate([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            BulkUpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<BulkUpdateRequest>(ed.Decrypt(aPIrequest.Request));
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
                failureMessage = new SlabDetailBAL().ServicewiseBulkUpdate(updateRequest, keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(SlabDetailMessage.UpdateSuccess);
        }

        [HttpPost]
        public APIResponse SelectForAdjustCommission([FromForm] APIRequest aPIRequest)
        {
            SelectAdjustCommissionRequest SelectRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SelectRequest = JsonConvert.DeserializeObject<SelectAdjustCommissionRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            List<DetailViewModel> SlabDetaiList;
            try
            {
                SlabDetaiList = new SlabDetailBAL().SelectForAdjustCommission(SelectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (SlabDetaiList == null || SlabDetaiList.Count <= 0)

                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SlabDetaiList));
        }

        [HttpPost]
        public APIResponse IncreaseDecreaseCommission([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            CommissionUpdateRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<CommissionUpdateRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string Action = "Commission update";
            try
            {
                failureMessage = new SlabDetailBAL().IncreaseDecreaseCommission(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, Action, updateRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Action, updateRequest.ToString());

            return ResponseProcess.GetSucessReason(SlabDetailMessage.UpdateCommSuccess,
                 keyDataModel.UserID, Action, updateRequest.ToString());
        }

        [HttpPost]
        public APIResponse UpdateAmountByID([FromForm] APIRequest aPIrequest)
        {
            KeyDataModel keyDataModel;
            UpdateAmountRequest updateRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIrequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<UpdateAmountRequest>(ed.Decrypt(aPIrequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            string Action = "Commission update";
            try
            {
                failureMessage = new SlabDetailBAL().UpdateAmountByID(updateRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, Action, updateRequest.ToString());
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                     keyDataModel.UserID, Action, updateRequest.ToString());

            return ResponseProcess.GetSucessReason(SlabDetailMessage.UpdateSuccess,
                 keyDataModel.UserID, Action, updateRequest.ToString());
        }

        [HttpPost]
        public APIResponse SelectSlabByUserIDWithService([FromForm] APIRequest aPIRequest)
        {
            SelectByUserIDWithService SelectRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SelectRequest = JsonConvert.DeserializeObject<SelectByUserIDWithService>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            UserwiseSlabReportWrapper wrapper;
            try
            {
                wrapper = new SlabDetailBAL().SelectSlabByUserIDWithService(SelectRequest, keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (SelectRequest.IsExcelDownload)
            {
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Comm_Report", wrapper.list, keyDataModel.UserID);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectUPICommissionByUserID([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectAmountRequest selectAmountRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectAmountRequest = JsonConvert.DeserializeObject<SelectAmountRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            PGCommissionViewModel ViewModel;
            try
            {
                ViewModel = new SlabDetailBAL().SelectUPICommissionByUserID(keyDataModel.UserID, selectAmountRequest.Amount);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (ViewModel == null)
                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ViewModel));
        }

        [HttpPost]
        public APIResponse SelectDMTChargeAndCommission([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            DMTChargeAndCommissionRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<DMTChargeAndCommissionRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            ChargeAndCommissionViewModel ViewModel;
            try
            {
                ViewModel = new SlabDetailBAL().SelectDMTChargeAndCommission(keyDataModel.UserID, mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (ViewModel == null)
                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ViewModel));
        }
        [HttpPost]
        public APIResponse SelectSlabCommissionByOperatorID([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            ChargeAndCommissionRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<ChargeAndCommissionRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }
            ChargeAndCommissionViewModel ViewModel;
            try
            {
                ViewModel = new SlabDetailBAL().SelectSlabCommissionByOperatorID(keyDataModel.UserID, mainRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            if (ViewModel == null)
                return ResponseProcess.GetFailReason(SlabDetailMessage.SlabDetailFound);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(ViewModel));
        }
    }
}