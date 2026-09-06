using DocumentFormat.OpenXml.Spreadsheet;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.SlabDetail;
using Project.Response.Slab;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class SlabDetailBAL
    {
        internal string Insert(InsertRequest mainRequest, bool isBulkInsert)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            SlabDetail slabDetail = new SlabDetail
            {
                SlabMasterID = mainRequest.SlabID,
                ServiceID = mainRequest.ServiceID,
                OperatorID = mainRequest.OperatorID,

                FromAmount = mainRequest.FromAmount,
                ToAmount = mainRequest.ToAmount,

                RetailerComm = mainRequest.Retailer.General.Commission,
                RetailerCommType = mainRequest.Retailer.General.CommissionType,

                RetailerCommissionCategory = mainRequest.Retailer.General.CommissionCategory,

                RealRetailerComm = mainRequest.Retailer.Real.Commission,
                RealRetailerCommType = mainRequest.Retailer.Real.CommissionType,
                RealRetailerCommissionCategory = mainRequest.Retailer.Real.CommissionCategory,

                DistributorComm = mainRequest.Distributer.General.Commission,
                DistributorCommType = mainRequest.Distributer.General.CommissionType,
                DistributorCommissionCategory = mainRequest.Distributer.General.CommissionCategory,

                RealDistributorComm = mainRequest.Distributer.Real.Commission,
                RealDistributorCommType = mainRequest.Distributer.Real.CommissionType,
                RealDistributorCommissionCategory = mainRequest.Distributer.Real.CommissionCategory,

                SuperDistributorComm = mainRequest.SuperDistributer.General.Commission,
                SuperDistributorCommType = mainRequest.SuperDistributer.General.CommissionType,
                SuperDistributorCommissionCategory = mainRequest.Distributer.General.CommissionCategory,

                RealSuperDistributorComm = mainRequest.SuperDistributer.Real.Commission,
                RealSuperDistributorCommType = mainRequest.SuperDistributer.Real.CommissionType,
                RealSuperDistributorCommissionCategory = mainRequest.Distributer.Real.CommissionCategory,

                ClientComm = mainRequest.Client.General.Commission,
                ClientCommType = mainRequest.Client.General.CommissionType,
                ClientCommissionCategory = mainRequest.Client.General.CommissionCategory,

                RealClientComm = mainRequest.Client.Real.Commission,
                RealClientCommType = mainRequest.Client.Real.CommissionType,
                RealClientCommissionCategory = mainRequest.Client.Real.CommissionCategory,
                IsActive = true
            };

            DetailViewModel ObjModel = new SlabDetailDAL().CheckDuplicate(slabDetail);
            try
            {
                if (ObjModel != null) return SlabDetailMessage.Duplicate;
                else
                {
                    int Result = 0;

                    if (isBulkInsert == true)
                    {
                        Result = new SlabDetailDAL().BulkInsert(slabDetail);
                    }
                    else
                    {
                        Result = new SlabDetailDAL().Insert(slabDetail);
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

        internal string BulkAdd(BulkAddRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            List<Project.Response.Operator.ViewModel> operators = new List<Response.Operator.ViewModel>();
            try
            {
                operators = new OperatorMasterBAL().SelectOperator();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (mainRequest.ServiceID == (int)ServiceCode.MoneyTransfer ||
                mainRequest.ServiceID == (int)ServiceCode.ExpressMoneyTransfer ||
                mainRequest.ServiceID == (int)ServiceCode.AEPS ||
                mainRequest.ServiceID == (int)ServiceCode.Payout ||
                mainRequest.ServiceID == (int)ServiceCode.VendorPay ||
                mainRequest.ServiceID == (int)ServiceCode.MATM ||
                mainRequest.ServiceID == (int)ServiceCode.PaymentGateway ||
                mainRequest.ServiceID == (int)ServiceCode.CreditCardToBank ||
                mainRequest.ServiceID == (int)ServiceCode.Mobile ||
                mainRequest.ServiceID == (int)ServiceCode.Electricity ||
                mainRequest.ServiceID == (int)ServiceCode.UPITransfer ||
                mainRequest.ServiceID == (int)ServiceCode.AEPSOnboarding ||
                mainRequest.ServiceID == (int)ServiceCode.MATMOnboarding ||
                mainRequest.ServiceID == (int)ServiceCode.CreditCardPayment ||
                mainRequest.ServiceID == (int)ServiceCode.POS ||
                mainRequest.ServiceID == (int)ServiceCode.CMS||
                mainRequest.ServiceID == (int)ServiceCode.Game)

            {
                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            //operators = operators.Where(o => o.ServiceID == mainRequest.ServiceID).ToList();
            bool isBulkInsert = true;

            foreach (Project.Response.Operator.ViewModel Operator in operators)
            {
                if (Operator.ServiceID == (int)ServiceCode.MoneyTransfer ||
                    Operator.ServiceID == (int)ServiceCode.ExpressMoneyTransfer ||
                   Operator.ServiceID == (int)ServiceCode.AEPS ||
                   Operator.ServiceID == (int)ServiceCode.Payout ||
                    Operator.ServiceID == (int)ServiceCode.VendorPay ||
                   Operator.ServiceID == (int)ServiceCode.MATM ||
                   Operator.ServiceID == (int)ServiceCode.PaymentGateway ||
                   Operator.ServiceID == (int)ServiceCode.CreditCardToBank ||
                   Operator.ServiceID == (int)ServiceCode.Mobile ||
                   Operator.ServiceID == (int)ServiceCode.Electricity ||
                   Operator.ServiceID == (int)ServiceCode.UPITransfer ||
                   Operator.ServiceID == (int)ServiceCode.AEPSOnboarding ||
                   Operator.ServiceID == (int)ServiceCode.MATMOnboarding ||
                   Operator.ServiceID == (int)ServiceCode.CreditCardPayment ||
                   Operator.ServiceID == (int)ServiceCode.POS ||
                    Operator.ServiceID == (int)ServiceCode.CMS ||
                     Operator.ServiceID == (int)ServiceCode.Game
                   )
                {
                    continue;
                }

                InsertRequest objModel = new InsertRequest
                {
                    SlabID = mainRequest.SlabID,
                    ServiceID = Operator.ServiceID,
                    OperatorID = Operator.OperatorID,
                    FromAmount = Operator.MinimumRechargeAmount,
                    ToAmount = Operator.MaximumRechargeAmount,
                    Commission = 0,
                    CommissionType = CommissionType.Fix,
                    CommissionCategory = CommissionCategory.Commission,
                    UserTypeID = (int)UserType.Retailer,
                };

                try
                {
                    Insert(objModel, isBulkInsert);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            new SlabDetailDAL().RemoveCacheForBulkInsert();

            failureMessage = StatusType.Ok;

            return failureMessage;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            SlabDetail ObjModel = null;
            try
            {
                ObjModel = new SlabDetailDAL().SelectByID(updateRequest.SlabDetailID);
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

            ObjModel.RealRetailerComm = updateRequest.RealRetailerComm;
            ObjModel.RealRetailerCommType = updateRequest.RealRetailerCommType;
            ObjModel.RealRetailerCommissionCategory = updateRequest.RealRetailerIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;

            ObjModel.RealDistributorComm = updateRequest.RealDistributorComm;
            ObjModel.RealDistributorCommType = updateRequest.RealDistributorCommType;
            ObjModel.RealDistributorCommissionCategory = updateRequest.RealDistributorIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;

            ObjModel.RealSuperDistributorComm = updateRequest.RealSuperDistributorComm;
            ObjModel.RealSuperDistributorCommType = updateRequest.SuperDistributorCommType;
            ObjModel.RealSuperDistributorCommissionCategory = updateRequest.RealSuperDistributorIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;

            ObjModel.RealClientComm = updateRequest.RealClientComm;
            ObjModel.RealClientCommType = updateRequest.RealClientCommType;
            ObjModel.RealClientCommissionCategory = updateRequest.RealClientIsSurcharge ? CommissionCategory.Surcharge : CommissionCategory.Commission;

            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new SlabDetailDAL().Update(ObjModel);

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

            SlabDetail ObjModel = null;
            try
            {
                ObjModel = new SlabDetailDAL().SelectByID(deleteRequest.SlabDetailID);
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
                Result = new SlabDetailDAL().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0) return SlabDetailMessage.DeleteFailure;
            return StatusType.Ok;
        }

        internal List<DetailViewModel> Select()
        {
            List<DetailViewModel> SlabDetaiList = null;
            try
            {
                SlabDetaiList = new SlabDetailDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return SlabDetaiList;
            }
            return SlabDetaiList;
        }

        internal DetailViewModel SelectBySlabDetailID(SelectByIDRequest selectRequest, out string failureMessage)
        {
            DetailViewModel slabDetailViewModel = new DetailViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return slabDetailViewModel;
            try
            {
                slabDetailViewModel = new SlabDetailDAL().SelectDetailByID(selectRequest.SlabDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return slabDetailViewModel;
        }

        private DetailViewModel CalculateCommission(int reatilerID, int amount, DetailViewModel slabDetailViewModel)
        {
            DetailViewModel ret = new DetailViewModel();
            if (slabDetailViewModel == null)
                return slabDetailViewModel;

            ret.RetailerComm = GetLower(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, amount, slabDetailViewModel.RetailerCommissionCategory);

            ret.DistributorComm = GetLower(slabDetailViewModel.DistributorComm, slabDetailViewModel.DistributorCommType, amount, slabDetailViewModel.DistributorCommissionCategory);

            ret.SuperDistributorComm = GetLower(slabDetailViewModel.SuperDistributorComm, slabDetailViewModel.SuperDistributorCommType, amount, slabDetailViewModel.SuperDistributorCommissionCategory);

            ret.ClientComm = GetLower(slabDetailViewModel.ClientComm, slabDetailViewModel.ClientCommType, amount, slabDetailViewModel.ClientCommissionCategory);
            ret.IsActive = slabDetailViewModel.IsActive;

            int UserTypeID = new UserDAL().SelectUserTypeID(reatilerID);
            if (UserTypeID == (int)UserType.Client)
            {
                ret.RetailerComm = ret.ClientComm;
            }

            return ret;
        }

        public decimal GetLower(decimal comm, string commType, int amount, string commissionCategory)
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

        internal DetailViewModel SelectByAmountOperatorAndUser(int reatilerID, int amount, int operatorID, int CircleID, out string OutMessage)
        {
            OutMessage = StatusType.Ok;
            Response.ROfferCommission.DetailViewModel rofferComm = new ROfferCommissionBAL().SelectByAmountOperatorAndUser(reatilerID, amount, operatorID);

            int UserTypeID = new UserDAL().SelectUserTypeID(reatilerID);
            DetailViewModel slabDetailViewModel = new DetailViewModel();
            //failureMessage = StatusType.Ok;
            if (rofferComm != null)
            {
                slabDetailViewModel.RetailerComm = rofferComm.RetailerComm;
                slabDetailViewModel.SuperDistributorComm = rofferComm.SuperDistributorComm;
                slabDetailViewModel.DistributorComm = rofferComm.DistributorComm;
                slabDetailViewModel.ClientComm = rofferComm.ClientComm;
                slabDetailViewModel.IsActive = rofferComm.IsActive;
                slabDetailViewModel.IsROffer = true;
                if (UserTypeID == (int)UserType.Client)
                {
                    slabDetailViewModel.RetailerComm = slabDetailViewModel.ClientComm;
                }
                return slabDetailViewModel;
            }

            slabDetailViewModel.IsROffer = false;
            bool IsChannel = new UserDAL().SelectIsChannel(reatilerID);
            if (IsChannel)
            {
                slabDetailViewModel = GetChannelCommission(reatilerID, amount, operatorID);
                OutMessage = CheckValidChannelCommission(slabDetailViewModel, reatilerID, amount, operatorID);
            }
            else
            {
                slabDetailViewModel = GetCommission(reatilerID, amount, operatorID, CircleID);
            }

            return isCommissionValid(slabDetailViewModel, operatorID);
        }

        public UplineDetailsWithCommission GetUplineDetailsWithCommission(int UserID, DetailViewModel slabComm)
        {
            UplineDetailsWithCommission result = new UplineDetailsWithCommission();

            if (slabComm == null)
            {
                return result;
            }

            ParentViewModel parent = new UserDAL().SelectParent(UserID);

            if (parent == null)
            {
                return result;
            }

            int ParentTypeID = new UserDAL().SelectUserTypeID(parent.ParentID);

            if (ParentTypeID == (int)UserType.Distributer)
            {
                result.DistributerUserID = parent.ParentID;
                result.DistributerCommission = slabComm.DistributorComm;
            }

            if (ParentTypeID == (int)UserType.SuperDistributer)
            {
                result.SuperDistributerUserID = parent.ParentID;
                result.SuperDistributerCommission = slabComm.SuperDistributorComm;
            }

            if (parent.ParentID != Company.AdminUserID && ParentTypeID == (int)UserType.Distributer)
            {
                ParentViewModel Superparent = new UserDAL().SelectParent(parent.ParentID);

                if (Superparent.ParentID != Company.AdminUserID)
                {
                    result.SuperDistributerUserID = Superparent.ParentID;
                    result.SuperDistributerCommission = slabComm.SuperDistributorComm;
                }
            }

            return result;
        }

        private DetailViewModel isCommissionValid(DetailViewModel slabDetailViewModel, int operatorID)
        {
            if (slabDetailViewModel == null)
                return slabDetailViewModel;

            int ServiceID = new OperatorMasterBAL().SelectServiceID(operatorID);

            if (ServiceID <= 0)
            {
                return slabDetailViewModel;
            }

            if (ServiceID == (int)ServiceCode.PaymentGateway)
            {
                if (Math.Abs(slabDetailViewModel.RetailerComm) >= 10000) // if by mistake, admin set wrong commission
                    return null;

                if (Math.Abs(slabDetailViewModel.DistributorComm) >= 5000)
                    return null;

                if (Math.Abs(slabDetailViewModel.SuperDistributorComm) >= 3000)
                    return null;

                return slabDetailViewModel;
            }

            if (Math.Abs(slabDetailViewModel.RetailerComm) >= 3000) // if by mistake, admin set wrong commission
                return null;

            if (Math.Abs(slabDetailViewModel.DistributorComm) >= 1000)
                return null;

            if (Math.Abs(slabDetailViewModel.SuperDistributorComm) >= 900)
                return null;

            return slabDetailViewModel;
        }

        public DetailViewModel GetCommission(int reatilerID, int amount, int operatorID, int CircleID)
        {
            DetailViewModel slabDetailViewModel = null;
            try
            {
                UserSlabMappingViewModel UserSlabMapping = new UserSlabMappingDal().SelectByUserID(reatilerID);

                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = SelectCircleAndGeneralCommissiom(amount, operatorID, UserSlabMapping.SlabID, CircleID);
                    slabDetailViewModel = CalculateCommission(reatilerID, amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }
                UserMaster Um = new UserDAL().SelectByIDFromCache(reatilerID);

                if (Um == null)
                    return slabDetailViewModel;

                if (Um.ParentID == Company.AdminUserID) // if admin is parent of retailer/api user, then select default slab
                {
                    slabDetailViewModel = SelectCircleAndGeneralCommissiom(amount, operatorID, Company.DefaultSlabID, CircleID);
                    slabDetailViewModel = CalculateCommission(reatilerID, amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(Um.ParentID);
                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = SelectCircleAndGeneralCommissiom(amount, operatorID, UserSlabMapping.SlabID, CircleID);
                    slabDetailViewModel = CalculateCommission(reatilerID, amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                Um = new UserDAL().SelectByIDFromCache(Um.ParentID);
                if (Um == null || Um.ParentID == Company.AdminUserID)
                {
                    slabDetailViewModel = SelectCircleAndGeneralCommissiom(amount, operatorID, Company.DefaultSlabID, CircleID);
                    slabDetailViewModel = CalculateCommission(reatilerID, amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(Um.ParentID);
                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = SelectCircleAndGeneralCommissiom(amount, operatorID, UserSlabMapping.SlabID, CircleID);
                    slabDetailViewModel = CalculateCommission(reatilerID, amount, slabDetailViewModel);
                    return slabDetailViewModel;
                }

                slabDetailViewModel = SelectCircleAndGeneralCommissiom(amount, operatorID, Company.DefaultSlabID, CircleID);
                slabDetailViewModel = CalculateCommission(reatilerID, amount, slabDetailViewModel);
                return slabDetailViewModel;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return slabDetailViewModel;
        }

        private DetailViewModel SelectCircleAndGeneralCommissiom(int Amount, int OperatorID, int SlabMasterID, int CircleID)
        {
            DetailViewModel slabDetailViewModel = null;
            if (CircleID != 0)
            {
                slabDetailViewModel = new CircleWiseCommissionDal().SelectByAmountOperatorSlabIDAndStateID(Amount, OperatorID, SlabMasterID, CircleID);
                if (slabDetailViewModel != null)
                    return slabDetailViewModel;
            }

            if (slabDetailViewModel == null)
                slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(Amount, OperatorID, SlabMasterID);

            return slabDetailViewModel;
        }

        private DetailViewModel GetChannelCommission(int reatilerID, int amount, int operatorID)
        {
            DetailViewModel slabDetailViewModel = new DetailViewModel();
            Response.Channel.ViewModel retailerCommi = null;
            Response.Channel.ViewModel DistCommi = null;
            Response.Channel.ViewModel SuperCommi = null;
            try
            {
                UserMaster RetailerUser = new UserDAL().SelectByIDFromCache(reatilerID);
                if (RetailerUser == null)
                    return slabDetailViewModel;

                UserMaster DistUser = new UserDAL().SelectByIDFromCache(RetailerUser.ParentID);
                UserMaster SuperUser = new UserDAL().SelectByIDFromCache(DistUser.ParentID);

                int UserTypeID = new UserDAL().SelectUserTypeID(SuperUser.UserID);

                Project.Response.UserChannelSlabMapping.ViewModel retailerSlab = new UserChannelSlabMappingDal().SelectByUserID(reatilerID);
                Project.Response.UserChannelSlabMapping.ViewModel DistSlab = new UserChannelSlabMappingDal().SelectByUserID(RetailerUser.ParentID);
                Project.Response.UserChannelSlabMapping.ViewModel SuperSlab = new UserChannelSlabMappingDal().SelectByUserID(DistUser.ParentID);

                decimal rComm = 0.0M;
                decimal DComm = 0.0M;
                decimal SDcomm = 0.0M;

                if (retailerSlab != null)
                {
                    retailerCommi = new ChannelSlabDetailDal().SelectByAmountOperatorAndMasterID(amount, operatorID, retailerSlab.ChannelSlabMasterID);
                    if (retailerCommi != null)
                    {
                        rComm = GetLower(retailerCommi.Commission, retailerCommi.CommissionType, amount, retailerCommi.GeneralCommissionCategory);
                    }
                }
                if (DistSlab != null)
                {
                    DistCommi = new ChannelSlabDetailDal().SelectByAmountOperatorAndMasterID(amount, operatorID, DistSlab.ChannelSlabMasterID);
                    if (DistCommi != null)
                    {
                        DComm = GetLower(DistCommi.Commission, DistCommi.CommissionType, amount, DistCommi.GeneralCommissionCategory);
                    }
                }
                if (SuperSlab != null)
                {
                    SuperCommi = new ChannelSlabDetailDal().SelectByAmountOperatorAndMasterID(amount, operatorID, SuperSlab.ChannelSlabMasterID);
                    if (SuperCommi != null)
                    {
                        SDcomm = GetLower(SuperCommi.Commission, SuperCommi.CommissionType, amount, SuperCommi.GeneralCommissionCategory);
                    }
                }

                slabDetailViewModel.RetailerComm = rComm;
                slabDetailViewModel.DistributorComm = DComm == 0 ? 0 : (DComm + (rComm * -1));

                if (DComm == 0)
                {
                    DComm = rComm;
                }

                slabDetailViewModel.SuperDistributorComm = SDcomm == 0 ? 0 : (SDcomm + (DComm * -1));

                if (DistUser.UserTypeID == (int)UserType.SuperDistributer)
                {
                    slabDetailViewModel.SuperDistributorComm = slabDetailViewModel.DistributorComm;
                    slabDetailViewModel.DistributorComm = 0;
                }

                slabDetailViewModel.ClientComm = 0.0M;
                slabDetailViewModel.IsActive = true;

                return slabDetailViewModel;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return slabDetailViewModel;
        }

        internal SlabReportWrapper SelectByUserIDWithService(SelectByUserIDWithService selectRequest, int LoginUserID, out string failureMessage)
        {
            SlabReportWrapper wrapper = new SlabReportWrapper();
            List<DetailViewModel> slabDetailViewModel = new List<DetailViewModel>();
            failureMessage = selectRequest.IsValidModalState();
            isExcelOrPDF(selectRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            if (selectRequest.UserID == 0)
                selectRequest.UserID = LoginUserID;

            int UserTypeID = -1;
            try
            {
                UserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = SlabDetailMessage.SlabNotSet;
            bool IsChannel = new UserDAL().SelectIsChannel(selectRequest.UserID);
            try
            {
                if (IsChannel)
                {
                    slabDetailViewModel = new ChannelSlabDetailDal().SelectByUserIDWithService(selectRequest);

                    if (slabDetailViewModel == null || slabDetailViewModel.Count == 0)
                        return wrapper;
                }
                else
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByUserIDWithService(selectRequest, LoginUserID);

                    if (slabDetailViewModel.Count == 0)
                    {
                        int ParentID = new UserDAL().SelectParentID(selectRequest.UserID);
                        selectRequest.UserID = ParentID;
                        slabDetailViewModel = new SlabDetailDAL().SelectByUserIDWithService(selectRequest, LoginUserID);
                    }
                    if (slabDetailViewModel.Count == 0)
                    {
                        int SuprParentID = new UserDAL().SelectParentID(selectRequest.UserID);
                        selectRequest.UserID = SuprParentID;
                        slabDetailViewModel = new SlabDetailDAL().SelectByUserIDWithService(selectRequest, LoginUserID);
                    }

                    if (slabDetailViewModel == null || slabDetailViewModel.Count == 0)
                        return wrapper;
                }
                wrapper.Count = slabDetailViewModel.Count();
                if (!Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
                {
                    slabDetailViewModel = slabDetailViewModel.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (DetailViewModel ObjModel in slabDetailViewModel)
            {
                ObjModel.Details = $"{ObjModel.OperatorName}{Environment.NewLine}Amount: {Environment.NewLine}{(int)ObjModel.FromAmount} to {(int)ObjModel.ToAmount}";

                if (ObjModel.RetailerCommissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
                    ObjModel.RetailerComm = Decimal.Negate(ObjModel.RetailerComm);

                if (ObjModel.RetailerCommType.EqualsIgnoreCase(CommissionType.Flexi))
                    ObjModel.StrRetailerComm = ObjModel.RetailerComm.TwoDigitDecimal() + " " + "%";
                if (ObjModel.RetailerCommType.EqualsIgnoreCase(CommissionType.Fix))
                    ObjModel.StrRetailerComm = ObjModel.RetailerComm.TwoDigitDecimal() + " " + "Rs";

                if (ObjModel.DistributorCommissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
                    ObjModel.DistributorComm = Decimal.Negate(ObjModel.DistributorComm);

                if (ObjModel.DistributorCommType.EqualsIgnoreCase(CommissionType.Flexi))
                    ObjModel.StrDistributorComm = ObjModel.DistributorComm.TwoDigitDecimal() + " " + "%";
                if (ObjModel.DistributorCommType.EqualsIgnoreCase(CommissionType.Fix))
                    ObjModel.StrDistributorComm = ObjModel.DistributorComm.TwoDigitDecimal() + " " + "Rs";

                if (ObjModel.SuperDistributorCommissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
                    ObjModel.SuperDistributorComm = Decimal.Negate(ObjModel.SuperDistributorComm);

                if (ObjModel.SuperDistributorCommType.EqualsIgnoreCase(CommissionType.Flexi))
                    ObjModel.StrSuperDistributorComm = ObjModel.SuperDistributorComm.TwoDigitDecimal() + " " + "%";
                if (ObjModel.SuperDistributorCommType.EqualsIgnoreCase(CommissionType.Fix))
                    ObjModel.StrSuperDistributorComm = ObjModel.SuperDistributorComm.TwoDigitDecimal() + " " + "Rs";

                if (ObjModel.ClientCommissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
                    ObjModel.ClientComm = Decimal.Negate(ObjModel.ClientComm);

                if (ObjModel.ClientCommType.EqualsIgnoreCase(CommissionType.Flexi))
                    ObjModel.StrClientComm = ObjModel.ClientComm.TwoDigitDecimal() + " " + "%";
                if (ObjModel.ClientCommType.EqualsIgnoreCase(CommissionType.Fix))
                    ObjModel.StrClientComm = ObjModel.ClientComm.TwoDigitDecimal() + " " + "Rs";

                if (UserTypeID == (int)UserType.Client)
                {
                    ObjModel.RetailerComm = ObjModel.ClientComm;
                }
            }

            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            wrapper.list = slabDetailViewModel;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.SlabDetail.SelectByUserIDWithService request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }

        internal List<DetailViewModel> SelectBySlabMasterID(SelectBySlabMasterIDRequest selectRequest)
        {
            List<DetailViewModel> list = new List<DetailViewModel>();

            try
            {
                BulkAddRequest InsertRequest = new BulkAddRequest();
                InsertRequest.SlabID = selectRequest.SlabMasterID;
                InsertRequest.ServiceID = selectRequest.ServiceID;

                BulkAdd(InsertRequest);

                list = new SlabDetailDAL().SelectBySlabMasterIDAndServiceID(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (DetailViewModel ObjModel in list)
            {
                ObjModel.Details = $"{ObjModel.OperatorName}{Environment.NewLine}Slab :{ObjModel.SlabName}{Environment.NewLine}Amount: {Environment.NewLine}{(int)ObjModel.FromAmount} to {(int)ObjModel.ToAmount}";
                ObjModel.GeneralRetailer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RetailerComm,
                    CommissionType = ObjModel.RetailerCommType,
                    CommissionCategory = ObjModel.RetailerCommissionCategory
                };
                ObjModel.GeneralDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.DistributorComm,
                    CommissionType = ObjModel.DistributorCommType,
                    CommissionCategory = ObjModel.DistributorCommissionCategory
                };

                ObjModel.GeneralSuperDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.SuperDistributorComm,
                    CommissionType = ObjModel.SuperDistributorCommType,
                    CommissionCategory = ObjModel.SuperDistributorCommissionCategory
                };

                ObjModel.GeneralClient = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.ClientComm,
                    CommissionType = ObjModel.ClientCommType,
                    CommissionCategory = ObjModel.ClientCommissionCategory
                };
                ObjModel.RealRetailer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealRetailerComm,
                    CommissionType = ObjModel.RealRetailerCommType,
                    CommissionCategory = ObjModel.RealRetailerCommissionCategory
                };
                ObjModel.RealDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealDistributorComm,
                    CommissionType = ObjModel.RealDistributorCommType,
                    CommissionCategory = ObjModel.RealDistributorCommissionCategory
                };
                ObjModel.RealSuperDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealSuperDistributorComm,
                    CommissionType = ObjModel.RealSuperDistributorCommType,
                    CommissionCategory = ObjModel.RealSuperDistributorCommissionCategory
                };
                ObjModel.RealClient = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealClientComm,
                    CommissionType = ObjModel.RealClientCommType,
                    CommissionCategory = ObjModel.RealClientCommissionCategory
                };
                ObjModel.Retailer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralRetailer,
                    Real = ObjModel.RealRetailer
                };

                ObjModel.Distributer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralDistributer,
                    Real = ObjModel.RealDistributer
                };

                ObjModel.SuperDistributer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralSuperDistributer,
                    Real = ObjModel.RealSuperDistributer
                };
                ObjModel.Client = new CommissionWrapper()
                {
                    General = ObjModel.GeneralClient,
                    Real = ObjModel.RealClient
                };
            }

            return list;
        }

        internal string BulkUpdate(BulkUpdateRequest updateRequest, int userID)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            int rowsAffected = -1;
            try
            {
                if (updateRequest.SlabType.EqualsIgnoreCase(CommissionCategory.General))
                {
                    rowsAffected = new SlabDetailDAL().BulkUpdateGeneral(updateRequest);
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

        internal List<DetailViewModel> SelectByServiceID(SelectBySlabMasterIDRequest selectRequest, int UserID)
        {
            List<DetailViewModel> list = new List<DetailViewModel>();

            try
            {
                BulkAddRequest InsertRequest = new BulkAddRequest();
                InsertRequest.SlabID = selectRequest.SlabMasterID;
                InsertRequest.ServiceID = (int)selectRequest.ServiceID;

                BulkAdd(InsertRequest);

                list = new SlabDetailDAL().SelectByServiceID(selectRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (DetailViewModel ObjModel in list)
            {
                ObjModel.Details = $"{ObjModel.OperatorName}{Environment.NewLine}Slab :{ObjModel.SlabName}{Environment.NewLine}Amount: {Environment.NewLine}{(int)ObjModel.FromAmount} to {(int)ObjModel.ToAmount}";

                ObjModel.GeneralRetailer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RetailerComm,
                    CommissionType = ObjModel.RetailerCommType,
                    CommissionCategory = ObjModel.RetailerCommissionCategory
                };
                ObjModel.GeneralDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.DistributorComm,
                    CommissionType = ObjModel.DistributorCommType,
                    CommissionCategory = ObjModel.DistributorCommissionCategory
                };

                ObjModel.GeneralSuperDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.SuperDistributorComm,
                    CommissionType = ObjModel.SuperDistributorCommType,
                    CommissionCategory = ObjModel.SuperDistributorCommissionCategory
                };

                ObjModel.GeneralClient = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.ClientComm,
                    CommissionType = ObjModel.ClientCommType,
                    CommissionCategory = ObjModel.ClientCommissionCategory
                };
                ObjModel.RealRetailer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealRetailerComm,
                    CommissionType = ObjModel.RealRetailerCommType,
                    CommissionCategory = ObjModel.RealRetailerCommissionCategory
                };
                ObjModel.RealDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealDistributorComm,
                    CommissionType = ObjModel.RealDistributorCommType,
                    CommissionCategory = ObjModel.RealDistributorCommissionCategory
                };
                ObjModel.RealSuperDistributer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealSuperDistributorComm,
                    CommissionType = ObjModel.RealSuperDistributorCommType,
                    CommissionCategory = ObjModel.RealSuperDistributorCommissionCategory
                };
                ObjModel.RealClient = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealClientComm,
                    CommissionType = ObjModel.RealClientCommType,
                    CommissionCategory = ObjModel.RealClientCommissionCategory
                };
                ObjModel.Retailer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralRetailer,
                    Real = ObjModel.RealRetailer
                };

                ObjModel.Distributer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralDistributer,
                    Real = ObjModel.RealDistributer
                };

                ObjModel.SuperDistributer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralSuperDistributer,
                    Real = ObjModel.RealSuperDistributer
                };
                ObjModel.Client = new CommissionWrapper()
                {
                    General = ObjModel.GeneralClient,
                    Real = ObjModel.RealClient
                };
            }

            return list;
        }

        internal string Activate(SelectByIDRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            SlabDetail ObjModel = null;
            try
            {
                ObjModel = new SlabDetailDAL().SelectByID(updateRequest.SlabDetailID);
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
                int rowsAffected = new SlabDetailDAL().Update(ObjModel);

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

        internal string ServicewiseBulkUpdate(BulkUpdateRequest updateRequest, int UserID)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            int rowsAffected = -1;
            try
            {
                if (updateRequest.SlabType.EqualsIgnoreCase(CommissionCategory.General))
                {
                    rowsAffected = new SlabDetailDAL().ServicewiseBulkUpdateGeneral(updateRequest);
                }
                if (updateRequest.SlabType.EqualsIgnoreCase(CommissionCategory.Real))
                {
                    rowsAffected = new SlabDetailDAL().ServicewiseBulkUpdateReal(updateRequest);
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

        internal List<DetailViewModel> SelectForAdjustCommission(SelectAdjustCommissionRequest selectRequest)
        {
            List<DetailViewModel> list = new List<DetailViewModel>();
            try
            {
                list = new SlabDetailDAL().SelectForAdjustCommission(selectRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (DetailViewModel ObjModel in list)
            {
                ObjModel.GeneralRetailer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RetailerComm,
                    CommissionType = ObjModel.RetailerCommType,
                    CommissionCategory = ObjModel.RetailerCommissionCategory
                };
                ObjModel.RealRetailer = new CommissionModel
                {
                    SlabDetailID = ObjModel.SlabDetailID,
                    Commission = ObjModel.RealRetailerComm,
                    CommissionType = ObjModel.RealRetailerCommType,
                    CommissionCategory = ObjModel.RealRetailerCommissionCategory
                };

                ObjModel.Retailer = new CommissionWrapper()
                {
                    General = ObjModel.GeneralRetailer,
                    Real = ObjModel.RealRetailer
                };
            }
            return list;
        }

        internal string IncreaseDecreaseCommission(CommissionUpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            try
            {
                List<DetailViewModel> list = new SlabDetailDAL().SelectForAdjustCommission(updateRequest.OperatorID);

                List<DetailViewModel> uniqSlabList = list.GroupBy(x => x.SlabDetailID).Select(y => y.FirstOrDefault()).Distinct().ToList();

                foreach (DetailViewModel Operator in uniqSlabList)
                {
                    SlabDetail ObjModel = new SlabDetailDAL().SelectByID(Operator.SlabDetailID);
                    if (ObjModel == null)
                    {
                        continue;
                    }
                    if (updateRequest.Adjustment.EqualsIgnoreCase(CommissionType.Increase))
                    {
                        ObjModel.RetailerComm += updateRequest.Commission;
                        ObjModel.SlabDetailID = ObjModel.SlabDetailID;
                    }
                    if (updateRequest.Adjustment.EqualsIgnoreCase(CommissionType.Decrease))
                    {
                        ObjModel.RetailerComm = (ObjModel.RetailerComm - updateRequest.Commission);
                    }
                    new SlabDetailDAL().Update(ObjModel);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal string UpdateAmountByID(UpdateAmountRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            SlabDetail ObjModel = null;
            try
            {
                ObjModel = new SlabDetailDAL().SelectByID(updateRequest.SlabDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return SlabDetailMessage.SlabDetailFound;

            ObjModel.FromAmount = updateRequest.FromAmount;
            ObjModel.ToAmount = updateRequest.ToAmount;

            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new SlabDetailDAL().Update(ObjModel);

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

        internal string BulkCopy(BulkCopyRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            SelectBySlabMasterIDRequest selectRequest = new SelectBySlabMasterIDRequest
            {
                SlabMasterID = mainRequest.CopySlabID
            };
            List<DetailViewModel> list = new List<DetailViewModel>();

            try
            {
                list = new SlabDetailDAL().SelectBySlabMasterID(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list.Count <= 0)
                return failureMessage;

            foreach (DetailViewModel obj in list)
            {
                SlabDetail objModel = new SlabDetail
                {
                    SlabMasterID = mainRequest.SlabID,
                    ServiceID = obj.ServiceID,
                    OperatorID = obj.OperatorID,
                    FromAmount = obj.FromAmount,
                    ToAmount = obj.ToAmount,
                    IsActive = obj.IsActive,

                    RetailerComm = obj.RetailerComm,
                    RetailerCommType = obj.RetailerCommType,
                    RetailerCommissionCategory = obj.RetailerCommissionCategory,

                    RealRetailerComm = obj.RealRetailerComm,
                    RealRetailerCommType = obj.RealRetailerCommType,
                    RealRetailerCommissionCategory = obj.RealRetailerCommissionCategory,

                    DistributorComm = obj.DistributorComm,
                    DistributorCommType = obj.DistributorCommType,
                    DistributorCommissionCategory = obj.DistributorCommissionCategory,

                    RealDistributorComm = obj.RealDistributorComm,
                    RealDistributorCommType = obj.RealDistributorCommType,
                    RealDistributorCommissionCategory = obj.RealDistributorCommissionCategory,

                    SuperDistributorComm = obj.SuperDistributorComm,
                    SuperDistributorCommType = obj.SuperDistributorCommType,
                    SuperDistributorCommissionCategory = obj.SuperDistributorCommissionCategory,

                    RealSuperDistributorComm = obj.RealSuperDistributorComm,
                    RealSuperDistributorCommType = obj.RealSuperDistributorCommType,
                    RealSuperDistributorCommissionCategory = obj.RealSuperDistributorCommissionCategory,

                    ClientComm = obj.ClientComm,
                    ClientCommType = obj.ClientCommType,
                    ClientCommissionCategory = obj.ClientCommissionCategory,

                    RealClientComm = obj.RealClientComm,
                    RealClientCommType = obj.RealClientCommType,
                    RealClientCommissionCategory = obj.RealClientCommissionCategory,
                };
                int Result = -1;
                try
                {
                    Result = new SlabDetailDAL().BulkInsert(objModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            new SlabDetailDAL().RemoveCacheForBulkInsert();

            failureMessage = StatusType.Ok;

            return failureMessage;
        }

        internal string CheckValidChannelCommission(DetailViewModel reatilslabDetailViewModel, int reatilerID, int amount, int operatorID)
        {
            ParentViewModel parent = new UserDAL().SelectParent(reatilerID);
            int UplineID = parent.ParentID;
            int Parent_UserTypeID = new UserDAL().SelectUserTypeID(UplineID);

            if (Parent_UserTypeID == (int)UserType.Distributer)
            {
                if (reatilslabDetailViewModel.DistributorComm < 0)
                    return Messages.CommssionDecrease;
            }
            if (Parent_UserTypeID == (int)UserType.SuperDistributer)
            {
                if (reatilslabDetailViewModel.SuperDistributorComm < 0)
                    return Messages.CommssionDecrease;
            }

            return StatusType.Ok;
        }

        internal UserwiseSlabReportWrapper SelectSlabByUserIDWithService(SelectByUserIDWithService selectRequest, int LoginUserID, out string failureMessage)
        {
            UserwiseSlabReportWrapper wrapper = new UserwiseSlabReportWrapper();
            List<DetailViewModel> slabDetailViewModel = new List<DetailViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            if (selectRequest.UserID == 0)
                selectRequest.UserID = LoginUserID;

            int UserTypeID = -1;
            try
            {
                UserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = SlabDetailMessage.SlabNotSet;
            bool IsChannel = new UserDAL().SelectIsChannel(selectRequest.UserID);
            try
            {
                if (IsChannel)
                {
                    slabDetailViewModel = new ChannelSlabDetailDal().SelectByUserIDWithService(selectRequest);
                    if (slabDetailViewModel == null || slabDetailViewModel.Count == 0)
                        return wrapper;
                }
                else
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByUserIDWithService(selectRequest, LoginUserID);

                    if (slabDetailViewModel.Count == 0 && (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser))
                    {
                        int ParentID = new UserDAL().SelectParentID(selectRequest.UserID);
                        selectRequest.UserID = ParentID;
                        slabDetailViewModel = new SlabDetailDAL().SelectByUserIDWithService(selectRequest, LoginUserID);
                    }
                    if (slabDetailViewModel == null || slabDetailViewModel.Count == 0)
                        return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            List<UserwiseSlabViewModel> slabDetail = new List<UserwiseSlabViewModel>();

            foreach (DetailViewModel ObjModel in slabDetailViewModel)
            {
                UserwiseSlabViewModel slab = new UserwiseSlabViewModel();

                slab.Type = $"{ObjModel.OperatorName}{Environment.NewLine}";
                slab.AmountString = $"Amount : {(int)ObjModel.FromAmount} to {(int)ObjModel.ToAmount}";

                if (ObjModel.RetailerCommissionCategory.EqualsIgnoreCase(CommissionCategory.Surcharge))
                    ObjModel.RetailerComm = Decimal.Negate(ObjModel.RetailerComm);

                if (ObjModel.RetailerCommType.EqualsIgnoreCase(CommissionType.Fix))
                    ObjModel.RetailerCommType = "Rs. " + ObjModel.RetailerComm.TwoDigitDecimal();

                if (ObjModel.RetailerCommType.EqualsIgnoreCase(CommissionType.Flexi))
                    ObjModel.RetailerCommType = ObjModel.RetailerComm.TwoDigitDecimal() + " %";

                slab.RetailerComm = ObjModel.RetailerComm;
                slab.RetailerCommType = ObjModel.RetailerCommType;

                slabDetail.Add(slab);
            }
            wrapper.Count = slabDetail.Count;
            wrapper.list = slabDetail;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal PGCommissionViewModel SelectUPICommissionByUserID(int reatilerID, int Amount)

        {
            PGCommissionViewModel response = new PGCommissionViewModel();

            DetailViewModel slabDetailViewModel = null;
            try
            {
                UserSlabMappingViewModel UserSlabMapping = new UserSlabMappingDal().SelectByUserID(reatilerID);

                if (UserSlabMapping != null)
                { ///
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(Amount, (int)OperatorCode.Upi, UserSlabMapping.SlabID);

                    response = new PGCommissionViewModel();
                    response.Commission = GetLower(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, Amount, slabDetailViewModel.RetailerCommissionCategory);
                    response.CommissionCategory = slabDetailViewModel.RetailerCommissionCategory;
                    return response;
                }
                UserMaster Um = new UserDAL().SelectByIDFromCache(reatilerID);

                if (Um == null)
                    return response;

                if (Um.ParentID == Company.AdminUserID) // if admin is parent of retailer/api user, then select default slab
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(Amount, (int)OperatorCode.Upi, Company.DefaultSlabID);
                    response = new PGCommissionViewModel();
                    response.Commission = GetLower(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, Amount, slabDetailViewModel.RetailerCommissionCategory);
                    response.CommissionCategory = slabDetailViewModel.RetailerCommissionCategory;
                    return response;
                }

                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(Um.ParentID);
                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(Amount, (int)OperatorCode.Upi, UserSlabMapping.SlabID);
                    response = new PGCommissionViewModel();
                    response.Commission = GetLower(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, Amount, slabDetailViewModel.RetailerCommissionCategory);
                    response.CommissionCategory = slabDetailViewModel.RetailerCommissionCategory;
                    return response;
                }

                Um = new UserDAL().SelectByIDFromCache(Um.ParentID);
                if (Um == null || Um.ParentID == Company.AdminUserID)
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(Amount, (int)OperatorCode.Upi, Company.DefaultSlabID);
                    response = new PGCommissionViewModel();
                    response.Commission = GetLower(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, Amount, slabDetailViewModel.RetailerCommissionCategory);
                    response.CommissionCategory = slabDetailViewModel.RetailerCommissionCategory;
                    return response;
                }

                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(Um.ParentID);
                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(Amount, (int)OperatorCode.Upi, UserSlabMapping.SlabID);
                    response = new PGCommissionViewModel();
                    response.Commission = GetLower(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, Amount, slabDetailViewModel.RetailerCommissionCategory);
                    response.CommissionCategory = slabDetailViewModel.RetailerCommissionCategory;
                    return response;
                }

                slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(Amount, (int)OperatorCode.Upi, Company.DefaultSlabID);
                response = new PGCommissionViewModel();
                response.Commission = GetLower(slabDetailViewModel.RetailerComm, slabDetailViewModel.RetailerCommType, Amount, slabDetailViewModel.RetailerCommissionCategory);
                response.CommissionCategory = slabDetailViewModel.RetailerCommissionCategory;
                return response;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return response;
        }

        internal ChargeAndCommissionViewModel SelectDMTChargeAndCommission(int UserID, DMTChargeAndCommissionRequest mainRequest)
        {
            string OutMessage = "";
            ChargeAndCommissionViewModel response = new ChargeAndCommissionViewModel();

            string ShowDMTChargeOnConfirmation = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowDMTChargeOnConfirmation), "Yes");
            if (!ShowDMTChargeOnConfirmation.EqualsIgnoreCase("Yes"))
            {
                response.Commission = 0;
                response.Charge = 0;
                response.Total = 0;
                return response;
            }

            DetailViewModel slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(mainRequest.Amount), (int)OperatorCode.Transfer_DMT, 0, out OutMessage);
            if (slabComm == null)
                return response;

            int ProviderID = GetProviderID(mainRequest.ActiveDMTName);
            DMTSetting settings = new DMTSettingsBal().GetActiveDMTSetting(ProviderID);

            if (settings == null || settings.IsCCF == false)
            {
                response.Commission = 0;
                response.Charge = slabComm.RetailerComm;
                response.Total = 0;
                return response;
            }
            if (settings.IsCCF == true)
            {
                decimal ccfCharge = new TaxProcessing().getCCF(mainRequest.Amount);

                response.Commission = slabComm.RetailerComm;
                response.Charge = ccfCharge;
                response.Total = ccfCharge - slabComm.RetailerComm;
                return response;
            }
            return response;
        }

        private int GetProviderID(string ActiveDMT)
        {
            return DMTCode.getIDByName(ActiveDMT);
        }

        internal ChargeAndCommissionViewModel SelectSlabCommissionByOperatorID(int UserID, int OperatorID)
        {
            string OutMessage = "";
            ChargeAndCommissionViewModel response = new ChargeAndCommissionViewModel();
          
            DetailViewModel slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, 10, OperatorID, 0, out OutMessage);
            if (slabComm == null)
                return response;
          
                response.Commission = slabComm.RetailerComm;
                response.Charge = 0;
                response.Total = 0;
                return response;
          
        }
        public DetailViewModel GetSetCommission(int reatilerID, int amount, int operatorID)
        {
            DetailViewModel slabDetailViewModel = null;
            try
            {
                UserSlabMappingViewModel UserSlabMapping = new UserSlabMappingDal().SelectByUserID(reatilerID);

                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, UserSlabMapping.SlabID);

                    return slabDetailViewModel;
                }
                UserMaster Um = new UserDAL().SelectByIDFromCache(reatilerID);

                if (Um == null)
                    return slabDetailViewModel;

                if (Um.ParentID == Company.AdminUserID) // if admin is parent of retailer/api user, then select default slab
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, Company.DefaultSlabID);
                    return slabDetailViewModel;
                }

                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(Um.ParentID);
                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, UserSlabMapping.SlabID);
                    return slabDetailViewModel;
                }

                Um = new UserDAL().SelectByIDFromCache(Um.ParentID);
                if (Um == null || Um.ParentID == Company.AdminUserID)
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, Company.DefaultSlabID);
                    return slabDetailViewModel;
                }

                UserSlabMapping = new UserSlabMappingDal().SelectByUserID(Um.ParentID);
                if (UserSlabMapping != null)
                {
                    slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, UserSlabMapping.SlabID);

                    return slabDetailViewModel;
                }
                slabDetailViewModel = new SlabDetailDAL().SelectByAmountOperatorAndMasterID(amount, operatorID, Company.DefaultSlabID);
                return slabDetailViewModel;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return slabDetailViewModel;
        }

    }
}