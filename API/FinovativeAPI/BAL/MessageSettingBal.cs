using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.MessageSetting;
using Project.Response.MessageSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class MessageSettingBal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            try
            {
                viewModel = new MessageSettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return viewModel;
        }

        internal string IsSMSActiveDeactive(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            MessageSetting ObjModel = null;
            try
            {
                ObjModel = new MessageSettingDal().SelectByID(updateRequest.MessageSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.IsSMS = updateRequest.IsSMS;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new MessageSettingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return MessageSettingMessage.updateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string IsMailActiveDeactive(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            MessageSetting ObjModel = null;
            try
            {
                ObjModel = new MessageSettingDal().SelectByID(updateRequest.MessageSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.IsMail = updateRequest.IsMail;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new MessageSettingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return MessageSettingMessage.updateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string IsWhatsUpActiveDeactive(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            MessageSetting ObjModel = null;
            try
            {
                ObjModel = new MessageSettingDal().SelectByID(updateRequest.MessageSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.IsWhatsUp = updateRequest.IsWhatsUp;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new MessageSettingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return MessageSettingMessage.updateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            MessageSetting ObjModel = null;
            try
            {
                ObjModel = new MessageSettingDal().SelectByID(updateRequest.MessageSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.SmsMessage = updateRequest.SmsMessage;
            ObjModel.EmailMessage = updateRequest.EmailMessage;
            ObjModel.TemplateID = updateRequest.TemplateID;
            ObjModel.WhatsUpMessage = updateRequest.WhatsUpMessage;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new MessageSettingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return MessageSettingMessage.updateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal Wrapper SelectForFilter(SelectRequest mainRequest, int UserID)
        {
            Wrapper wrapper = new Wrapper();
            List<ViewModel> modelObj = new List<ViewModel>();
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                modelObj = new MessageSettingDal().SelectForFilter(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }

            if (modelObj == null || modelObj.Count <= 0)
            {
                return wrapper;
            }
            wrapper.Count = modelObj.Count();

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = modelObj;

            return wrapper;
        }

        internal string IsOTPActiveDeactive(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            MessageSetting ObjModel = null;
            try
            {
                ObjModel = new MessageSettingDal().SelectByID(updateRequest.MessageSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.IsOTP = updateRequest.IsOTP;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new MessageSettingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return MessageSettingMessage.updateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

    }
}