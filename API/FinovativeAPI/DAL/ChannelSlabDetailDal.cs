using Project.Model;
using Project.Processing;
using Project.Requests.Channel;
using Project.Response.Channel;
using Project.Response.Slab;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class ChannelSlabDetailDal
    {
        public static string Key
        { get { return "_ChannelSlabDetail"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(ChannelSlabDetail objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ChannelSlabDetail.Add(objModel);
            context.SaveChanges();
            return objModel.ChannelSlabDetailID;
        }
        internal async Task<string> UpdateRange(List<ChannelSlabDetail> objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            try
            {
                context.ChannelSlabDetail.UpdateRange(objModel);
                await context.SaveChangesAsync();  
            }
            catch  
            { }
            return StatusType.Ok;  
        }

        internal int BulkInsert(ChannelSlabDetail objModel)
        {
            using var context = new DatabaseContext();
            context.ChannelSlabDetail.Add(objModel);
            context.SaveChanges();
            return objModel.ChannelSlabDetailID;
        }

        internal void RemoveCacheForBulkInsert()
        {
            RemoveCache();
        }

        internal int Delete(ChannelSlabDetail objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ChannelSlabDetail.Remove(objModel);
            return context.SaveChanges();
        }

        internal int Update(ChannelSlabDetail objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ChannelSlabDetail.Update(objModel);
            return context.SaveChanges();
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> mainList = null;
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                mainList = (from slabDetail in context.ChannelSlabDetail
                            where slabDetail.IsDelete == false
                            orderby slabDetail.ChannelSlabMasterID, slabDetail.ServiceMaster.ServiceID, slabDetail.OperatorID, slabDetail.FromAmount
                            select new ViewModel
                            {
                                ChannelSlabDetailID = slabDetail.ChannelSlabDetailID,
                                ChannelSlabMasterID = slabDetail.ChannelSlabMasterID,
                                ServiceName = slabDetail.ServiceMaster.Name,
                                ServiceID = slabDetail.ServiceID,
                                OperatorID = slabDetail.OperatorID,
                                OperatorName = slabDetail.OperatorMaster.OperatorName,
                                SlabName = slabDetail.ChannelSlabMaster.SlabName,
                                UplineUserID = slabDetail.UplineUserID,
                                FromAmount = slabDetail.FromAmount,
                                ToAmount = slabDetail.ToAmount,
                                CommissionType = slabDetail.CommissionType,
                                Commission = slabDetail.Commission,
                                RealCommission = slabDetail.RealCommission,
                                RealCommissionType = slabDetail.RealCommissionType,
                                GeneralCommissionCategory = slabDetail.GeneralCommissionCategory,
                                RealCommissionCategory = slabDetail.RealCommissionCategory,
                                IsActive = slabDetail.IsActive,
                                IsDelete = slabDetail.IsDelete,
                            }).ToList();
            }
            List<ViewModel> sortedList = (from slabDetail in mainList
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

        internal ViewModel CheckDuplicate(ChannelSlabDetail objModel)
        {
            List<ViewModel> list = Select();

            ViewModel response = null;

            response = (from slabDetail in list
                        where slabDetail.OperatorID == objModel.OperatorID &&
                              slabDetail.ChannelSlabMasterID == objModel.ChannelSlabMasterID &&
                              slabDetail.UplineUserID == objModel.UplineUserID
                             && (slabDetail.ToAmount >= objModel.FromAmount && slabDetail.ToAmount <= objModel.ToAmount
                             || (slabDetail.FromAmount >= objModel.FromAmount && slabDetail.ToAmount <= objModel.ToAmount)
                             || (slabDetail.FromAmount >= objModel.FromAmount && slabDetail.FromAmount <= objModel.ToAmount))

                        select slabDetail).FirstOrDefault();

            return response;
        }

        internal ChannelSlabDetail SelectByID(int ChannelSlabDetailID)
        {
            using var context = new DatabaseContext();
            return context.ChannelSlabDetail.Find(ChannelSlabDetailID);
        }

        internal List<ViewModel> SelectBySlabMasterID(int ChannelSlabMasterID, int UserID)
        {
            List<ViewModel> list = Select();

            return (from slabDetail in list
                    where slabDetail.ChannelSlabMasterID == ChannelSlabMasterID
                    && slabDetail.UplineUserID == UserID
                    && slabDetail.IsDelete == false
                    select slabDetail).ToList();
        }

        internal ViewModel SelectByAmountOperatorAndMasterID(int amount, int operatorID, int ChannelSlabMasterID)
        {
            List<ViewModel> list = Select();

            int serviceID = new OperatorMasterDAL().SelectServiceID(operatorID);

            list = (from slabDetail in list
                    where slabDetail.OperatorID == operatorID &&
                          slabDetail.ServiceID == serviceID &&
                          slabDetail.ChannelSlabMasterID == ChannelSlabMasterID &&
                          slabDetail.IsDelete == false &&
                          slabDetail.IsActive == true
                    select slabDetail).ToList();

            return (from slabDetail in list
                    where slabDetail.FromAmount <= amount && slabDetail.ToAmount >= amount
                    select slabDetail).FirstOrDefault();
        }

        internal ViewModel SelectByAmountOperatorAndUser(int uplineUserID, InsertRequest insertRequest)
        {
            List<ViewModel> list = Select();
            List<Response.UserChannelSlabMapping.ViewModel> MappingList = new UserChannelSlabMappingDal().Select();
            list = (from item in list
                    join mapping in MappingList on item.ChannelSlabMasterID equals mapping.ChannelSlabMasterID
                    where item.OperatorID == insertRequest.OperatorID
                          && mapping.UserID == uplineUserID
                         && item.IsDelete == false
                    select item).ToList();

            return (from item in list
                    where item.FromAmount <= insertRequest.FromAmount && item.ToAmount >= insertRequest.ToAmount
                    select item).FirstOrDefault();
        }

        internal ViewModel SelectByOpertorandUser(int uplineUserID, InsertRequest insertRequest)
        {
            List<ViewModel> list = Select();
            List<Response.UserChannelSlabMapping.ViewModel> MappingList = new UserChannelSlabMappingDal().Select();
            list = (from item in list
                    join mapping in MappingList on item.ChannelSlabMasterID equals mapping.ChannelSlabMasterID
                    where item.OperatorID == insertRequest.OperatorID
                          && mapping.UserID == uplineUserID
                         && item.FromAmount <= insertRequest.FromAmount
                         && item.ToAmount >= insertRequest.ToAmount
                         && item.IsDelete == false
                    select item).ToList();

            return (from item in list
                    select item).FirstOrDefault();
        }

        internal List<DetailViewModel> SelectByUserIDWithService(Requests.SlabDetail.SelectByUserIDWithService selectRequest)
        {
            List<DetailViewModel> report = new List<DetailViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);
            List<ViewModel> list = Select();
            List<Response.UserChannelSlabMapping.ViewModel> mainSlablist = new UserChannelSlabMappingDal().Select();

            var mainlist = (from slabDetail in list
                            join userSlabMapping in mainSlablist on
                            slabDetail.ChannelSlabMasterID equals userSlabMapping.ChannelSlabMasterID

                            where slabDetail.IsDelete == false
                             && userSlabMapping.UserID == selectRequest.UserID
                             && slabDetail.ServiceID == selectRequest.ServiceID

                            select new
                            {
                                ChannelSlabDetailID = slabDetail.ChannelSlabDetailID,
                                ChannelSlabMasterID = slabDetail.ChannelSlabMasterID,
                                SlabName = slabDetail.SlabName,
                                ServiceName = slabDetail.ServiceName,
                                OperatorID = slabDetail.OperatorID,
                                OperatorName = slabDetail.OperatorName,
                                FromAmount = slabDetail.FromAmount,
                                ToAmount = slabDetail.ToAmount,
                                Commission = slabDetail.Commission,
                                CommissionType = slabDetail.CommissionType,
                                RealCommission = slabDetail.RealCommission,
                                RealCommissionType = slabDetail.RealCommissionType,
                                GeneralCommissionCategory = slabDetail.GeneralCommissionCategory,
                                RealCommissionCategory = slabDetail.RealCommissionCategory,
                            }).ToList();

            if (UserTypeID == (int)UserType.Retailer)
            {
                report = (from item in mainlist
                          select new DetailViewModel
                          {
                              SlabDetailID = item.ChannelSlabDetailID,
                              SlabMasterID = item.ChannelSlabMasterID,
                              SlabName = item.SlabName,
                              ServiceName = item.ServiceName,
                              OperatorID = item.OperatorID,
                              OperatorName = item.OperatorName,
                              FromAmount = item.FromAmount,
                              ToAmount = item.ToAmount,

                              RetailerComm = item.Commission,
                              RetailerCommType = item.CommissionType,
                              RetailerCommissionCategory = item.GeneralCommissionCategory,
                              RealRetailerComm = item.RealCommission,
                              RealRetailerCommType = item.RealCommissionType,
                              RealRetailerCommissionCategory = item.RealCommissionCategory,

                              DistributorComm = 0,
                              DistributorCommType = "Fix",
                              DistributorCommissionCategory = CommissionCategory.Commission,
                              RealDistributorComm = 0,
                              RealDistributorCommType = "Fix",
                              RealDistributorCommissionCategory = CommissionCategory.Commission,

                              SuperDistributorComm = 0,
                              SuperDistributorCommType = "Fix",
                              SuperDistributorCommissionCategory = CommissionCategory.Commission,
                              RealSuperDistributorComm = item.RealCommission,
                              RealSuperDistributorCommType = "Fix",
                              RealSuperDistributorCommissionCategory = CommissionCategory.Commission,
                          }).ToList();
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                report = (from item in mainlist
                          select new DetailViewModel
                          {
                              SlabDetailID = item.ChannelSlabDetailID,
                              SlabMasterID = item.ChannelSlabMasterID,
                              SlabName = item.SlabName,
                              ServiceName = item.ServiceName,
                              OperatorID = item.OperatorID,
                              OperatorName = item.OperatorName,
                              FromAmount = item.FromAmount,
                              ToAmount = item.ToAmount,
                              RetailerComm = 0,
                              RetailerCommType = "Fix",
                              RetailerCommissionCategory = CommissionCategory.Commission,
                              RealRetailerComm = 0,
                              RealRetailerCommType = "Fix",
                              RealRetailerCommissionCategory = CommissionCategory.Commission,

                              DistributorComm = item.Commission,
                              DistributorCommType = item.CommissionType,
                              DistributorCommissionCategory = item.GeneralCommissionCategory,
                              RealDistributorComm = item.RealCommission,
                              RealDistributorCommType = item.RealCommissionType,
                              RealDistributorCommissionCategory = item.RealCommissionCategory,

                              SuperDistributorComm = 0,
                              SuperDistributorCommType = "Fix",
                              SuperDistributorCommissionCategory = CommissionCategory.Commission,
                              RealSuperDistributorComm = item.RealCommission,
                              RealSuperDistributorCommType = "Fix",
                              RealSuperDistributorCommissionCategory = CommissionCategory.Commission,
                          }).ToList();
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                report = (from item in mainlist
                          select new DetailViewModel
                          {
                              SlabDetailID = item.ChannelSlabDetailID,
                              SlabMasterID = item.ChannelSlabMasterID,
                              SlabName = item.SlabName,
                              ServiceName = item.ServiceName,
                              OperatorID = item.OperatorID,
                              OperatorName = item.OperatorName,
                              FromAmount = item.FromAmount,
                              ToAmount = item.ToAmount,
                              RetailerComm = 0,
                              RetailerCommType = "Fix",
                              RetailerCommissionCategory = CommissionCategory.Commission,
                              RealRetailerComm = 0,
                              RealRetailerCommType = "Fix",
                              RealRetailerCommissionCategory = CommissionCategory.Commission,

                              DistributorComm = 0,
                              DistributorCommType = "Fix",
                              DistributorCommissionCategory = CommissionCategory.Commission,
                              RealDistributorComm = 0,
                              RealDistributorCommType = "Fix",
                              RealDistributorCommissionCategory = CommissionCategory.Commission,

                              SuperDistributorComm = item.Commission,
                              SuperDistributorCommType = item.CommissionType,
                              SuperDistributorCommissionCategory = item.GeneralCommissionCategory,
                              RealSuperDistributorComm = item.RealCommission,
                              RealSuperDistributorCommType = item.RealCommissionType,
                              RealSuperDistributorCommissionCategory = item.RealCommissionCategory
                          }).ToList();
            }
            return report;
        }

        internal int BulkUpdateGeneral(BulkUpdateRequest updateRequest, int UplineUserID)
        {
            RemoveCache();
            int result = -1;
            using var context = new DatabaseContext();
            var OperatedIdUpdateList = context.ChannelSlabDetail
                .Where(q => q.OperatorID == updateRequest.OperatorID && q.UplineUserID == UplineUserID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                OperatedIdUpdateList.ForEach(c =>
                {
                    c.Commission = updateRequest.Commission;
                    c.CommissionType = updateRequest.CommissionType;
                    c.GeneralCommissionCategory = updateRequest.CommissionCategory;
                });
                context.ChannelSlabDetail.UpdateRange(OperatedIdUpdateList);
                result = context.SaveChanges();
            }
            return result;
        }

        internal int BulkUpdateReal(BulkUpdateRequest updateRequest, int UplineUserID)
        {
            RemoveCache();
            int result = -1;
            using var context = new DatabaseContext();
            var OperatedIdUpdateList = context.ChannelSlabDetail
                .Where(q => q.OperatorID == updateRequest.OperatorID && q.UplineUserID == UplineUserID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                OperatedIdUpdateList.ForEach(c =>
                {
                    c.RealCommission = updateRequest.Commission;
                    c.RealCommissionType = updateRequest.CommissionType;
                    c.RealCommissionCategory = updateRequest.CommissionCategory;
                });
                context.ChannelSlabDetail.UpdateRange(OperatedIdUpdateList);
                result = context.SaveChanges();
            }
            return result;
        }

        internal List<ViewModel> SelectByServiceID(SelectByIDMasterRequest selectRequest, int UplineUserID)
        {
            List<ViewModel> list = Select();

            List<ViewModel> response = new List<ViewModel>();
            response = (from slabDetail in list
                        where slabDetail.ChannelSlabMasterID == selectRequest.ChannelSlabMasterID
                        && slabDetail.ServiceID == selectRequest.ServiceID
                        && slabDetail.UplineUserID == UplineUserID
                        && slabDetail.IsDelete == false
                        && slabDetail.IsActive == true
                        select slabDetail).ToList();
            return response;
        }

        internal int ServicewiseBulkUpdateGeneral(BulkUpdateRequest updateRequest, int UplineUserID)
        {
            RemoveCache();
            int result = -1;
            using var context = new DatabaseContext();
            var OperatedIdUpdateList = context.ChannelSlabDetail
                .Where(q => q.ServiceID == updateRequest.ServiceID && q.ChannelSlabMasterID == updateRequest.ChannelSlabMasterID && q.UplineUserID == UplineUserID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                OperatedIdUpdateList.ForEach(c =>
                {
                    c.Commission = updateRequest.Commission;
                    c.CommissionType = updateRequest.CommissionType;
                    c.GeneralCommissionCategory = updateRequest.CommissionCategory;
                });
                context.ChannelSlabDetail.UpdateRange(OperatedIdUpdateList);
                result = context.SaveChanges();
            }
            return result;
        }

        internal int ServicewiseBulkUpdateReal(BulkUpdateRequest updateRequest, int UplineUserID)
        {
            RemoveCache();
            int result = -1;
            using var context = new DatabaseContext();
            var OperatedIdUpdateList = context.ChannelSlabDetail
                .Where(q => q.ServiceID == updateRequest.ServiceID &&
                q.ChannelSlabMasterID == updateRequest.ChannelSlabMasterID &&
                q.UplineUserID == UplineUserID).ToList();
            if (OperatedIdUpdateList.Count > 0)
            {
                OperatedIdUpdateList.ForEach(c =>
                {
                    c.RealCommission = updateRequest.Commission;
                    c.RealCommissionType = updateRequest.CommissionType;
                    c.RealCommissionCategory = updateRequest.CommissionCategory;
                });
                context.ChannelSlabDetail.UpdateRange(OperatedIdUpdateList);
                result = context.SaveChanges();
            }
            return result;
        }
    }
}