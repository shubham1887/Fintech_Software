using Project.Model;
using Project.Processing;
using Project.Requests.OfflineSlabDetail;
using Project.Response.OfflineSlabDetail;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OfflineSlabDetailDAL
    {
        public static string Key
        { get { return "_OfflineSlabDetail"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DetailViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(OfflineSlabDetail offlineSlabDetailID)
        {
            RemoveCache();

            using var contextSlab = new DatabaseContext();
            contextSlab.OfflineSlabDetail.Add(offlineSlabDetailID);
            contextSlab.SaveChanges();
            return offlineSlabDetailID.OfflineSlabDetailID;
        }

        internal void RemoveCacheForBulkInsert()
        {
            RemoveCache();
        }

        internal int BulkInsert(OfflineSlabDetail offlineSlabDetailID)
        {
            using var contextSlab = new DatabaseContext();
            contextSlab.OfflineSlabDetail.Add(offlineSlabDetailID);
            contextSlab.SaveChanges();
            return offlineSlabDetailID.OfflineSlabDetailID;
        }

        internal int Delete(OfflineSlabDetail objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.OfflineSlabDetail.Remove(objModel);
            return context.SaveChanges();
        }

        internal int Update(OfflineSlabDetail objModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.OfflineSlabDetail.Update(objModel);
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
                mainList = (from slabDetail in context.OfflineSlabDetail
                            where slabDetail.IsDelete == false
                            orderby slabDetail.SlabMasterID, slabDetail.ServiceMaster.ServiceID, slabDetail.OperatorID, slabDetail.FromAmount
                            select new DetailViewModel
                            {
                                OfflineSlabDetailID = slabDetail.OfflineSlabDetailID,
                                SlabMasterID = slabDetail.SlabMasterID,
                                SlabName = slabDetail.SlabMaster.SlabName,
                                ServiceName = slabDetail.ServiceMaster.Name,
                                PackageName = slabDetail.OfflineProductMaster.Name,
                                OfflineProductMasterID = slabDetail.OfflineProductMasterID,
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

                                IsActive = slabDetail.IsActive,
                                IsDelete = slabDetail.IsDelete,
                            }).ToList();
            }

            return mainList;
        }

        internal DetailViewModel CheckDuplicate(OfflineSlabDetail objModel)
        {
            List<DetailViewModel> list = Select();

            return (from slabDetail in list
                    where slabDetail.OperatorID == objModel.OperatorID &&
                          slabDetail.SlabMasterID == objModel.SlabMasterID
                          && slabDetail.OfflineProductMasterID == objModel.OfflineProductMasterID
                          && (slabDetail.ToAmount >= objModel.FromAmount && slabDetail.ToAmount <= objModel.ToAmount
                          || (slabDetail.FromAmount >= objModel.FromAmount && slabDetail.ToAmount <= objModel.ToAmount)
                          || (slabDetail.FromAmount >= objModel.FromAmount && slabDetail.FromAmount <= objModel.ToAmount))
                    select slabDetail).FirstOrDefault();
        }

        internal DetailViewModel SelectDetailByID(int offlineSlabDetailID)
        {
            List<DetailViewModel> list = Select();

            return (from slabDetail in list
                    where slabDetail.OfflineSlabDetailID == offlineSlabDetailID
                    && slabDetail.IsDelete == false
                    select slabDetail).FirstOrDefault();
        }

        internal OfflineSlabDetail SelectByID(int OfflineSlabDetailID)
        {
            using var context = new DatabaseContext();
            return context.OfflineSlabDetail.Find(OfflineSlabDetailID);
        }

        internal List<DetailViewModel> SelectByServiceID(SelectBySlabMasterIDRequest selectRequest, int UserID)
        {
            List<DetailViewModel> list = Select();
            List<Response.Operator.ViewModelForDropDown> ActiveOperator = new OperatorMasterDAL().SelectByServiceID(selectRequest.ServiceID, UserID);

            return (from slabDetail in list
                    join Op in ActiveOperator on slabDetail.OperatorID equals Op.OperatorID
                    where slabDetail.SlabMasterID == selectRequest.SlabMasterID
                    && slabDetail.ServiceID == selectRequest.ServiceID
                    && slabDetail.OperatorID == selectRequest.OperatorID
                   && slabDetail.IsDelete == false
                    select new DetailViewModel
                    {
                        OfflineSlabDetailID = slabDetail.OfflineSlabDetailID,
                        SlabMasterID = slabDetail.SlabMasterID,
                        SlabName = slabDetail.SlabName,
                        ServiceName = slabDetail.ServiceName,
                        OperatorID = slabDetail.OperatorID,
                        OperatorName = slabDetail.OperatorName,

                        PackageName = slabDetail.PackageName,
                        OfflineProductMasterID = slabDetail.OfflineProductMasterID,

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

                        IsActive = slabDetail.IsActive,
                        IsDelete = slabDetail.IsDelete,
                    }).ToList();
        }

        internal DetailViewModel SelectByAmountOperatorAndMasterID(int amount, int operatorID, int SlabMasterID, int offlineProductMasterID)
        {
            List<DetailViewModel> list = Select();

            int serviceID = new OperatorMasterDAL().SelectServiceID(operatorID);

            list = (from slabDetail in list
                    where slabDetail.OperatorID == operatorID &&
                          slabDetail.ServiceID == serviceID &&
                          slabDetail.SlabMasterID == SlabMasterID &&
                          slabDetail.OfflineProductMasterID == offlineProductMasterID &&
                          slabDetail.IsDelete == false &&
                          slabDetail.IsActive == true
                    select slabDetail).ToList();

            return (from slabDetail in list
                    where slabDetail.FromAmount <= amount && slabDetail.ToAmount >= amount
                    select slabDetail).FirstOrDefault();
        }
    }
}