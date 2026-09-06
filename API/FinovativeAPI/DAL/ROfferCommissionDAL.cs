using Project.Model;
using Project.Processing;
using Project.Requests.ROfferCommission;
using Project.Response.ROfferCommission;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ROfferCommissionDAL
    {
        private readonly string Key = "_ROfferCommission";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DetailViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(ROfferCommission commission)
        {
            RemoveCache();

            using var contextSlab = new DatabaseContext();
            contextSlab.ROfferCommission.Add(commission);
            contextSlab.SaveChanges();
            return commission.ROfferCommissionID;
        }

        internal int BulkInsert(ROfferCommission commission)
        {
            using var contextSlab = new DatabaseContext();
            contextSlab.ROfferCommission.Add(commission);
            contextSlab.SaveChanges();
            return commission.ROfferCommissionID;
        }

        internal DetailViewModel CheckDuplicate(ROfferCommission objModel)
        {
            List<DetailViewModel> list = Select();

            return (from coomission in list
                    where coomission.OperatorID == objModel.OperatorID &&
                          coomission.ServiceID == objModel.ServiceID
                          && coomission.SlabMasterID == objModel.SlabMasterID
                          && coomission.Amount == objModel.Amount
                    select coomission).FirstOrDefault();
        }

        internal List<DetailViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<DetailViewModel>>();

            List<DetailViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        internal List<DetailViewModel> SelectBySlabIDServiceID(SelectRequest selectRequest)
        {
            List<DetailViewModel> list = Select();

            List<DetailViewModel> response = new List<DetailViewModel>();
            response = (from item in list
                        where item.SlabMasterID == selectRequest.SlabMasterID
                        && item.ServiceID == selectRequest.ServiceID
                        select item).ToList();

            return response;
        }

        private List<DetailViewModel> SelectWithoutCache()
        {
            List<DetailViewModel> mainList = null;
            using (var context = new DatabaseContext())
            {
                mainList = (from commission in context.ROfferCommission
                            where commission.IsDelete == false
                            orderby commission.SlabMasterID, commission.ServiceMaster.ServiceID, commission.OperatorID, commission.Amount
                            select new DetailViewModel
                            {
                                ROfferCommissionID = commission.ROfferCommissionID,
                                SlabMasterID = commission.SlabMasterID,
                                SlabName = commission.SlabMaster.SlabName,
                                ServiceName = commission.ServiceMaster.Name,
                                ServiceID = commission.ServiceID,
                                OperatorID = commission.OperatorID,
                                OperatorName = commission.OperatorMaster.OperatorName,
                                Amount = commission.Amount,

                                RetailerComm = commission.RetailerComm,
                                RetailerCommType = commission.RetailerCommType,
                                RetailerCommissionCategory = commission.RetailerCommissionCategory,

                                DistributorComm = commission.DistributorComm,
                                DistributorCommType = commission.DistributorCommType,
                                DistributorCommissionCategory = commission.DistributorCommissionCategory,

                                SuperDistributorComm = commission.SuperDistributorComm,
                                SuperDistributorCommType = commission.SuperDistributorCommType,
                                SuperDistributorCommissionCategory = commission.SuperDistributorCommissionCategory,

                                ClientComm = commission.ClientComm,
                                ClientCommType = commission.ClientCommType,
                                ClientCommissionCategory = commission.ClientCommissionCategory,

                                IsActive = commission.IsActive,
                                IsDelete = commission.IsDelete,
                            }).ToList();
            }

            List<DetailViewModel> sortedList = (from slabDetail in mainList
                                                orderby (slabDetail.ServiceID == (int)ServiceCode.Mobile ? 1 :
                                                        slabDetail.ServiceID == (int)ServiceCode.Dth ? 2 :
                                                        slabDetail.ServiceID == (int)ServiceCode.PostPaid ? 3 :

                                                        slabDetail.ServiceID == (int)ServiceCode.Electricity ? 5 :
                                                        slabDetail.ServiceID == (int)ServiceCode.MoneyTransfer ? 6 :
                                                        slabDetail.ServiceID == (int)ServiceCode.AEPS ? 7 :
                                                            slabDetail.ServiceID == (int)ServiceCode.OTT ? 8 :
                                                   slabDetail.ServiceID == (int)ServiceCode.FRC ? 20 : 9
                                                        )
                                                select slabDetail).ToList();

            return sortedList;
        }

        internal ROfferCommission SelectByID(int ROfferCommissionID)
        {
            using var context = new DatabaseContext();
            return context.ROfferCommission.Find(ROfferCommissionID);
        }

        internal int Delete(ROfferCommission objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ROfferCommission.Remove(objModel);
            return context.SaveChanges();
        }

        internal int Update(ROfferCommission objModel)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            context.ROfferCommission.Update(objModel);
            return context.SaveChanges();
        }

        internal DetailViewModel SelectByAmountOperatorAndMasterID(int amount, int operatorID, int slabMasterID)
        {
            List<DetailViewModel> list = Select();

            list = (from slabDetail in list
                    where slabDetail.OperatorID == operatorID &&
                          slabDetail.SlabMasterID == slabMasterID &&
                          slabDetail.IsDelete == false

                    select slabDetail).ToList();

            return (from slabDetail in list
                    where slabDetail.Amount == amount
                    && slabDetail.IsActive == true
                    select slabDetail).FirstOrDefault();
        }

        internal List<SelectFRCAmountResponse> SelectFRCAmount(int operatorID, int SlabMasterID)
        {
            List<DetailViewModel> list = Select();
            List<SelectFRCAmountResponse> objList = null;

            objList = (from slabDetail in list
                       where slabDetail.OperatorID == operatorID &&
                            slabDetail.SlabMasterID == SlabMasterID &&
                             slabDetail.IsDelete == false

                       select new SelectFRCAmountResponse
                       {
                           OperatorID = slabDetail.OperatorID,
                           Amount = slabDetail.Amount
                       }).Distinct().ToList();

            return objList;
        }
    }
}