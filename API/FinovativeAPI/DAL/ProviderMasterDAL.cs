using Project.Model;
using Project.Processing;
using Project.Response.Provider;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderMasterDAL
    {
        private readonly string Key = "_ProviderMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(ProviderMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderMaster.Add(objModel);
            return context.SaveChanges();
        }

        internal int Update(ProviderMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderMaster.Update(objModel);
            return context.SaveChanges();
        }

        internal bool IsRealAPI(int providerID)
        {
            ProviderMaster ObjModel = null;
            using (var context = new DatabaseContext())
            {
                ObjModel = context.ProviderMaster.Find(providerID);
            }
            if (ObjModel == null) return false;
            return ObjModel.IsRealAPI;
        }

        internal ProviderMaster SelectByID(int providerID)
        {
            using var context = new DatabaseContext();
            return context.ProviderMaster.Find(providerID);
        }

        internal ProviderMaster SelectByProviderUniqueID(string ProviderUniqueID)
        {
            using var context = new DatabaseContext();
            return context.ProviderMaster.FirstOrDefault(P => P.ProviderUniqueID == ProviderUniqueID && P.IsActive == true && P.IsDelete == false);
        }

        internal ProviderMaster SelectActiveByID(int ProviderID)
        {
            using var context = new DatabaseContext();
            return context.ProviderMaster.FirstOrDefault(P => P.ProviderID == ProviderID && P.IsActive == true && P.IsDelete == false);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        internal ViewModel CacheSelectByProviderID(int ProviderID)
        {
            List<ViewModel> list = Select();
            return (from item in list
                    where item.ProviderID == ProviderID
                    select item).FirstOrDefault();
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return (from Provider in context.ProviderMaster
                    where Provider.IsDelete == false
                    orderby Provider.ProviderID ascending
                    select new ViewModel
                    {
                        ProviderID = Provider.ProviderID,
                        ProviderName = Provider.ProviderName,
                        ProviderUrl = Company.WebAPIURL + "Callback/response/" + Provider.ProviderUniqueID,
                        RechargeBalance = Provider.RechargeBalance,
                        DMTBalance = Provider.DMTBalance,
                        ProviderUniqueID = Provider.ProviderUniqueID,
                        ResponseGroupID = Provider.ResponseGroupID,
                        IsActive = Provider.IsActive,
                        IPAddress = Provider.IPAddress ?? "0",
                        IsHLR = Provider.IsHLR,
                        IsRealAPI = Provider.IsRealAPI,
                        LoginID = Provider.LoginID,
                        Token = Provider.Token ?? "",
                        SupportNumber = Provider.SupportNumber,
                        LowBalanceAmount = Provider.LowBalanceAmount
                    }).ToList();
        }

        internal List<ViewModel> SelectOnlyActiveProviderID()
        {
            List<ViewModel> list = Select();

            List<ViewModel> response = (from item in list
                                        where item.IsActive == true
                                        select item
                                      ).ToList();
            return response;
        }

        internal List<ViewModel> SelectOnlyActivePayUrlTypeProviderID(int ServiceID)
        {
            List<DetailsViewModel> details = new ProviderDetailsDAL().Select();
            List<ViewModel> list = Select();
            List<ViewModel> response = null;
            if (ServiceID == (int)ServiceCode.Mobile ||
                ServiceID == (int)ServiceCode.Dth ||
                ServiceID == (int)ServiceCode.PostPaid)
            {
                response = (from item in list
                            join detail in details on item.ProviderID equals detail.ProviderID
                            where detail.UrlType == UrlTypes.Pay
                            && (detail.ServiceID == (int)ServiceCode.Mobile ||
                            ServiceID == (int)ServiceCode.Dth ||
                            ServiceID == (int)ServiceCode.PostPaid)
                            && item.IsActive == true
                            select item).Distinct().ToList();
            }
            else
            {
                response = (from item in list
                            join detail in details on item.ProviderID equals detail.ProviderID
                            where detail.UrlType == UrlTypes.Pay
                            && detail.ServiceID == ServiceID
                            && item.IsActive == true
                            select item).Distinct().ToList();
            }

            return response;
        }

        internal List<ViewModel> SelectOnlyActivePayUrlTypeProviderIDAll()
        {
            List<DetailsViewModel> details = new ProviderDetailsDAL().Select();
            List<ViewModel> list = Select();
            List<ViewModel> response = (from item in list
                                        join detail in details on item.ProviderID equals detail.ProviderID
                                        where detail.UrlType == UrlTypes.Pay
                                        && item.IsActive == true
                                        select item).ToList();

            return response;
        }

        internal int SelectProviderIDByUniqueNo(string aPIGUID)
        {
            List<ViewModel> list = Select();

            ViewModel item = list.FirstOrDefault(P => P.ProviderUniqueID.EqualsIgnoreCase(aPIGUID));
            if (item == null)
            {
                return 0;
            }
            return item.ProviderID;
        }

        internal int SelectResponseGroupIDByProviderID(int ProviderID)
        {
            List<ViewModel> list = Select();

            ViewModel ObjModel = list.FirstOrDefault(P => P.ProviderID == ProviderID);

            return (ObjModel == null) ? 0 : ObjModel.ResponseGroupID;
        }

        internal List<DropDownViewModel> SelectForViewAPIDropDown()
        {
            List<ViewModel> list = Select();
            List<DetailsViewModel> details = new ProviderDetailsDAL().Select();

            return (from Provider in list
                    join detail in details on Provider.ProviderID equals detail.ProviderID
                    where detail.UrlType == UrlTypes.View

                    select new DropDownViewModel
                    {
                        ProviderID = Provider.ProviderID,
                        ProviderName = Provider.ProviderName
                    }).DistinctBy(p => p.ProviderID).ToList();
        }

        internal string SelectProviderNameByProviderID(int ProviderID)
        {
            List<ViewModel> list = Select();

            ViewModel item = list.FirstOrDefault(P => P.ProviderID == ProviderID);
            if (item == null)
            {
                return "";
            }
            return item.ProviderName;
        }

        internal ViewModel SelectByProviderID(int ProviderID)
        {
            List<ViewModel> list = Select();

            ViewModel item = list.FirstOrDefault(P => P.ProviderID == ProviderID);
            return item;
        }
    }
}