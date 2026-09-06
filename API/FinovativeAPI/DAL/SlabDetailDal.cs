using Project.Model;
using Project.Processing;
using Project.Requests.SlabDetail;
using Project.Response.Slab;
using Project.Response.User;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SlabDetailDAL
    {
        public static readonly string Key = "_SlabDetail";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DetailViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(SlabDetail slabDetail)
        {
            RemoveCache();

            using var contextSlab = new DatabaseContext();
            contextSlab.SlabDetail.Add(slabDetail);
            contextSlab.SaveChanges();
            return slabDetail.SlabDetailID;
        }

        internal void RemoveCacheForBulkInsert()
        {
            RemoveCache();
        }

        internal int BulkInsert(SlabDetail slabDetail)
        {
            using var contextSlab = new DatabaseContext();
            contextSlab.SlabDetail.Add(slabDetail);
            contextSlab.SaveChanges();
            return slabDetail.SlabDetailID;
        }

        internal int Delete(SlabDetail objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.SlabDetail.Remove(objModel);
            return context.SaveChanges();
        }

        internal int Update(SlabDetail objModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.SlabDetail.Update(objModel);
            return context.SaveChanges();
        }

        internal List<DetailViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<DetailViewModel>>();

            List<DetailViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<DetailViewModel> SelectWithoutCache()
        {
            List<DetailViewModel> mainList = null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                mainList = (from slabDetail in context.SlabDetail
                            where slabDetail.IsDelete == false
                            orderby slabDetail.SlabMasterID, slabDetail.ServiceMaster.ServiceID, slabDetail.OperatorID, slabDetail.FromAmount
                            select new DetailViewModel
                            {
                                SlabDetailID = slabDetail.SlabDetailID,
                                SlabMasterID = slabDetail.SlabMasterID,
                                SlabName = slabDetail.SlabMaster.SlabName,
                                ServiceName = slabDetail.ServiceMaster.Name,
                                ServiceID = slabDetail.ServiceID,
                                OperatorID = slabDetail.OperatorID,
                                OperatorName = slabDetail.OperatorMaster.OperatorName,
                                Logo = slabDetail.OperatorMaster.OperatorImagePath.ConcatWebAPIURL(),
                                FromAmount = slabDetail.FromAmount,
                                ToAmount = slabDetail.ToAmount,

                                RetailerComm = slabDetail.RetailerComm,
                                RetailerCommType = slabDetail.RetailerCommType,
                                RetailerCommissionCategory = slabDetail.RetailerCommissionCategory,

                                DistributorComm = slabDetail.DistributorComm,
                                DistributorCommType = slabDetail.DistributorCommType,
                                DistributorCommissionCategory = slabDetail.DistributorCommissionCategory,

                                SuperDistributorComm = slabDetail.SuperDistributorComm,
                                SuperDistributorCommType = slabDetail.SuperDistributorCommType,
                                SuperDistributorCommissionCategory = slabDetail.SuperDistributorCommissionCategory,

                                ClientComm = slabDetail.ClientComm,
                                ClientCommType = slabDetail.ClientCommType,
                                ClientCommissionCategory = slabDetail.ClientCommissionCategory,

                                RealRetailerComm = slabDetail.RealRetailerComm,
                                RealRetailerCommType = slabDetail.RealRetailerCommType,
                                RealRetailerCommissionCategory = slabDetail.RealRetailerCommissionCategory,

                                RealDistributorComm = slabDetail.RealDistributorComm,
                                RealDistributorCommType = slabDetail.RealDistributorCommType,
                                RealDistributorCommissionCategory = slabDetail.RealDistributorCommissionCategory,

                                RealSuperDistributorComm = slabDetail.RealSuperDistributorComm,
                                RealSuperDistributorCommType = slabDetail.RealSuperDistributorCommType,
                                RealSuperDistributorCommissionCategory = slabDetail.RealSuperDistributorCommissionCategory,
                                RealClientComm = slabDetail.RealClientComm,
                                RealClientCommType = slabDetail.RealClientCommType,
                                RealClientCommissionCategory = slabDetail.RealClientCommissionCategory,
                                IsActive = slabDetail.IsActive,
                                IsDelete = slabDetail.IsDelete,
                            }).ToList();
            }

            List<DetailViewModel> sortedList = (from slabDetail in mainList
                                                orderby (slabDetail.ServiceID == (int)ServiceCode.Mobile ? 1 :
                                                        slabDetail.ServiceID == (int)ServiceCode.Dth ? 2 :
                                                        slabDetail.ServiceID == (int)ServiceCode.PostPaid ? 3 :
                                                        slabDetail.ServiceID == (int)ServiceCode.Electricity ? 5 :
                                                        slabDetail.ServiceID == (int)ServiceCode.MoneyTransfer ? 6 :
                                                        slabDetail.ServiceID == (int)ServiceCode.AEPS ? 7 : 8
                                                        )
                                                select slabDetail).ToList();

            return sortedList;
        }

        internal DetailViewModel CheckDuplicate(SlabDetail objModel)
        {
            List<DetailViewModel> list = Select();

            return (from slabDetail in list
                    where slabDetail.OperatorID == objModel.OperatorID &&
                          slabDetail.SlabMasterID == objModel.SlabMasterID
                          && (slabDetail.ToAmount >= objModel.FromAmount && slabDetail.ToAmount <= objModel.ToAmount
                          || (slabDetail.FromAmount >= objModel.FromAmount && slabDetail.ToAmount <= objModel.ToAmount)
                          || (slabDetail.FromAmount >= objModel.FromAmount && slabDetail.FromAmount <= objModel.ToAmount)
                          || (slabDetail.FromAmount <= objModel.FromAmount && slabDetail.ToAmount >= objModel.ToAmount))
                    select slabDetail).FirstOrDefault();
        }

        internal DetailViewModel SelectDetailByID(int SlabDetailID)
        {
            List<DetailViewModel> list = Select();

            return (from slabDetail in list
                    where slabDetail.SlabDetailID == SlabDetailID
                    && slabDetail.IsDelete == false
                    select slabDetail).FirstOrDefault();
        }

        internal DetailViewModel SelectByAmountOperatorAndMasterID(int amount, int operatorID, int slabMasterID)
        {
            List<DetailViewModel> list = Select();

            int serviceID = new OperatorMasterDAL().SelectServiceID(operatorID);

            list = (from slabDetail in list
                    where slabDetail.OperatorID == operatorID &&
                          slabDetail.ServiceID == serviceID &&
                          slabDetail.SlabMasterID == slabMasterID &&
                          slabDetail.IsDelete == false &&
                          slabDetail.IsActive == true
                    select slabDetail).ToList();

            return (from slabDetail in list
                    where slabDetail.FromAmount <= amount && slabDetail.ToAmount >= amount
                    select slabDetail).FirstOrDefault();
        }

        internal SlabDetail SelectByID(int SlabDetailID)
        {
            using var context = new DatabaseContext();
            return context.SlabDetail.Find(SlabDetailID);
        }

        internal List<DetailViewModel> SelectBySlabMasterID(SelectBySlabMasterIDRequest selectRequest)
        {
            List<DetailViewModel> list = Select();
            List<DetailViewModel> Response = null;
            Response = (from slabDetail in list
                        where slabDetail.SlabMasterID == selectRequest.SlabMasterID
                       && slabDetail.IsDelete == false
                        select new DetailViewModel
                        {
                            SlabDetailID = slabDetail.SlabDetailID,
                            SlabMasterID = slabDetail.SlabMasterID,
                            SlabName = slabDetail.SlabName,
                            ServiceID = slabDetail.ServiceID,
                            ServiceName = slabDetail.ServiceName,
                            OperatorID = slabDetail.OperatorID,
                            OperatorName = slabDetail.OperatorName,
                            FromAmount = slabDetail.FromAmount,
                            ToAmount = slabDetail.ToAmount,

                            RetailerComm = slabDetail.RetailerComm,
                            RetailerCommType = slabDetail.RetailerCommType,
                            RetailerCommissionCategory = slabDetail.RetailerCommissionCategory,

                            DistributorComm = slabDetail.DistributorComm,
                            DistributorCommType = slabDetail.DistributorCommType,
                            DistributorCommissionCategory = slabDetail.DistributorCommissionCategory,

                            SuperDistributorComm = slabDetail.SuperDistributorComm,
                            SuperDistributorCommType = slabDetail.SuperDistributorCommType,
                            SuperDistributorCommissionCategory = slabDetail.SuperDistributorCommissionCategory,

                            ClientComm = slabDetail.ClientComm,
                            ClientCommType = slabDetail.ClientCommType,
                            ClientCommissionCategory = slabDetail.ClientCommissionCategory,

                            RealRetailerComm = slabDetail.RealRetailerComm,
                            RealRetailerCommType = slabDetail.RealRetailerCommType,
                            RealRetailerCommissionCategory = slabDetail.RealRetailerCommissionCategory,

                            RealDistributorComm = slabDetail.RealDistributorComm,
                            RealDistributorCommType = slabDetail.RealDistributorCommType,
                            RealDistributorCommissionCategory = slabDetail.RealDistributorCommissionCategory,

                            RealSuperDistributorComm = slabDetail.RealSuperDistributorComm,
                            RealSuperDistributorCommType = slabDetail.RealSuperDistributorCommType,
                            RealSuperDistributorCommissionCategory = slabDetail.RealSuperDistributorCommissionCategory,

                            RealClientComm = slabDetail.RealClientComm,
                            RealClientCommType = slabDetail.RealClientCommType,
                            RealClientCommissionCategory = slabDetail.RealClientCommissionCategory,
                            IsActive = slabDetail.IsActive,
                            IsDelete = slabDetail.IsDelete,
                        }).ToList();

            return Response;
        }

        internal List<DetailViewModel> SelectBySlabMasterIDAndServiceID(SelectBySlabMasterIDRequest selectRequest)
        {
            List<DetailViewModel> list = Select();
            List<DetailViewModel> Response = null;
            Response = (from slabDetail in list
                        where slabDetail.SlabMasterID == selectRequest.SlabMasterID
                       && slabDetail.IsDelete == false
                        select new DetailViewModel
                        {
                            SlabDetailID = slabDetail.SlabDetailID,
                            SlabMasterID = slabDetail.SlabMasterID,
                            SlabName = slabDetail.SlabName,
                            ServiceID = slabDetail.ServiceID,
                            ServiceName = slabDetail.ServiceName,
                            OperatorID = slabDetail.OperatorID,
                            OperatorName = slabDetail.OperatorName,
                            FromAmount = slabDetail.FromAmount,
                            ToAmount = slabDetail.ToAmount,

                            RetailerComm = slabDetail.RetailerComm,
                            RetailerCommType = slabDetail.RetailerCommType,
                            RetailerCommissionCategory = slabDetail.RetailerCommissionCategory,

                            DistributorComm = slabDetail.DistributorComm,
                            DistributorCommType = slabDetail.DistributorCommType,
                            DistributorCommissionCategory = slabDetail.DistributorCommissionCategory,

                            SuperDistributorComm = slabDetail.SuperDistributorComm,
                            SuperDistributorCommType = slabDetail.SuperDistributorCommType,
                            SuperDistributorCommissionCategory = slabDetail.SuperDistributorCommissionCategory,

                            ClientComm = slabDetail.ClientComm,
                            ClientCommType = slabDetail.ClientCommType,
                            ClientCommissionCategory = slabDetail.ClientCommissionCategory,

                            RealRetailerComm = slabDetail.RealRetailerComm,
                            RealRetailerCommType = slabDetail.RealRetailerCommType,
                            RealRetailerCommissionCategory = slabDetail.RealRetailerCommissionCategory,

                            RealDistributorComm = slabDetail.RealDistributorComm,
                            RealDistributorCommType = slabDetail.RealDistributorCommType,
                            RealDistributorCommissionCategory = slabDetail.RealDistributorCommissionCategory,

                            RealSuperDistributorComm = slabDetail.RealSuperDistributorComm,
                            RealSuperDistributorCommType = slabDetail.RealSuperDistributorCommType,
                            RealSuperDistributorCommissionCategory = slabDetail.RealSuperDistributorCommissionCategory,

                            RealClientComm = slabDetail.RealClientComm,
                            RealClientCommType = slabDetail.RealClientCommType,
                            RealClientCommissionCategory = slabDetail.RealClientCommissionCategory,
                            IsActive = slabDetail.IsActive,
                            IsDelete = slabDetail.IsDelete,
                        }).ToList();
            if (selectRequest.ServiceType == ServiceType.Recharge)
            {
                Response = (from item in Response
                            where item.ServiceID != (int)ServiceCode.MoneyTransfer
                            && item.ServiceID != (int)ServiceCode.AEPS
                            && item.ServiceID != (int)ServiceCode.Payout
                            && item.ServiceID != (int)ServiceCode.DTHBooking
                            && item.ServiceID != (int)ServiceCode.PhoneBooking

                            select item).ToList();
            }
            if (selectRequest.ServiceType == ServiceType.DMT)
            {
                Response = (from item in Response
                            where item.ServiceID == (int)ServiceCode.MoneyTransfer
                            || item.ServiceID == (int)ServiceCode.AEPS
                            || item.ServiceID == (int)ServiceCode.Payout
                            || item.ServiceID == (int)ServiceCode.DTHBooking
                            || item.ServiceID == (int)ServiceCode.PhoneBooking
                            select item).ToList();
            }

            if (selectRequest.ServiceID != 0)
            {
                Response = (from item in Response
                            where item.ServiceID == selectRequest.ServiceID
                            select item).ToList();
            }
            return Response;
        }

        internal List<DetailViewModel> SelectByUserIDWithService(SelectByUserIDWithService selectRequest, int LoginUserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(LoginUserID);

            if (Util.isRoleAdminOrSupport(UserTypeID) && selectRequest.UserID == Company.AdminUserID)
            {
                List<DetailViewModel> slabDetailView = SelectAllUserWithFirmName(selectRequest);
                return slabDetailView;
            }

            List<DetailViewModel> response = new List<DetailViewModel>();
            List<DetailViewModel> list = Select();
            List<UserSlabMappingViewModel> mainSlablist = new UserSlabMappingDal().Select();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();
            int userID = selectRequest.UserID;

            ParentViewModel parent = new UserDAL().SelectParent(userID);
            UserSlabMappingViewModel userMapping = mainSlablist.FirstOrDefault(X => X.UserID == userID);

            if (userMapping == null) // user in not in maping
            {
                userID = parent.ParentID;
                if (userID == Company.AdminUserID)
                {
                    List<DetailViewModel> slabDetailViewModel = SelectDefaultSlab(selectRequest);
                    return slabDetailViewModel;
                }
                UserSlabMappingViewModel parentMapping = mainSlablist.FirstOrDefault(X => X.UserID == userID);

                if (parentMapping == null) // user's parent in not in maping
                {
                    ParentViewModel SDparent = new UserDAL().SelectParent(userID);

                    userID = SDparent.ParentID;
                    if (userID == Company.AdminUserID)
                    {
                        List<DetailViewModel> slabDetailViewModel = SelectDefaultSlab(selectRequest);
                        return slabDetailViewModel;
                    }
                    UserSlabMappingViewModel SDparentMapping = mainSlablist.FirstOrDefault(X => X.UserID == userID);
                    if (SDparentMapping == null) // user's parent in not in maping
                    {
                        List<DetailViewModel> slabDetailViewModel = SelectDefaultSlab(selectRequest);
                        return slabDetailViewModel;
                    }
                }
            }

            response = (from slabDetail in list
                        join userSlabMapping in mainSlablist on
                        slabDetail.SlabMasterID equals userSlabMapping.SlabID
                        join operators in opList on
                        slabDetail.OperatorID equals operators.OperatorID
                        where slabDetail.IsDelete == false
                         && userSlabMapping.UserID == userID
                         && slabDetail.ServiceID == selectRequest.ServiceID
                        select new DetailViewModel
                        {
                            UserID = userSlabMapping.UserID,
                            SlabDetailID = slabDetail.SlabDetailID,
                            SlabMasterID = slabDetail.SlabMasterID,
                            SlabName = slabDetail.SlabName,
                            ServiceName = slabDetail.ServiceName,
                            OperatorID = slabDetail.OperatorID,
                            OperatorName = slabDetail.OperatorName,
                            Logo = slabDetail.Logo,
                            FromAmount = slabDetail.FromAmount,
                            ToAmount = slabDetail.ToAmount,

                            RetailerComm = slabDetail.RetailerComm,
                            RetailerCommType = slabDetail.RetailerCommType,
                            RetailerCommissionCategory = slabDetail.RetailerCommissionCategory,

                            DistributorComm = slabDetail.DistributorComm,
                            DistributorCommType = slabDetail.DistributorCommType,
                            DistributorCommissionCategory = slabDetail.DistributorCommissionCategory,

                            SuperDistributorComm = slabDetail.SuperDistributorComm,
                            SuperDistributorCommType = slabDetail.SuperDistributorCommType,
                            SuperDistributorCommissionCategory = slabDetail.SuperDistributorCommissionCategory,
                            ClientComm = slabDetail.ClientComm,
                            ClientCommType = slabDetail.ClientCommType,
                            ClientCommissionCategory = slabDetail.ClientCommissionCategory,

                            RealRetailerComm = slabDetail.RealRetailerComm,
                            RealRetailerCommType = slabDetail.RealRetailerCommType,
                            RealRetailerCommissionCategory = slabDetail.RealRetailerCommissionCategory,

                            RealDistributorComm = slabDetail.RealDistributorComm,
                            RealDistributorCommType = slabDetail.RealDistributorCommType,
                            RealDistributorCommissionCategory = slabDetail.RealDistributorCommissionCategory,

                            RealSuperDistributorComm = slabDetail.RealSuperDistributorComm,
                            RealSuperDistributorCommType = slabDetail.RealSuperDistributorCommType,
                            RealSuperDistributorCommissionCategory = slabDetail.RealSuperDistributorCommissionCategory,

                            RealClientComm = slabDetail.RealClientComm,
                            RealClientCommType = slabDetail.RealClientCommType,
                            RealClientCommissionCategory = slabDetail.RealClientCommissionCategory,
                            IsActive = slabDetail.IsActive,
                            IsDelete = slabDetail.IsDelete,
                        }).ToList();

            if (selectRequest.UserID != 0)
            {
                response = (from item in response
                            where item.UserID == selectRequest.UserID
                            select item).ToList();
            }

            if (selectRequest.OperatorID != 0)
            {
                response = (from item in response
                            where item.OperatorID == selectRequest.OperatorID
                            select item).ToList();
            }
            return response;
        }

        private List<DetailViewModel> SelectAllUserWithFirmName(SelectByUserIDWithService selectRequest)
        {
            List<DetailViewModel> response = new List<DetailViewModel>();
            List<DetailViewModel> list = Select();
            List<UserSlabMappingViewModel> mainSlablist = new UserSlabMappingDal().Select();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();
            List<UserMaster> Userlist = new UserDAL().Select();
            response = (from slabDetail in list
                        join userSlabMapping in mainSlablist on
                        slabDetail.SlabMasterID equals userSlabMapping.SlabID
                        join operators in opList on
                        slabDetail.OperatorID equals operators.OperatorID
                        join user in Userlist on userSlabMapping.UserID equals user.UserID
                        where slabDetail.IsDelete == false
                         && slabDetail.ServiceID == selectRequest.ServiceID

                        select new DetailViewModel
                        {
                            UserID = userSlabMapping.UserID,
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            SlabDetailID = slabDetail.SlabDetailID,
                            SlabMasterID = slabDetail.SlabMasterID,
                            SlabName = slabDetail.SlabName,
                            ServiceName = slabDetail.ServiceName,
                            OperatorID = slabDetail.OperatorID,
                            OperatorName = slabDetail.OperatorName,
                            Logo = slabDetail.Logo,
                            FromAmount = slabDetail.FromAmount,
                            ToAmount = slabDetail.ToAmount,

                            RetailerComm = slabDetail.RetailerComm,
                            RetailerCommType = slabDetail.RetailerCommType,
                            RetailerCommissionCategory = slabDetail.RetailerCommissionCategory,

                            DistributorComm = slabDetail.DistributorComm,
                            DistributorCommType = slabDetail.DistributorCommType,
                            DistributorCommissionCategory = slabDetail.DistributorCommissionCategory,

                            SuperDistributorComm = slabDetail.SuperDistributorComm,
                            SuperDistributorCommType = slabDetail.SuperDistributorCommType,
                            SuperDistributorCommissionCategory = slabDetail.SuperDistributorCommissionCategory,
                            ClientComm = slabDetail.ClientComm,
                            ClientCommType = slabDetail.ClientCommType,
                            ClientCommissionCategory = slabDetail.ClientCommissionCategory,

                            RealRetailerComm = slabDetail.RealRetailerComm,
                            RealRetailerCommType = slabDetail.RealRetailerCommType,
                            RealRetailerCommissionCategory = slabDetail.RealRetailerCommissionCategory,

                            RealDistributorComm = slabDetail.RealDistributorComm,
                            RealDistributorCommType = slabDetail.RealDistributorCommType,
                            RealDistributorCommissionCategory = slabDetail.RealDistributorCommissionCategory,

                            RealSuperDistributorComm = slabDetail.RealSuperDistributorComm,
                            RealSuperDistributorCommType = slabDetail.RealSuperDistributorCommType,
                            RealSuperDistributorCommissionCategory = slabDetail.RealSuperDistributorCommissionCategory,

                            RealClientComm = slabDetail.RealClientComm,
                            RealClientCommType = slabDetail.RealClientCommType,
                            RealClientCommissionCategory = slabDetail.RealClientCommissionCategory,
                            IsActive = slabDetail.IsActive,
                            IsDelete = slabDetail.IsDelete,
                        }).ToList();

            if (selectRequest.OperatorID != 0)
            {
                response = (from item in response
                            where item.OperatorID == selectRequest.OperatorID
                            select item).ToList();
            }
            return response;
        }

        private List<DetailViewModel> SelectDefaultSlab(SelectByUserIDWithService selectRequest)
        {
            List<DetailViewModel> list = Select();
            return (from slabDetail in list
                    where slabDetail.SlabMasterID == Company.DefaultSlabID
                     && slabDetail.IsDelete == false
                     && slabDetail.ServiceID == selectRequest.ServiceID

                    select new DetailViewModel
                    {
                        SlabDetailID = slabDetail.SlabDetailID,
                        SlabMasterID = slabDetail.SlabMasterID,
                        SlabName = slabDetail.SlabName,
                        ServiceName = slabDetail.ServiceName,
                        OperatorID = slabDetail.OperatorID,
                        OperatorName = slabDetail.OperatorName,
                        Logo = slabDetail.Logo,
                        FromAmount = slabDetail.FromAmount,
                        ToAmount = slabDetail.ToAmount,

                        RetailerComm = slabDetail.RetailerComm,
                        RetailerCommType = slabDetail.RetailerCommType,
                        RetailerCommissionCategory = slabDetail.RetailerCommissionCategory,

                        DistributorComm = slabDetail.DistributorComm,
                        DistributorCommType = slabDetail.DistributorCommType,
                        DistributorCommissionCategory = slabDetail.DistributorCommissionCategory,

                        SuperDistributorComm = slabDetail.SuperDistributorComm,
                        SuperDistributorCommType = slabDetail.SuperDistributorCommType,
                        SuperDistributorCommissionCategory = slabDetail.SuperDistributorCommissionCategory,
                        ClientComm = slabDetail.ClientComm,
                        ClientCommType = slabDetail.ClientCommType,
                        ClientCommissionCategory = slabDetail.ClientCommissionCategory,

                        RealRetailerComm = slabDetail.RealRetailerComm,
                        RealRetailerCommType = slabDetail.RealRetailerCommType,
                        RealRetailerCommissionCategory = slabDetail.RealRetailerCommissionCategory,

                        RealDistributorComm = slabDetail.RealDistributorComm,
                        RealDistributorCommType = slabDetail.RealDistributorCommType,
                        RealDistributorCommissionCategory = slabDetail.RealDistributorCommissionCategory,

                        RealSuperDistributorComm = slabDetail.RealSuperDistributorComm,
                        RealSuperDistributorCommType = slabDetail.RealSuperDistributorCommType,
                        RealSuperDistributorCommissionCategory = slabDetail.RealSuperDistributorCommissionCategory,

                        RealClientComm = slabDetail.RealClientComm,
                        RealClientCommType = slabDetail.RealClientCommType,
                        RealClientCommissionCategory = slabDetail.RealClientCommissionCategory,
                        IsActive = slabDetail.IsActive,
                        IsDelete = slabDetail.IsDelete,
                    }).ToList();
        }

        internal List<DetailViewModel> SelectByServiceID(SelectBySlabMasterIDRequest selectRequest, int UserID)
        {
            List<DetailViewModel> list = Select();
            List<Response.Operator.ViewModelForDropDown> ActiveOperator = new OperatorMasterDAL().SelectByServiceID(selectRequest.ServiceID, UserID);

            return (from slabDetail in list
                    join Op in ActiveOperator on slabDetail.OperatorID equals Op.OperatorID
                    where slabDetail.SlabMasterID == selectRequest.SlabMasterID
                    && slabDetail.ServiceID == selectRequest.ServiceID
                   && slabDetail.IsDelete == false
                    select new DetailViewModel
                    {
                        SlabDetailID = slabDetail.SlabDetailID,
                        SlabMasterID = slabDetail.SlabMasterID,
                        SlabName = slabDetail.SlabName,
                        ServiceName = slabDetail.ServiceName,
                        OperatorID = slabDetail.OperatorID,
                        OperatorName = slabDetail.OperatorName,
                        FromAmount = slabDetail.FromAmount,
                        ToAmount = slabDetail.ToAmount,

                        RetailerComm = slabDetail.RetailerComm,
                        RetailerCommType = slabDetail.RetailerCommType,
                        RetailerCommissionCategory = slabDetail.RetailerCommissionCategory,

                        DistributorComm = slabDetail.DistributorComm,
                        DistributorCommType = slabDetail.DistributorCommType,
                        DistributorCommissionCategory = slabDetail.DistributorCommissionCategory,

                        SuperDistributorComm = slabDetail.SuperDistributorComm,
                        SuperDistributorCommType = slabDetail.SuperDistributorCommType,
                        SuperDistributorCommissionCategory = slabDetail.SuperDistributorCommissionCategory,

                        ClientComm = slabDetail.ClientComm,
                        ClientCommType = slabDetail.ClientCommType,
                        ClientCommissionCategory = slabDetail.ClientCommissionCategory,

                        RealRetailerComm = slabDetail.RealRetailerComm,
                        RealRetailerCommType = slabDetail.RealRetailerCommType,
                        RealRetailerCommissionCategory = slabDetail.RealRetailerCommissionCategory,

                        RealDistributorComm = slabDetail.RealDistributorComm,
                        RealDistributorCommType = slabDetail.RealDistributorCommType,
                        RealDistributorCommissionCategory = slabDetail.RealDistributorCommissionCategory,

                        RealSuperDistributorComm = slabDetail.RealSuperDistributorComm,
                        RealSuperDistributorCommType = slabDetail.RealSuperDistributorCommType,
                        RealSuperDistributorCommissionCategory = slabDetail.RealSuperDistributorCommissionCategory,

                        RealClientComm = slabDetail.RealClientComm,
                        RealClientCommType = slabDetail.RealClientCommType,
                        RealClientCommissionCategory = slabDetail.RealClientCommissionCategory,
                        IsActive = slabDetail.IsActive,
                        IsDelete = slabDetail.IsDelete,
                    }).ToList();
        }

        internal int BulkUpdateGeneral(BulkUpdateRequest updateRequest)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            int result = -1;
            var OperatedIdUpdateList = context.SlabDetail
                .Where(q => q.OperatorID == updateRequest.OperatorID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                if (updateRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.SuperDistributorComm = updateRequest.Commission;
                        c.SuperDistributorCommType = updateRequest.CommissionType;
                        c.SuperDistributorCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Distributer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.DistributorComm = updateRequest.Commission;
                        c.DistributorCommType = updateRequest.CommissionType;
                        c.DistributorCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Retailer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.RetailerComm = updateRequest.Commission;
                        c.RetailerCommType = updateRequest.CommissionType;
                        c.RetailerCommissionCategory = updateRequest.CommissionCategory;
                    });
                }

                if (updateRequest.UserTypeID == (int)UserType.Client)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.ClientComm = updateRequest.Commission;
                        c.ClientCommType = updateRequest.CommissionType;
                        c.ClientCommissionCategory = updateRequest.CommissionCategory;
                    });
                }

                context.SlabDetail.UpdateRange(OperatedIdUpdateList);
                result = context.SaveChanges();
            }
            return result;
        }

        internal int BulkUpdateMaxMinAmount(int OperatorID, decimal MinimumRechargeAmount, decimal MaximumRechargeAmount)
        {
            RemoveCache();
            int result = -1;
            using var context = new DatabaseContext();
            var OperatedIdUpdateList = context.SlabDetail
                .Where(q => q.OperatorID == OperatorID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                OperatedIdUpdateList.ForEach(c =>
                {
                    c.FromAmount = MinimumRechargeAmount;
                    c.ToAmount = MaximumRechargeAmount;
                });
            }

            context.SlabDetail.UpdateRange(OperatedIdUpdateList);
            result = context.SaveChanges();

            return result;
        }

        internal int ServicewiseBulkUpdateGeneral(BulkUpdateRequest updateRequest)
        {
            RemoveCache();
            int result = -1;
            using var context = new DatabaseContext();
            var OperatedIdUpdateList = context.SlabDetail
                .Where(q => q.ServiceID == updateRequest.ServiceID && q.SlabMasterID == updateRequest.SlabMasterID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                if (updateRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.SuperDistributorComm = updateRequest.Commission;
                        c.SuperDistributorCommType = updateRequest.CommissionType;
                        c.SuperDistributorCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Distributer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.DistributorComm = updateRequest.Commission;
                        c.DistributorCommType = updateRequest.CommissionType;
                        c.DistributorCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Retailer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.RetailerComm = updateRequest.Commission;
                        c.RetailerCommType = updateRequest.CommissionType;
                        c.RetailerCommissionCategory = updateRequest.CommissionCategory;
                    });
                }

                if (updateRequest.UserTypeID == (int)UserType.Client)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.ClientComm = updateRequest.Commission;
                        c.ClientCommType = updateRequest.CommissionType;
                        c.ClientCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                context.SlabDetail.UpdateRange(OperatedIdUpdateList);
                result = context.SaveChanges();
            }
            return result;
        }

        internal int ServicewiseBulkUpdateReal(BulkUpdateRequest updateRequest)
        {
            RemoveCache();
            int result = -1;
            using var context = new DatabaseContext();
            var OperatedIdUpdateList = context.SlabDetail
                .Where(q => q.ServiceID == updateRequest.ServiceID && q.SlabMasterID == updateRequest.SlabMasterID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                if (updateRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.RealSuperDistributorComm = updateRequest.Commission;
                        c.RealSuperDistributorCommType = updateRequest.CommissionType;
                        c.RealSuperDistributorCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Distributer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.RealDistributorComm = updateRequest.Commission;
                        c.RealDistributorCommType = updateRequest.CommissionType;
                        c.RealDistributorCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Retailer)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.RealRetailerComm = updateRequest.Commission;
                        c.RealRetailerCommType = updateRequest.CommissionType;
                        c.RealRetailerCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                if (updateRequest.UserTypeID == (int)UserType.Client)
                {
                    OperatedIdUpdateList.ForEach(c =>
                    {
                        c.RealClientComm = updateRequest.Commission;
                        c.RealClientCommType = updateRequest.CommissionType;
                        c.RealClientCommissionCategory = updateRequest.CommissionCategory;
                    });
                }
                context.SlabDetail.UpdateRange(OperatedIdUpdateList);
                result = context.SaveChanges();
            }
            return result;
        }

        internal List<DetailViewModel> SelectForAdjustCommission(int OperatorID)
        {
            List<DetailViewModel> list = Select();
            List<UserSlabMappingViewModel> mainSlablist = new UserSlabMappingDal().Select();
            List<UserMaster> usersList = new UserDAL().Select();
            return (from slabDetail in list
                    join userSlabMapping in mainSlablist on
                     slabDetail.SlabMasterID equals userSlabMapping.SlabID
                    join user in usersList on userSlabMapping.UserID equals user.UserID
                    where slabDetail.OperatorID == OperatorID
                    && slabDetail.RetailerComm > 0
                    && slabDetail.IsDelete == false
                    select new DetailViewModel
                    {
                        UserID = userSlabMapping.UserID,
                        FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                        SlabDetailID = slabDetail.SlabDetailID,
                        SlabMasterID = slabDetail.SlabMasterID,
                        SlabName = slabDetail.SlabName,
                        ServiceName = slabDetail.ServiceName,
                        OperatorID = slabDetail.OperatorID,
                        OperatorName = slabDetail.OperatorName,
                        FromAmount = slabDetail.FromAmount,
                        ToAmount = slabDetail.ToAmount,

                        RetailerComm = slabDetail.RetailerComm,
                        RetailerCommType = slabDetail.RetailerCommType,
                        RetailerCommissionCategory = slabDetail.RetailerCommissionCategory,

                        RealRetailerComm = slabDetail.RealRetailerComm,
                        RealRetailerCommType = slabDetail.RealRetailerCommType,
                        RealRetailerCommissionCategory = slabDetail.RealRetailerCommissionCategory,
                    }).ToList();
        }
    }
}