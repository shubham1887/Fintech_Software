using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.VirtualAccountMaster;
using Project.Response.VirtualAccountMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class VirtualAccountMasterBal
    {
        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            VirtualAccountMaster ObjModel = null;
            try
            {
                ObjModel = new VirtualAccountMasterDal().SelectBySettingID(updateRequest.VirtualAccountMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.BankName = updateRequest.BankName;
            ObjModel.IFSC = updateRequest.IFSC;
            ObjModel.Prefix = updateRequest.Prefix;
            ObjModel.UpdatedDOC = DateTime.Now;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new VirtualAccountMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return VirtaulAccountMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new VirtualAccountMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = VirtaulAccountMessage.DataNotFound;

            try
            {
                viewModel = new VirtualAccountMasterDal().SelectByID(mainRequest.VirtualAccountMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (viewModel == null) return viewModel;

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal Response.VirtualBalance.SelectVirtualAccountModel SelectVirtualAccount(int userID, out string failureMessage)
        {
            Response.VirtualBalance.SelectVirtualAccountModel viewModel = null;

            failureMessage = VirtaulAccountMessage.DataNotFound;

            try
            {
                viewModel = new VirtualAccountMasterDal().SelectVirtualAccount(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (viewModel == null) return viewModel;

            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}