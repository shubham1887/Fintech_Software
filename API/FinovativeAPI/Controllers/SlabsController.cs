using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.Slab;
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
    public class SlabsController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Insert([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            InsertRequest slabMasterInsertRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                slabMasterInsertRequest = JsonConvert.DeserializeObject<InsertRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabBAL().Insert(slabMasterInsertRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Slab Insert", slabMasterInsertRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Slab Insert", slabMasterInsertRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(Message.SlabMessage.InsertSuccess,
                keyDataModel.UserID, "Slab Insert", slabMasterInsertRequest.ToString());
        }

        [HttpPost]
        public APIResponse Copy([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            CopyRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<CopyRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabBAL().Copy(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Slab Copied", mainRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Slab Copied", mainRequest.ToString());
            }

            return ResponseProcess.GetSucessReason(Message.SlabMessage.InsertSuccess,
                keyDataModel.UserID, "Slab Copied", mainRequest.ToString());
        }

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            List<MasterViewModel> SlabList = null;
            try
            {
                SlabList = new SlabBAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(SlabList));
        }

        [HttpPost]
        public APIResponse SelectSlabForExcel([FromForm] APIRequest aPIRequest)
        {
            Project.Requests.SlabDetail.SelectBySlabMasterIDRequest selectRequest;
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
                selectRequest = JsonConvert.DeserializeObject<Project.Requests.SlabDetail.SelectBySlabMasterIDRequest>(request);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            SlabExcelWrapper wrapper = null;
            try
            {
                wrapper = new SlabBAL().SelectSlabForExcel(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (wrapper.list == null || wrapper.list.Count <= 0)
                return ResponseProcess.GetFailReason(failureMessage);

            if (selectRequest.IsExcelDownload)
                wrapper.ReportPath = new ExcelProcessing().GenerateExcel("Slab", wrapper.list, keyDataModel.UserID);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost]
        public APIResponse SelectByID([FromForm] APIRequest aPIRequest)
        {
            SelectByIDRequest SlabRequest;
            KeyDataModel keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SlabRequest = JsonConvert.DeserializeObject<SelectByIDRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            string failureMessage = keyDataModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            MasterViewModel Slab;

            try
            {
                Slab = new SlabBAL().SelectByID(SlabRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailReason(failureMessage);

            if (Slab == null) return ResponseProcess.GetFailReason(ProviderMessage.ProviderNotFound);
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(Slab));
        }

        [HttpPost]
        public APIResponse Update([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            UpdateRequest SlabRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SlabRequest = JsonConvert.DeserializeObject<UpdateRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabBAL().Update(SlabRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Slab Update", SlabRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Slab Update", SlabRequest.ToString());

            return ResponseProcess.GetSucessReason(Message.SlabMessage.UpdateSuccess,
                 keyDataModel.UserID, "Slab Update", SlabRequest.ToString());
        }

        [HttpPost]
        public APIResponse Delete([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            DeleteRequest SlabRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SlabRequest = JsonConvert.DeserializeObject<DeleteRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabBAL().Delete(SlabRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                     keyDataModel.UserID, "Slab delete", SlabRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Slab delete", SlabRequest.ToString());

            return ResponseProcess.GetSucessReason(Message.SlabMessage.DeleteSuccess,
                keyDataModel.UserID, "Slab delete", SlabRequest.ToString());
        }

        [HttpPost]
        public APIResponse ActiveDeactive([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            ActiveDeactiveRequest SlabRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                SlabRequest = JsonConvert.DeserializeObject<ActiveDeactiveRequest>(ed.Decrypt(aPIRequest.Request));
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
                failureMessage = new SlabBAL().ActiveDeactive(SlabRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured,
                    keyDataModel.UserID, "Slab ActiveDeactive", SlabRequest.ToString());
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage,
                    keyDataModel.UserID, "Slab ActiveDeactive", SlabRequest.ToString());

            string returnMessage = SlabRequest.IsActive ? Message.SlabMessage.ActiveSuccess : Message.SlabMessage.DeactiveSuccess;
            return ResponseProcess.GetSucessReason(returnMessage,
                keyDataModel.UserID, "Slab ActiveDeactive", SlabRequest.ToString());
        }
    }
}