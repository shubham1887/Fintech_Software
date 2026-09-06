using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderConfigurationAdminCommission;
using Project.Response.ProviderConfigurationAdminCommission;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ProviderConfigurationAdminCommissionBal
    {
        internal string Insert(InsertRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (mainRequest.OperatorID == 0)
            {
                BulkUpdateByServiceID(mainRequest, UserID);
            }
            else
            {
                ProviderConfigurationAdminCommission objModel = new ProviderConfigurationAdminCommission
                {
                    ProviderID = mainRequest.ProviderID,
                    ServiceID = mainRequest.ServiceID,
                    OperatorID = mainRequest.OperatorID,

                    FromAmount = mainRequest.FromAmount,
                    ToAmount = mainRequest.ToAmount,

                    AdminComm = mainRequest?.Admin.General.Commission ?? 0,
                    AdminCommType = mainRequest?.Admin.General.CommissionType ?? CommissionType.Flexi,
                    AdminCommissionCategory = mainRequest?.Admin.General.CommissionCategory ?? CommissionCategory.Commission,
                };

                DetailViewModel ObjModel = new ProviderConfigurationAdminCommissionDal().CheckDuplicate(objModel);

                try
                {
                    if (ObjModel != null) return SlabDetailMessage.Duplicate;

                    int Result = 0;
                    Result = new ProviderConfigurationAdminCommissionDal().Insert(objModel);
                    if (Result <= 0) return SlabDetailMessage.InsertFailure;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
            }

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderConfigurationAdminCommission ObjModel = null;
            try
            {
                ObjModel = new ProviderConfigurationAdminCommissionDal().SelectByID(updateRequest.ProviderConfigurationAdminCommissionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return SlabDetailMessage.SlabDetailFound;

            ObjModel.AdminComm = updateRequest.AdminComm;
            ObjModel.AdminCommType = updateRequest.AdminCommType;
            ObjModel.AdminCommissionCategory = updateRequest.AdminIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new ProviderConfigurationAdminCommissionDal().Update(ObjModel);

                if (rowsAffected <= 0)

                    return SlabDetailMessage.UpdateFail;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal List<DetailViewModel> SelectByServiceID(SelecRequest selectRequest, int UserID)
        {
            List<DetailViewModel> list = new List<DetailViewModel>();
            try
            {
                //BulkAdd(selectRequest, UserID);
                list = new ProviderConfigurationAdminCommissionDal().SelectByServiceID(selectRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (DetailViewModel ObjModel in list)
            {
                ObjModel.GeneralAdmin = new CommissionModel
                {
                    ProviderConfigurationAdminCommissionID = ObjModel.ProviderConfigurationAdminCommissionID,
                    Commission = ObjModel.AdminComm,
                    CommissionType = ObjModel.AdminCommType,
                    CommissionCategory = ObjModel.AdminCommissionCategory
                };

                ObjModel.Admin = new CommissionWrapper()
                {
                    General = ObjModel.GeneralAdmin,
                };
            }

            return list;
        }

        internal string BulkUpdateByServiceID(InsertRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            List<Project.Response.Operator.ViewModelForDropDown> operators = new List<Response.Operator.ViewModelForDropDown>();
            try
            {
                operators = new OperatorMasterDAL().SelectByServiceID(mainRequest.ServiceID, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            foreach (Response.Operator.ViewModelForDropDown Operator in operators)
            {
                List<DetailViewModel> list = new ProviderConfigurationAdminCommissionDal().SelectByProviderIDServiceIDAndOperatorID(mainRequest.ProviderID, mainRequest.ServiceID, Operator.OperatorID);
                if (list.Count > 1)
                    continue;

                ProviderConfigurationAdminCommission ObjModel = null;
                try
                {
                    ObjModel = new ProviderConfigurationAdminCommissionDal().SelectByID(list[0].ProviderConfigurationAdminCommissionID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (ObjModel == null) return SlabDetailMessage.SlabDetailFound;

                ObjModel.FromAmount = mainRequest.FromAmount;
                ObjModel.ToAmount = mainRequest.ToAmount;
                ObjModel.AdminComm = mainRequest?.Admin.General.Commission ?? 0;
                ObjModel.AdminCommType = mainRequest?.Admin.General.CommissionType ?? CommissionType.Flexi;
                ObjModel.AdminCommissionCategory = mainRequest?.Admin.General.CommissionCategory ?? CommissionCategory.Commission;
                ObjModel.UpdatedDOC = DateTime.Now;
                try
                {
                    int rowsAffected = new ProviderConfigurationAdminCommissionDal().Update(ObjModel);

                    if (rowsAffected <= 0)

                        return SlabDetailMessage.UpdateFail;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
            }

            new ProviderConfigurationAdminCommissionDal().RemoveCacheForBulkInsert();
            return StatusType.Ok;
        }

        internal string BulkAdd(SelecRequest selectRequest, int UserID)
        {
            string failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            List<Project.Response.Operator.ViewModelForDropDown> operators = new List<Response.Operator.ViewModelForDropDown>();
            try
            {
                operators = new OperatorMasterDAL().SelectByServiceID(selectRequest.ServiceID, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (selectRequest.ServiceID == (int)ServiceCode.MoneyTransfer ||
                selectRequest.ServiceID == (int)ServiceCode.AEPS ||
                selectRequest.ServiceID == (int)ServiceCode.Payout ||
                 selectRequest.ServiceID == (int)ServiceCode.MATM ||
                selectRequest.ServiceID == (int)ServiceCode.PaymentGateway)
            {
                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            foreach (Response.Operator.ViewModelForDropDown Operator in operators)
            {
                InsertRequest objModel = new InsertRequest
                {
                    ProviderID = selectRequest.ProviderID,
                    ServiceID = selectRequest.ServiceID,
                    OperatorID = Operator.OperatorID,
                    FromAmount = Operator.MinAmount,
                    ToAmount = Operator.MaxAmount,
                };

                try
                {
                    Insert(objModel, UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            new ProviderConfigurationAdminCommissionDal().RemoveCacheForBulkInsert();
            return StatusType.Ok;
        }

        internal string Delete(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderConfigurationAdminCommission ObjModel = null;
            try
            {
                ObjModel = new ProviderConfigurationAdminCommissionDal().SelectByID(deleteRequest.ProviderConfigurationAdminCommissionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null) return SlabMessage.NotFound;
            int Result = 0;
            try
            {
                Result = new ProviderConfigurationAdminCommissionDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0) return SlabDetailMessage.DeleteFailure;
            return StatusType.Ok;
        }
    }
}