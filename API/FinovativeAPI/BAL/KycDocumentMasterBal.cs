using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.KYC;
using Project.Response.KycDocumentMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class KycDocumentMasterBal
    {
        internal List<ViewModel> Select(out string failureMessage)
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            try
            {
                viewModel = new KycDocumentMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (viewModel.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal string RequiredOnOff(IsRequriedRequest isRequriedRequest)
        {
            string failureMessage = isRequriedRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            KycDocumentMaster ObjModel = null;
            try
            {
                ObjModel = new KycDocumentMasterDal().SelectByID(isRequriedRequest.KycDocumentMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return KYCMessage.RequestNotFound; ;
            }
            ObjModel.IsRequired = isRequriedRequest.IsRequired;
            try
            {
                int rowsAffected = new KycDocumentMasterDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return KYCMessage.UploadFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal List<ViewModel> SelectRequired(out string failureMessage)
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            try
            {
                viewModel = new KycDocumentMasterDal().SelectRequired();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (viewModel.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}