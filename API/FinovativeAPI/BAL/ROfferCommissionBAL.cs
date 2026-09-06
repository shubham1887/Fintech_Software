using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ROfferCommission;
using Project.Response.ROfferCommission;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ROfferCommissionBAL
    {
        internal string Insert(InsertRequest mainRequest, bool isBulkInsert)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            ROfferCommission commission = new ROfferCommission
            {
                SlabMasterID = mainRequest.SlabID,
                ServiceID = mainRequest.ServiceID,
                OperatorID = mainRequest.OperatorID,
                Amount = mainRequest.Amount,

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

                IsActive = true
            };

            DetailViewModel ObjModel = new ROfferCommissionDAL().CheckDuplicate(commission);

            try
            {
                if (ObjModel != null) return SlabDetailMessage.Duplicate;
                else
                {
                    int Result = 0;

                    if (isBulkInsert == true)
                    {
                        Result = new ROfferCommissionDAL().BulkInsert(commission);
                    }
                    else
                    {
                        Result = new ROfferCommissionDAL().Insert(commission);
                    }

                    if (Result <= 0) return SlabDetailMessage.InsertFailure;
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

            ROfferCommission ObjModel = null;
            try
            {
                ObjModel = new ROfferCommissionDAL().SelectByID(updateRequest.ROfferCommissionID);
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

            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new ROfferCommissionDAL().Update(ObjModel);

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

            ROfferCommission ObjModel = null;
            try
            {
                ObjModel = new ROfferCommissionDAL().SelectByID(deleteRequest.ROfferCommissionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null) return SlabMessage.NotFound;

            ObjModel.IsDelete = true; int Result = 0;
            try
            {
                Result = new ROfferCommissionDAL().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0) return SlabDetailMessage.DeleteFailure;
            return StatusType.Ok;
        }

        internal List<DetailViewModel> Select(SelectRequest selectRequest)
        {
            List<DetailViewModel> SlabDetaiList = null;
            try
            {
                SlabDetaiList = new ROfferCommissionDAL().SelectBySlabIDServiceID(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return SlabDetaiList;
            }

            foreach (DetailViewModel ObjModel in SlabDetaiList)
            {
                ObjModel.Details = $"{ObjModel.OperatorName}{Environment.NewLine}Amount: {Environment.NewLine}{(int)ObjModel.Amount}";

                ObjModel.GeneralRetailer = new CommissionModel
                {
                    Commission = ObjModel.RetailerComm,
                    CommissionType = ObjModel.RetailerCommType,
                    CommissionCategory = ObjModel.RetailerCommissionCategory
                };
                ObjModel.GeneralDistributer = new CommissionModel
                {
                    Commission = ObjModel.DistributorComm,
                    CommissionType = ObjModel.DistributorCommType,
                    CommissionCategory = ObjModel.DistributorCommissionCategory
                };

                ObjModel.GeneralSuperDistributer = new CommissionModel
                {
                    Commission = ObjModel.SuperDistributorComm,
                    CommissionType = ObjModel.SuperDistributorCommType,
                    CommissionCategory = ObjModel.SuperDistributorCommissionCategory
                };

                ObjModel.GeneralClient = new CommissionModel
                {
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
            return SlabDetaiList;
        }

        internal List<SelectFRCAmountResponse> SelectFRCAmount(SelectFRCAmountRequest selectRequest, int UserID)
        {
            List<SelectFRCAmountResponse> SlabDetaiList = null;
            string failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return SlabDetaiList;
            Response.Slab.UserSlabMappingViewModel UserSlabMapping = new UserSlabMappingDal().SelectByUserID(UserID);
            if (UserSlabMapping == null)
                return SlabDetaiList;

            try
            {
                SlabDetaiList = new ROfferCommissionDAL().SelectFRCAmount(selectRequest.OperatorID, UserSlabMapping.SlabID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return SlabDetaiList;
            }
            return SlabDetaiList;
        }

        internal string Activate(SelectByIDRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ROfferCommission ObjModel = null;
            try
            {
                ObjModel = new ROfferCommissionDAL().SelectByID(updateRequest.ROfferCommissionID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return SlabDetailMessage.SlabDetailFound;

            ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new ROfferCommissionDAL().Update(ObjModel);

                if (rowsAffected <= 0)

                    return OperatorMessage.OperatorActiveDeactiveFail;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal DetailViewModel SelectByAmountOperatorAndUser(int reatilerID, int amount, int operatorID)
        {
            DetailViewModel slabDetailViewModel = null;

            slabDetailViewModel = GetROfferCommission(reatilerID, amount, operatorID);

            if (slabDetailViewModel == null)

                return slabDetailViewModel;

            if (Math.Abs(slabDetailViewModel.RetailerComm) >= 500) // if by mistake, admin set wrong commission

                return null;

            if (Math.Abs(slabDetailViewModel.DistributorComm) >= 400)

                return null;

            if (Math.Abs(slabDetailViewModel.SuperDistributorComm) >= 300)

                return null;

            return slabDetailViewModel;
        }

        private DetailViewModel GetROfferCommission(int reatilerID, int amount, int operatorID)
        {
            DetailViewModel slabDetailViewModel = null;

            try
            {
                Response.Slab.UserSlabMappingViewModel UserSlabMapping = new UserSlabMappingDal().SelectByUserID(reatilerID);

                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = new ROfferCommissionDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, UserSlabMapping.SlabID);
                    slabDetailViewModel = CalculateCommission(amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                UserMaster Um = new UserDAL().SelectByIDFromCache(reatilerID);

                if (Um == null)
                    return slabDetailViewModel;

                if (Um.ParentID == Company.AdminUserID) // if admin is parent of retiler/api user, then select default slab
                {
                    slabDetailViewModel = new ROfferCommissionDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, Company.DefaultSlabID);
                    slabDetailViewModel = CalculateCommission(amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(Um.ParentID);
                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = new ROfferCommissionDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, UserSlabMapping.SlabID);
                    slabDetailViewModel = CalculateCommission(amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                Um = new UserDAL().SelectByIDFromCache(Um.ParentID);
                if (Um == null || Um.ParentID == Company.AdminUserID)
                {
                    slabDetailViewModel = new ROfferCommissionDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, Company.DefaultSlabID);
                    slabDetailViewModel = CalculateCommission(amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(Um.ParentID);
                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = new ROfferCommissionDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, UserSlabMapping.SlabID);
                    slabDetailViewModel = CalculateCommission(amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                slabDetailViewModel = new ROfferCommissionDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, Company.DefaultSlabID);
                slabDetailViewModel = CalculateCommission(amount, slabDetailViewModel);
                return slabDetailViewModel;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return slabDetailViewModel;
        }

        private DetailViewModel CalculateCommission(int amount, DetailViewModel slabDetailViewModel)
        {
            DetailViewModel ret = new DetailViewModel();
            if (slabDetailViewModel == null)

                return slabDetailViewModel;

            ret.RetailerComm = GetLower(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, amount, slabDetailViewModel.RetailerCommissionCategory);

            ret.DistributorComm = GetLower(slabDetailViewModel.DistributorComm, slabDetailViewModel.DistributorCommType, amount, slabDetailViewModel.DistributorCommissionCategory);

            ret.SuperDistributorComm = GetLower(slabDetailViewModel.SuperDistributorComm, slabDetailViewModel.SuperDistributorCommType, amount, slabDetailViewModel.SuperDistributorCommissionCategory);

            ret.ClientComm = GetLower(slabDetailViewModel.SuperDistributorComm, slabDetailViewModel.ClientCommType, amount, slabDetailViewModel.ClientCommissionCategory);
            ret.IsActive = slabDetailViewModel.IsActive;
            return ret;
        }

        private decimal GetLower(decimal comm, string commType, int amount, string commissionCategory)
        {
            // comm always +ve
            if (comm == 0)
                return comm;

            if (comm <= 0.000001M)
                commissionCategory = CommissionCategory.Surcharge;

            if (commissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
            {
                comm = Math.Abs(comm) * -1;
            }

            if (commType.EqualsIgnoreCase(CommissionType.Fix))
                return comm;

            if (!commType.EqualsIgnoreCase(CommissionType.Flexi))
                return comm;

            decimal flexi = amount * (comm / 100);

            comm = flexi;

            return comm;
        }
    }
}