using Project.Model;
using Project.Processing;
using Project.Response.Provider;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderDetailsDAL
    {
        private readonly string ProviderDetailKey = "_ProviderDetail";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DetailsViewModel>>();
            cache.Remove(ProviderDetailKey);
        }

        internal int Insert(ProviderDetails objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderDetails.Add(objModel);
            return context.SaveChanges();
        }

        internal int Update(ProviderDetails objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderDetails.Update(objModel);
            return context.SaveChanges();
        }

        internal ProviderDetails SelectByID(int providerdetailsID)
        {
            using var context = new DatabaseContext();
            return context.ProviderDetails.Find(providerdetailsID);
        }

        internal List<DetailsViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<DetailsViewModel>>();

            List<DetailsViewModel> list = _list.GetOrCreate(ProviderDetailKey, () => SelectWithoutCache());

            return list;
        }

        private List<DetailsViewModel> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            return (from providerDetails in context.ProviderDetails
                    where providerDetails.IsDelete == false
                    orderby providerDetails.ProviderID descending
                    select new DetailsViewModel
                    {
                        ProviderID = providerDetails.ProviderID,
                        ServiceID = providerDetails.ServiceID,
                        ProviderName = providerDetails.ProviderMaster.ProviderName,
                        ServiceName = providerDetails.ServiceMaster.Name,
                        ProviderDetailID = providerDetails.ProviderDetailID,
                        MethodType = providerDetails.MethodType,
                        HeaderJson = providerDetails.HeaderJson ?? "0",
                        DateFormat = providerDetails.DateFormat ?? "",
                        ProviderURL = providerDetails.ProviderURL,
                        UrlType = providerDetails.UrlType,
                        TransactionIDPrefix = providerDetails.TransactionIDPrefix,
                        IsActive = providerDetails.IsActive,
                        IsDelete = providerDetails.IsDelete,
                    }).ToList();
        }

        internal DetailsViewModel SelectByProviderDetailID(int providerDetailID)
        {
            List<DetailsViewModel> list = Select();

            return (from providerDetails in list

                    where providerDetails.ProviderDetailID == providerDetailID
                    && providerDetails.IsDelete == false

                    select providerDetails).FirstOrDefault();
        }

        internal ProviderDetails SelectURL(int ProviderID, int ServiceID, string Type)
        {
            List<DetailsViewModel> list = Select();

            return (from providerDetails in list

                    where providerDetails.ProviderID == ProviderID
                    && providerDetails.ServiceID == ServiceID
                    && providerDetails.UrlType == Type
                    && providerDetails.IsDelete == false

                    select new ProviderDetails
                    {
                        ServiceID = providerDetails.ServiceID,
                        ProviderID = providerDetails.ProviderID,
                        MethodType = providerDetails.MethodType,
                        HeaderJson = providerDetails.HeaderJson ?? "0",
                        DateFormat = providerDetails.DateFormat ?? "",
                        ProviderURL = providerDetails.ProviderURL,
                        UrlType = providerDetails.UrlType,
                        TransactionIDPrefix = providerDetails.TransactionIDPrefix ?? "",
                    }).FirstOrDefault();
        }

        internal ProviderDetails SelectForDuplicate(int ProviderID, int ServiceID, string Type)
        {
            List<DetailsViewModel> list = Select();

            return (from providerDetails in list

                    where providerDetails.ProviderID == ProviderID
                    && providerDetails.ServiceID == ServiceID
                    && providerDetails.UrlType == Type
                    && providerDetails.IsDelete == false
                    select new ProviderDetails
                    {
                        ServiceID = providerDetails.ServiceID,
                        ProviderID = providerDetails.ProviderID,
                        MethodType = providerDetails.MethodType,
                        HeaderJson = providerDetails.HeaderJson ?? "0",
                        DateFormat = providerDetails.DateFormat ?? "",
                        ProviderURL = providerDetails.ProviderURL,
                        UrlType = providerDetails.UrlType,
                    }).FirstOrDefault();
        }

        internal ProviderDetails SelectURLForMobile(int ProviderID, string Type)
        {
            List<DetailsViewModel> list = Select();
            return (from providerDetails in list
                    where providerDetails.ProviderID == ProviderID
                    && providerDetails.UrlType == Type
                    && providerDetails.IsDelete == false
                    && (providerDetails.ServiceID == (int)ServiceCode.Mobile ||
                        providerDetails.ServiceID == (int)ServiceCode.Dth ||

                        providerDetails.ServiceID == (int)ServiceCode.PostPaid)

                    select new ProviderDetails
                    {
                        ServiceID = providerDetails.ServiceID,
                        ProviderID = providerDetails.ProviderID,
                        MethodType = providerDetails.MethodType,
                        HeaderJson = providerDetails.HeaderJson ?? "0",
                        DateFormat = providerDetails.DateFormat ?? "",
                        ProviderURL = providerDetails.ProviderURL,
                        UrlType = providerDetails.UrlType,
                    }).FirstOrDefault();
        }

        internal List<DetailsViewModel> SelectByProviderID(int providerID)
        {
            List<DetailsViewModel> list = Select();

            return (from providerDetails in list
                    where providerDetails.ProviderID == providerID
                    && providerDetails.IsDelete == false
                    select new DetailsViewModel
                    {
                        ProviderID = providerDetails.ProviderID,
                        ServiceID = providerDetails.ServiceID,
                        ProviderName = providerDetails.ProviderName,
                        ServiceName = providerDetails.ServiceName,
                        ProviderDetailID = providerDetails.ProviderDetailID,
                        MethodType = providerDetails.MethodType,
                        HeaderJson = providerDetails.HeaderJson ?? "0",
                        DateFormat = providerDetails.DateFormat ?? "",
                        ProviderURL = providerDetails.ProviderURL,
                        UrlType = providerDetails.UrlType,
                        TransactionIDPrefix = providerDetails.TransactionIDPrefix ?? "",
                        IsActive = providerDetails.IsActive,
                        IsDelete = providerDetails.IsDelete,
                    }).ToList();
        }

        internal DetailsViewModel GetBalanceURL(int providerID)
        {
            List<DetailsViewModel> list = Select();

            return (from providerDetails in list
                    where providerDetails.ProviderID == providerID
                    && providerDetails.IsDelete == false
                    && providerDetails.UrlType == UrlTypes.Balance
                    select new DetailsViewModel
                    {
                        ProviderID = providerDetails.ProviderID,
                        ServiceID = providerDetails.ServiceID,
                        MethodType = providerDetails.MethodType,
                        HeaderJson = providerDetails.HeaderJson ?? "0",
                        DateFormat = providerDetails.DateFormat ?? "",
                        ProviderURL = providerDetails.ProviderURL,
                    }).FirstOrDefault();
        }

        internal DetailsViewModel GetBillFetchURL(int providerID)
        {
            List<DetailsViewModel> list = Select();

            return (from providerDetails in list
                    where providerDetails.ProviderID == providerID
                    && providerDetails.IsDelete == false
                    && providerDetails.UrlType == UrlTypes.View
                    select new DetailsViewModel
                    {
                        ProviderID = providerDetails.ProviderID,
                        ServiceID = providerDetails.ServiceID,
                        MethodType = providerDetails.MethodType,
                        HeaderJson = providerDetails.HeaderJson ?? "0",
                        DateFormat = providerDetails.DateFormat ?? "",
                        ProviderURL = providerDetails.ProviderURL
                    }).FirstOrDefault();
        }

        internal DetailsViewModel GetBillFetchURLByServiceID(int providerID, int ServiceID)
        {
            List<DetailsViewModel> list = Select();

            return (from providerDetails in list
                    where providerDetails.ProviderID == providerID
                    && providerDetails.ServiceID == ServiceID
                    && providerDetails.IsDelete == false
                    && providerDetails.UrlType == UrlTypes.View
                    select new DetailsViewModel
                    {
                        ProviderID = providerDetails.ProviderID,
                        ServiceID = providerDetails.ServiceID,
                        MethodType = providerDetails.MethodType,
                        HeaderJson = providerDetails.HeaderJson ?? "0",
                        DateFormat = providerDetails.DateFormat ?? "",
                        ProviderURL = providerDetails.ProviderURL
                    }).FirstOrDefault();
        }

        internal DetailsViewModel SelectForDuplicateSampleInsert(ProviderDetails objModel)
        {
            DetailsViewModel obj;
            List<DetailsViewModel> list = Select();
            obj = (from item in list
                   where item.ServiceID == objModel.ServiceID
                   && item.ProviderID == objModel.ProviderID
                           && item.UrlType == objModel.UrlType
                           && item.ProviderURL == objModel.ProviderURL
                           && item.MethodType == objModel.MethodType
                           && item.HeaderJson == objModel.HeaderJson
                           && item.DateFormat == objModel.DateFormat
                   orderby item.ProviderDetailID descending
                   select item).FirstOrDefault();
            return obj;
        }
    }
}