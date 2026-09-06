using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.WebHookSetting;
using Project.Response.WebHookSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class WebHookSettingBal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            try
            {
                viewModel = new WebHookSettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return viewModel;
        }

        internal string IsActiveDeactive(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            WebHookSetting ObjModel = null;
            try
            {
                ObjModel = new WebHookSettingDal().SelectByID(updateRequest.WebHookSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new WebHookSettingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return WebHookSettingMessage.updateFail;
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

            WebHookSetting ObjModel = null;
            try
            {
                ObjModel = new WebHookSettingDal().SelectByID(updateRequest.WebHookSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            //   ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.URL = updateRequest.URL;
            ObjModel.MethodType = updateRequest.MethodType;
            ObjModel.HeaderJson = updateRequest.HeaderJson;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new WebHookSettingDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return WebHookSettingMessage.updateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal ViewModel SelectByType(string type)
        {
            ViewModel ObjModel = null;
            try
            {
                ObjModel = new WebHookSettingDal().SelectByType(type);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return ObjModel;
            }
            return ObjModel;
        }
    }
}