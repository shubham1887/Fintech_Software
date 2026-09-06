using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ReconciliationSettings;
using Project.Response.ReconciliationSettings;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ReconciliationSettingBal
    {
        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ViewModel obj = new ReconciliationSettingDal().SelectByProviderID(mainRequest.ProviderID);

            failureMessage = ReconciliationSettingMesages.AlreadyExists;

            if (obj != null) return failureMessage;

            failureMessage = ReconciliationSettingMesages.InsertFail;

            ReconciliationSetting ObjModel = new ReconciliationSetting
            {
                ProviderID = mainRequest.ProviderID,
                Amount = mainRequest.Amount,
                ConsumerNumber = mainRequest.ConsumerNumber,
                Status = mainRequest.Status,
                WorksheetName = mainRequest.WorksheetName,
                APIUserRequestID = mainRequest.APIUserRequestID,
                SuccessStatusAs = mainRequest.SuccessStatusAs,
                FailureStatusAs = mainRequest.FailureStatusAs
            };

            int Result;
            try
            {
                Result = new ReconciliationSettingDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
            if (Result <= 0) return failureMessage;

            failureMessage = StatusType.Ok;
            return ReconciliationSettingMesages.InsertSuccess;
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = ReconciliationSettingMesages.NotFound;
            ReconciliationSetting ObjModel = null;
            try
            {
                ObjModel = new ReconciliationSettingDal().SelectByPrimaryID(mainRequest.ReconciliationSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            failureMessage = ReconciliationSettingMesages.UpdateFail;

            ObjModel.ProviderID = mainRequest.ProviderID;
            ObjModel.Amount = mainRequest.Amount;
            ObjModel.ConsumerNumber = mainRequest.ConsumerNumber;
            ObjModel.Status = mainRequest.Status;
            ObjModel.WorksheetName = mainRequest.WorksheetName;
            ObjModel.APIUserRequestID = mainRequest.APIUserRequestID;
            ObjModel.SuccessStatusAs = mainRequest.SuccessStatusAs;
            ObjModel.FailureStatusAs = mainRequest.FailureStatusAs;
            int Result;
            try
            {
                Result = new ReconciliationSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
            if (Result <= 0) return failureMessage;

            failureMessage = StatusType.Ok;
            return ReconciliationSettingMesages.UpdateSuccess;
        }

        internal ViewModel SelectByProviderID(int providerID)
        {
            return new ReconciliationSettingDal().SelectByProviderID(providerID);
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel response = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ReconciliationSettingDal().SelectByID(mainRequest.ReconciliationSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = ReconciliationSettingMesages.NotFound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = ReconciliationSettingMesages.NotFound;

            List<ViewModel> response = new List<ViewModel>();

            try
            {
                response = new ReconciliationSettingDal().SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Delete(DeleteRequest deleteRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            ReconciliationSetting viewModel;

            try
            {
                viewModel = new ReconciliationSettingDal().SelectBySettingID(deleteRequest.ReconciliationSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (viewModel == null) return Message.ReconciliationSettingMesages.NotFound;

            failureMessage = Message.ReconciliationSettingMesages.DeleleFail;
            int Result;
            try
            {
                Result = new ReconciliationSettingDal().Delete(viewModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (Result <= 0) return response;

            response = Message.ReconciliationSettingMesages.DeleleSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}