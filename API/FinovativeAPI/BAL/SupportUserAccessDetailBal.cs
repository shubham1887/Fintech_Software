using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.SupportUserAccessDetail;
using Project.Response.SupportUserAccessDetail;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class SupportUserAccessDetailBal
    {
        internal List<ViewModel> Select(SelectRequest selectRequest)
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;
            UserMaster userMaster = new UserDAL().SelectByIDFromCache(selectRequest.UserID);
            try
            {
                List<ViewModel> Supportdtail = new SupportUserAccessDetailDal().Select();

                foreach (ViewModel item in Supportdtail)
                {
                    ViewModel Model = new ViewModel
                    {
                        SupportUserAccessMasterID = item.SupportUserAccessMasterID,
                        SupportUserAccessDetailID = item.SupportUserAccessDetailID,
                        UserID = selectRequest.UserID,
                        FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName),
                        Controller = item.Controller,
                        Action = item.Action,
                        IsActive = new SupportUserAccessDetailDal().SelectByUserIDwithSupportUserAccessMasterID(selectRequest.UserID, item.SupportUserAccessMasterID)
                    };
                    viewModel.Add(Model);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return viewModel;
        }

        internal string Insert(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            List<MenuActionMapping> listmenuMapping = new MenuActionMappingDal().SelectByMenuMasterID(updateRequest.MenuGUID);

            if (listmenuMapping == null || listmenuMapping.Count <= 0) return SupportUserMessage.ActionNot;

            foreach (MenuActionMapping item in listmenuMapping)
            {
                SupportUserAccessDetail ObjModel = new SupportUserAccessDetail
                {
                    MenuGUID = updateRequest.MenuGUID,
                    UserID = updateRequest.UserID,
                    Controller = item.Controller,
                    Action = item.Action,
                };
                SupportUserAccessDetail model = null;
                try
                {
                    model = new SupportUserAccessDetailDal().CheckDuplicate(updateRequest.MenuGUID, updateRequest.UserID, item.Controller, item.Action);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                int rowsAffected;
                if (model == null)
                {
                    try
                    {
                        rowsAffected = new SupportUserAccessDetailDal().Insert(ObjModel);
                        if (rowsAffected <= 0) return failureMessage = SupportUserMessage.AccessFail;
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                        return Messages.ExceptionOccured;
                    }
                }
            }

            return failureMessage = StatusType.Ok;
        }

        internal void Delete(UpdateRequest updateRequest)
        {
            List<MenuActionMapping> listmenuMapping = new MenuActionMappingDal().SelectByMenuMasterID(updateRequest.MenuGUID);

            if (listmenuMapping == null || listmenuMapping.Count <= 0) return;

            foreach (MenuActionMapping item in listmenuMapping)
            {
                SupportUserAccessDetail ObjModel = new SupportUserAccessDetail
                {
                    MenuGUID = updateRequest.MenuGUID,
                    UserID = updateRequest.UserID,
                    Controller = item.Controller,
                    Action = item.Action,
                    SupportUserAccessDetailID = new SupportUserAccessDetailDal().SelectID(updateRequest.MenuGUID, updateRequest.UserID, item.Controller, item.Action),
                };

                int rowsAffected;

                try
                {
                    rowsAffected = new SupportUserAccessDetailDal().Delete(ObjModel);
                    if (rowsAffected <= 0) return;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            return;
        }

        internal bool SelectForIsAuthorize(string controller, string action, int userID)
        {
            bool IsActive = true;
            try
            {
                IsActive = new SupportUserAccessDetailDal().SelectForIsAuthorize(controller, action, userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return IsActive;
        }
    }
}