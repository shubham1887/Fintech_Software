using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserCreationSetting;
using Project.Response.UserCreationSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserCreationSettingBal
    {
        internal string Insert(InsertRequest insertRequest)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ViewModel Model = new UserCreationSettingDal().SelectDuplicate(insertRequest.UserID);

            if (Model != null)
                return UserCreationMessage.AlreadyExist;

            UserCreationSetting ObjModel = new UserCreationSetting
            {
                UserID = insertRequest.UserID,
                MinimumCount = insertRequest.MinimumCount,
                Cost = insertRequest.Cost,
            };
            try
            {
                int rowsAfftected = new UserCreationSettingDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return UserCreationMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            new UserCreationWalletBal().Insert(insertRequest.UserID);
            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserCreationSetting ObjModel = null;
            try
            {
                ObjModel = new UserCreationSettingDal().SelectByID(updateRequest.UserCreationSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.MinimumCount = updateRequest.MinimumCount;
            ObjModel.Cost = updateRequest.Cost;
            ObjModel.UpdatedDOC = DateTime.Now;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserCreationSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return UserCreationMessage.UpdateFailure;

            return StatusType.Ok;
        }

        internal string Delete(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserCreationSetting ObjModel = null;
            try
            {
                ObjModel = new UserCreationSettingDal().SelectByID(deleteRequest.UserCreationSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserCreationSettingDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return UserCreationMessage.DeleteFail;

            return StatusType.Ok;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            try
            {
                viewModel = new UserCreationSettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return viewModel;
        }

        internal ViewModel SelectByUserID(int UserID)
        {
            ViewModel viewModel = new ViewModel();
            try
            {
                viewModel = new UserCreationSettingDal().SelectByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return viewModel;
        }
    }
}