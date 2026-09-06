using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.CircleWiseCommission;
using Project.Response.CircleWiseCommission;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CircleWiseCommissionBal
    {
        internal string Insert(InsertRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            CircleWiseCommission slabDetail = new CircleWiseCommission
            {
                SlabMasterID = mainRequest.SlabID,
                StateID = mainRequest.StateID,
                OperatorID = mainRequest.OperatorID,
                FromAmount = mainRequest.FromAmount,
                ToAmount = mainRequest.ToAmount,

                RetailerComm = mainRequest.Retailer.General.Commission,
                RetailerCommType = mainRequest.Retailer.General.CommissionType,
                RetailerCommissionCategory = mainRequest.Retailer.General.CommissionCategory,

                DistributorComm = mainRequest.Distributer.General.Commission,
                DistributorCommType = mainRequest.Distributer.General.CommissionType,
                DistributorCommissionCategory = mainRequest.Distributer.General.CommissionCategory,

                SuperDistributorComm = mainRequest.SuperDistributer.General.Commission,
                SuperDistributorCommType = mainRequest.SuperDistributer.General.CommissionType,
                SuperDistributorCommissionCategory = mainRequest.Distributer.General.CommissionCategory,

                ClientComm = mainRequest.Client.General.Commission,
                ClientCommType = mainRequest.Client.General.CommissionType,
                ClientCommissionCategory = mainRequest.Client.General.CommissionCategory,
            };

            ViewModel ObjModel = new CircleWiseCommissionDal().CheckDuplicate(slabDetail);
            try
            {
                if (ObjModel != null) return SlabDetailMessage.Duplicate;
                int Result = new CircleWiseCommissionDal().Insert(slabDetail);
                if (Result <= 0) return SlabDetailMessage.InsertFailure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal List<ViewModel> SelectByStateIDAndSlabID(SelectRequest selectRequest, int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new CircleWiseCommissionDal().SelectByStateIDAndSlabID(selectRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (ViewModel ObjModel in list)
            {
                ObjModel.Details = $"{ObjModel.OperatorName}{Environment.NewLine}Slab :{ObjModel.SlabName}{Environment.NewLine}Amount: {Environment.NewLine}{(int)ObjModel.FromAmount} to {(int)ObjModel.ToAmount}";

                ObjModel.GeneralRetailer = new CommissionModel
                {
                    CircleWiseCommissionID = ObjModel.CircleWiseCommissionID,
                    Commission = ObjModel.RetailerComm,
                    CommissionType = ObjModel.RetailerCommType,
                    CommissionCategory = ObjModel.RetailerCommissionCategory
                };
                ObjModel.GeneralDistributer = new CommissionModel
                {
                    CircleWiseCommissionID = ObjModel.CircleWiseCommissionID,
                    Commission = ObjModel.DistributorComm,
                    CommissionType = ObjModel.DistributorCommType,
                    CommissionCategory = ObjModel.DistributorCommissionCategory
                };

                ObjModel.GeneralSuperDistributer = new CommissionModel
                {
                    CircleWiseCommissionID = ObjModel.CircleWiseCommissionID,
                    Commission = ObjModel.SuperDistributorComm,
                    CommissionType = ObjModel.SuperDistributorCommType,
                    CommissionCategory = ObjModel.SuperDistributorCommissionCategory
                };

                ObjModel.GeneralClient = new CommissionModel
                {
                    CircleWiseCommissionID = ObjModel.CircleWiseCommissionID,
                    Commission = ObjModel.ClientComm,
                    CommissionType = ObjModel.ClientCommType,
                    CommissionCategory = ObjModel.ClientCommissionCategory
                };

                ObjModel.Retailer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralRetailer,
                };

                ObjModel.Distributer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralDistributer,
                };

                ObjModel.SuperDistributer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralSuperDistributer,
                };
                ObjModel.Client = new CommissionWrapper()
                {
                    General = ObjModel.GeneralClient,
                };
            }

            return list;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            CircleWiseCommission ObjModel = null;
            try
            {
                ObjModel = new CircleWiseCommissionDal().SelectByID(updateRequest.CircleWiseCommissionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return SlabDetailMessage.SlabDetailFound;

            ObjModel.RetailerComm = updateRequest.RetailerComm;
            ObjModel.RetailerCommType = updateRequest.RetailerCommType;
            ObjModel.RetailerCommissionCategory = updateRequest.RetailerIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;

            ObjModel.DistributorComm = updateRequest.DistributorComm;
            ObjModel.DistributorCommType = updateRequest.DistributorCommType;
            ObjModel.DistributorCommissionCategory = updateRequest.DistributorIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;

            ObjModel.SuperDistributorComm = updateRequest.SuperDistributorComm;
            ObjModel.SuperDistributorCommType = updateRequest.SuperDistributorCommType;
            ObjModel.SuperDistributorCommissionCategory = updateRequest.SuperDistributorIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;

            ObjModel.ClientComm = updateRequest.ClientComm;
            ObjModel.ClientCommType = updateRequest.ClientCommType;
            ObjModel.ClientCommissionCategory = updateRequest.ClientIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;
            try
            {
                int rowsAffected = new CircleWiseCommissionDal().Update(ObjModel);

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

        internal string BulkUpdate(BulkUpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new CircleWiseCommissionDal().BulkUpdate(updateRequest);

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

        internal string Delete(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            CircleWiseCommission ObjModel = null;
            try
            {
                ObjModel = new CircleWiseCommissionDal().SelectByID(deleteRequest.CircleWiseCommissionID);
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
                Result = new CircleWiseCommissionDal().Delete(ObjModel);
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