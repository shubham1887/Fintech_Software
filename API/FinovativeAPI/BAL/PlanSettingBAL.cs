using DocumentFormat.OpenXml.Wordprocessing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.PlanSetting;
using Project.Response.PlanSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class PlanSettingBAL
    {
        private void DeactiveAll()
        {
            try
            {
                List<ViewModel> settings = new PlanSettingDAL().Select();

                foreach (ViewModel item in settings)
                {
                    PlanSetting ObjSetting = new PlanSettingDAL().SelectByID(item.PlanSettingID);
                    ObjSetting.IsActive = false;
                    new PlanSettingDAL().Update(ObjSetting);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            PlanSetting ObjModel = null;
            try
            {
                ObjModel = new PlanSettingDAL().SelectByID(updateRequest.PlanSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.UserID = updateRequest.UserID;
            ObjModel.Token = updateRequest.Token;
            ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.Circle = updateRequest.Circle;
            ObjModel.Password = updateRequest.Password;

            if (ObjModel.IsActive == true)
            {
                DeactiveAll();
            }

            int rowsAffected = -1;
            try
            {
                rowsAffected = new PlanSettingDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return PlanSettingMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new PlanSettingDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal ViewModel SelectByID(OnOffRequest mainRequest, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = DMTSettingMessages.NotFound;

            PlanSetting settings;
            try
            {
                settings = new PlanSettingDAL().SelectByID(mainRequest.PlanSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;

            try
            {
                viewModel = new ViewModel();
                viewModel.PlanSettingID = settings.PlanSettingID;
                viewModel.UserID = settings.UserID;
                viewModel.Token = settings.Token;
                viewModel.IsActive = settings.IsActive;
                viewModel.Name = settings.Name;
                viewModel.Circle = settings.Circle;

            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}