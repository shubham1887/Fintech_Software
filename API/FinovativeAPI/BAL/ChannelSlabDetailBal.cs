using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Channel;
using Project.Response.Channel;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ChannelSlabDetailBal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> SlabDetaiList = null;
            try
            {
                SlabDetaiList = new ChannelSlabDetailDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return SlabDetaiList;
            }
            return SlabDetaiList;
        }

        internal string Insert(InsertRequest insertRequest, int UplineUserID)
        {
            if (UplineUserID != Company.AdminUserID)
            {
                bool IsChannel = new UserDAL().SelectIsChannel(UplineUserID);
                if (!IsChannel)
                    return SlabDetailMessage.NotChannel;
            }

            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (UplineUserID != Company.AdminUserID)
            {
                ViewModel slabComm = new ChannelSlabDetailDal().SelectByAmountOperatorAndUser(UplineUserID, insertRequest);
                if (slabComm == null)
                    return SlabDetailMessage.InvalidToAmount;
                if (slabComm.GeneralCommissionCategory != CommissionCategory.Surcharge)
                {
                    if (slabComm.Commission < insertRequest.DownlineCommission.General.Commission)
                        return $"Your Own commission is {slabComm.Commission} and you set {insertRequest.DownlineCommission.General.Commission}, please set proper";

                    if (slabComm.RealCommission < insertRequest.DownlineCommission.Real.Commission)
                        return $"Your Own real commission is {slabComm.Commission} and you set {insertRequest.DownlineCommission.Real.Commission}, please set proper";
                }
                else
                {
                    if (slabComm.Commission > insertRequest.DownlineCommission.General.Commission)
                        return $"Your Own commission is {slabComm.Commission} and you set {insertRequest.DownlineCommission.General.Commission}, please set proper";

                    if (slabComm.RealCommission > insertRequest.DownlineCommission.Real.Commission)
                        return $"Your Own real commission is {slabComm.Commission} and you set {insertRequest.DownlineCommission.Real.Commission}, please set proper";
                }
            }
            ChannelSlabDetail ObjModel = new ChannelSlabDetail
            {
                ChannelSlabMasterID = insertRequest.ChannelSlabMasterID,
                ServiceID = insertRequest.ServiceID,
                UplineUserID = UplineUserID,
                FromAmount = insertRequest.FromAmount,
                ToAmount = insertRequest.ToAmount,
                OperatorID = insertRequest.OperatorID,

                Commission = insertRequest.DownlineCommission.General.Commission,
                CommissionType = insertRequest.DownlineCommission.General.CommissionType,
                GeneralCommissionCategory = insertRequest.DownlineCommission.General.CommissionCategory ?? CommissionCategory.Commission,

                RealCommission = insertRequest.DownlineCommission.Real.Commission,
                RealCommissionType = insertRequest.DownlineCommission.Real.CommissionType,
                RealCommissionCategory = insertRequest.DownlineCommission.Real.CommissionCategory ?? CommissionCategory.Commission,
                IsDelete = false,
                IsActive = true
            };
            int rowsAffected;

            ViewModel DupliModel = new ChannelSlabDetailDal().CheckDuplicate(ObjModel);
            try
            {
                if (DupliModel != null) return SlabDetailMessage.Duplicate;
                else
                {
                    rowsAffected = new ChannelSlabDetailDal().Insert(ObjModel);
                    if (rowsAffected <= 0)
                        return SlabDetailMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal string BulkAdd(BulkAddRequest insertRequest, int UplineUserID, out string failureMessage)
        {
            failureMessage = insertRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            List<Project.Response.Operator.ViewModel> operators = new List<Response.Operator.ViewModel>();
            try
            {
                operators = new OperatorMasterDAL().SelectActiveOperatorOnly();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (insertRequest.ServiceID == (int)ServiceCode.MoneyTransfer ||
               insertRequest.ServiceID == (int)ServiceCode.AEPS ||
               insertRequest.ServiceID == (int)ServiceCode.Payout ||
               insertRequest.ServiceID == (int)ServiceCode.PaymentGateway ||
               insertRequest.ServiceID == (int)ServiceCode.Mobile ||
               insertRequest.ServiceID == (int)ServiceCode.Electricity ||
               insertRequest.ServiceID == (int)ServiceCode.UPITransfer ||
               insertRequest.ServiceID == (int)ServiceCode.AEPSOnboarding ||
               insertRequest.ServiceID == (int)ServiceCode.MATMOnboarding)
            {
                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            bool isBulkInsert = true;

            foreach (Project.Response.Operator.ViewModel Operator in operators)
            {
                if (Operator.ServiceID == (int)ServiceCode.MoneyTransfer ||
                  Operator.ServiceID == (int)ServiceCode.AEPS ||
                  Operator.ServiceID == (int)ServiceCode.Payout ||
                  Operator.ServiceID == (int)ServiceCode.PaymentGateway ||
                  Operator.ServiceID == (int)ServiceCode.Mobile ||
                  Operator.ServiceID == (int)ServiceCode.Electricity ||
                  Operator.ServiceID == (int)ServiceCode.UPITransfer ||
                  Operator.ServiceID == (int)ServiceCode.AEPSOnboarding ||
                  Operator.ServiceID == (int)ServiceCode.MATMOnboarding)
                {
                    continue;
                }

                InsertRequest objModel = new InsertRequest
                {
                    ChannelSlabMasterID = insertRequest.ChannelSlabMasterID,
                    ServiceID = Operator.ServiceID,
                    OperatorID = Operator.OperatorID,
                    FromAmount = Operator.MinimumRechargeAmount,
                    ToAmount = Operator.MaximumRechargeAmount,
                    Commission = 0,
                    CommissionType = CommissionType.Fix,
                    RealCommission = 0,
                    RealCommissionType = CommissionType.Fix,
                };

                try
                {
                    BulkInsert(objModel, UplineUserID, isBulkInsert);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }
            new ChannelSlabDetailDal().RemoveCacheForBulkInsert();
            failureMessage = StatusType.Ok;

            return failureMessage;
        }

        internal List<ViewModel> SelectBySlabMasterID(SelectByIDMasterRequest selectRequest, int UserID, out string failureMessage)
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;
            try
            {
                viewModel = new ChannelSlabDetailDal().SelectBySlabMasterID(selectRequest.ChannelSlabMasterID, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            foreach (ViewModel ObjModel in viewModel)
            {
                ObjModel.Details = $"{ObjModel.OperatorName}{Environment.NewLine}Slab :{ObjModel.SlabName}{Environment.NewLine}Amount: {Environment.NewLine}{(int)ObjModel.FromAmount} to {(int)ObjModel.ToAmount}";
                ObjModel.Commissionmodel = new CommissionModel
                {
                    ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                    Commission = ObjModel.Commission,
                    CommissionType = ObjModel.CommissionType,
                    CommissionCategory = ObjModel.GeneralCommissionCategory
                };

                ObjModel.RealCommissionmodel = new CommissionModel
                {
                    ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                    Commission = ObjModel.RealCommission,
                    CommissionType = ObjModel.RealCommissionType,
                    CommissionCategory = ObjModel.RealCommissionCategory
                };

                ObjModel.Commissionwrapper = new CommissionWrapper()
                {
                    General = (CommissionModel)ObjModel.Commissionmodel.Clone(),
                    Real = (CommissionModel)ObjModel.RealCommissionmodel.Clone()
                };

                // own comm
                ObjModel.Commissionmodel = new CommissionModel
                {
                    ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                    Commission = 0,
                    CommissionType = CommissionType.Fix,
                    CommissionCategory = CommissionCategory.Commission,
                };

                ObjModel.RealCommissionmodel = new CommissionModel
                {
                    ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                    Commission = 0,
                    CommissionType = CommissionType.Fix,
                    CommissionCategory = CommissionCategory.Commission,
                };

                InsertRequest insertRequest = new InsertRequest();
                insertRequest.OperatorID = ObjModel.OperatorID;
                insertRequest.FromAmount = ObjModel.FromAmount;
                insertRequest.ToAmount = ObjModel.ToAmount;

                if (UserID != Company.AdminUserID)
                {
                    ViewModel slabComm = new ChannelSlabDetailDal().SelectByOpertorandUser(UserID, insertRequest);
                    if (slabComm == null)
                    {
                        ObjModel.OwnCommissionwrapper = new CommissionWrapper()
                        {
                            General = ObjModel.Commissionmodel,
                            Real = ObjModel.RealCommissionmodel
                        };
                        continue;
                    }

                    ObjModel.Commissionmodel = new CommissionModel
                    {
                        ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                        Commission = slabComm.Commission,
                        CommissionType = slabComm.CommissionType,
                        CommissionCategory = slabComm.GeneralCommissionCategory,
                    };

                    ObjModel.RealCommissionmodel = new CommissionModel
                    {
                        ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                        Commission = slabComm.RealCommission,
                        CommissionType = slabComm.RealCommissionType,
                        CommissionCategory = slabComm.RealCommissionCategory,
                    };
                    ObjModel.OwnCommissionwrapper = new CommissionWrapper()
                    {
                        General = ObjModel.Commissionmodel,
                        Real = ObjModel.RealCommissionmodel
                    };
                }
            }
            return viewModel;
        }

        internal string Update(UpdateRequest updateRequest, int UplineUserID)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ChannelSlabDetail ObjModel = null;
            try
            {
                ObjModel = new ChannelSlabDetailDal().SelectByID(updateRequest.ChannelSlabDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return SlabDetailMessage.SlabDetailFound;

            InsertRequest insertRequest = new InsertRequest();
            insertRequest.OperatorID = ObjModel.OperatorID;
            insertRequest.FromAmount = ObjModel.FromAmount;
            insertRequest.ToAmount = ObjModel.ToAmount;
            insertRequest.DownlineCommission.General.CommissionType = updateRequest.CommissionType;
            insertRequest.DownlineCommission.Real.CommissionType = updateRequest.RealCommissionType;

            if (UplineUserID != Company.AdminUserID)
            {
                ViewModel slabComm = new ChannelSlabDetailDal().SelectByAmountOperatorAndUser(UplineUserID, insertRequest);
                if (slabComm == null)
                    return SlabDetailMessage.InvalidToAmount;

                if (slabComm.GeneralCommissionCategory != CommissionCategory.Surcharge)
                {
                    if (slabComm.Commission < updateRequest.Commission)
                        return $"Your Own commission is {slabComm.Commission} and you set {updateRequest.Commission}, please set proper";

                    if (slabComm.RealCommission < updateRequest.RealCommission)
                        return $"Your Own real commission is {slabComm.Commission} and you set {updateRequest.RealCommission}, please set proper";
                }
                else
                {
                    if (slabComm.Commission > updateRequest.Commission)
                        return $"Your Own commission is {slabComm.Commission} and you set {updateRequest.Commission}, please set proper";

                    if (slabComm.RealCommission > updateRequest.RealCommission)
                        return $"Your Own real commission is {slabComm.Commission} and you set {updateRequest.RealCommission}, please set proper";
                }
            }
            ObjModel.Commission = updateRequest.Commission;
            ObjModel.CommissionType = updateRequest.CommissionType;
            ObjModel.GeneralCommissionCategory = updateRequest.GeneralCommissionCategory;
            ObjModel.RealCommission = updateRequest.RealCommission;
            ObjModel.RealCommissionType = updateRequest.RealCommissionType;
            ObjModel.RealCommissionCategory = updateRequest.RealCommissionCategory;

            try
            {
                int rowsAffected = new ChannelSlabDetailDal().Update(ObjModel);

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

        internal string Delete(SelectByIDRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ChannelSlabDetail ObjModel = null;
            try
            {
                ObjModel = new ChannelSlabDetailDal().SelectByID(deleteRequest.ChannelSlabDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null) return SlabMessage.NotFound;

            ObjModel.IsDelete = true;
            int Result = 0;
            try
            {
                Result = new ChannelSlabDetailDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0) return SlabDetailMessage.DeleteFailure;
            return StatusType.Ok;
        }

        internal string BulkUpdate(BulkUpdateRequest updateRequest, int UplineUserID)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            int rowsAffected = -1;
            try
            {
                if (updateRequest.SlabType.EqualsIgnoreCase(CommissionCategory.General))
                {
                    rowsAffected = new ChannelSlabDetailDal().BulkUpdateGeneral(updateRequest, UplineUserID);
                }
                if (updateRequest.SlabType.EqualsIgnoreCase(CommissionCategory.Real))
                {
                    rowsAffected = new ChannelSlabDetailDal().BulkUpdateReal(updateRequest, UplineUserID);
                }

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

        private string BulkInsert(InsertRequest insertRequest, int UplineUserID, bool isBulkInsert)
        {
            if (UplineUserID != Company.AdminUserID)
            {
                bool IsChannel = new UserDAL().SelectIsChannel(UplineUserID);
                if (!IsChannel)
                    return SlabDetailMessage.NotChannel;
            }

            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ChannelSlabDetail ObjModel = new ChannelSlabDetail
            {
                ChannelSlabMasterID = insertRequest.ChannelSlabMasterID,
                ServiceID = insertRequest.ServiceID,
                UplineUserID = UplineUserID,
                FromAmount = insertRequest.FromAmount,
                ToAmount = insertRequest.ToAmount,
                OperatorID = insertRequest.OperatorID,
                Commission = insertRequest.Commission,
                CommissionType = insertRequest.CommissionType,
                GeneralCommissionCategory = insertRequest.GeneralCommissionCategory ?? CommissionCategory.Commission,
                RealCommission = insertRequest.RealCommission,
                RealCommissionType = insertRequest.RealCommissionType,
                RealCommissionCategory = insertRequest.RealCommissionCategory ?? CommissionCategory.Commission,
                IsDelete = false,
                IsActive = true
            };
            int rowsAffected;

            ViewModel DupliModel = new ChannelSlabDetailDal().CheckDuplicate(ObjModel);
            try
            {
                if (DupliModel != null) return SlabDetailMessage.Duplicate;
                else
                {
                    if (isBulkInsert == true)
                    {
                        rowsAffected = new ChannelSlabDetailDal().BulkInsert(ObjModel);
                    }
                    else
                    {
                        rowsAffected = new ChannelSlabDetailDal().Insert(ObjModel);
                    }

                    if (rowsAffected <= 0)
                        return SlabDetailMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal List<ViewModel> SelectByServiceID(SelectByIDMasterRequest selectRequest, int UplineUserID, out string failureMessage)
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;
            try
            {
                BulkAddRequest InsertRequest = new BulkAddRequest();
                InsertRequest.ChannelSlabMasterID = selectRequest.ChannelSlabMasterID;
                InsertRequest.ServiceID = selectRequest.ServiceID;
                new ChannelSlabDetailBal().BulkAdd(InsertRequest, UplineUserID, out failureMessage);

                viewModel = new ChannelSlabDetailDal().SelectByServiceID(selectRequest, UplineUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            foreach (ViewModel ObjModel in viewModel)
            {
                ObjModel.Details = $"{ObjModel.OperatorName}{Environment.NewLine}Slab :{ObjModel.SlabName}{Environment.NewLine}Amount: {Environment.NewLine}{(int)ObjModel.FromAmount} to {(int)ObjModel.ToAmount}";
                ObjModel.Commissionmodel = new CommissionModel
                {
                    ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                    Commission = ObjModel.Commission,
                    CommissionType = ObjModel.CommissionType,
                    CommissionCategory = ObjModel.GeneralCommissionCategory
                };

                ObjModel.RealCommissionmodel = new CommissionModel
                {
                    ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                    Commission = ObjModel.RealCommission,
                    CommissionType = ObjModel.RealCommissionType,
                    CommissionCategory = ObjModel.RealCommissionCategory
                };
                ObjModel.Commissionwrapper = new CommissionWrapper()
                {
                    General = ObjModel.Commissionmodel,
                    Real = ObjModel.RealCommissionmodel
                };
                // own comm
                ObjModel.Commissionmodel = new CommissionModel
                {
                    ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                    Commission = 0,
                    CommissionType = CommissionType.Fix,
                    CommissionCategory = CommissionCategory.Commission,
                };

                ObjModel.RealCommissionmodel = new CommissionModel
                {
                    ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                    Commission = 0,
                    CommissionType = CommissionType.Fix,
                    CommissionCategory = CommissionCategory.Commission,
                };

                InsertRequest insertRequest = new InsertRequest();
                insertRequest.OperatorID = ObjModel.OperatorID;
                insertRequest.FromAmount = ObjModel.FromAmount;
                insertRequest.ToAmount = ObjModel.ToAmount;

                if (UplineUserID != Company.AdminUserID)
                {
                    ViewModel slabComm = new ChannelSlabDetailDal().SelectByOpertorandUser(UplineUserID, insertRequest);
                    if (slabComm == null)
                    {
                        ObjModel.OwnCommissionwrapper = new CommissionWrapper()
                        {
                            General = ObjModel.Commissionmodel,
                            Real = ObjModel.RealCommissionmodel
                        };
                        continue;
                    }

                    ObjModel.Commissionmodel = new CommissionModel
                    {
                        ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                        Commission = slabComm.Commission,
                        CommissionType = slabComm.CommissionType,
                        CommissionCategory = slabComm.GeneralCommissionCategory,
                    };

                    ObjModel.RealCommissionmodel = new CommissionModel
                    {
                        ChannelSlabMasterID = ObjModel.ChannelSlabDetailID,
                        Commission = slabComm.RealCommission,
                        CommissionType = slabComm.RealCommissionType,
                        CommissionCategory = slabComm.RealCommissionCategory,
                    };
                    ObjModel.OwnCommissionwrapper = new CommissionWrapper()
                    {
                        General = ObjModel.Commissionmodel,
                        Real = ObjModel.RealCommissionmodel
                    };
                }
            }
            return viewModel;
        }

        internal string ServicewiseBulkUpdate(BulkUpdateRequest updateRequest, int UplineUserID)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            int rowsAffected = -1;
            try
            {
                if (updateRequest.SlabType.EqualsIgnoreCase(CommissionCategory.General))
                {
                    rowsAffected = new ChannelSlabDetailDal().ServicewiseBulkUpdateGeneral(updateRequest, UplineUserID);
                }
                if (updateRequest.SlabType.EqualsIgnoreCase(CommissionCategory.Real))
                {
                    rowsAffected = new ChannelSlabDetailDal().ServicewiseBulkUpdateReal(updateRequest, UplineUserID);
                }

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
    }
}