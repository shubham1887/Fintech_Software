using Project.Model;
using Project.Processing;
using Project.Response.ProviderResponse;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderResponseMasterDal
    {
        private string key = "_ProviderResponse";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal int Delete(int ProviderResponseMasterID)
        {
            RemoveCache();

            using var context = new DatabaseContext();
            ProviderResponseMaster ObjModel = context.ProviderResponseMaster.FirstOrDefault(P => P.ProviderResponseMasterID == ProviderResponseMasterID);
            context.Remove(ObjModel);
            int Result = context.SaveChanges();

            return Result;
        }

        internal int Insert(ProviderResponseMaster ObjModel)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.ProviderResponseMaster.Add(ObjModel);
                context.SaveChanges();
            }

            return ObjModel.ProviderResponseMasterID;
        }

        internal int Update(ProviderResponseMaster objModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ProviderResponseMaster.Update(objModel);
                context.SaveChanges();
            }
            return objModel.ProviderResponseMasterID;
        }

        internal List<ViewModel> Select(int ProviderID)
        {
            List<ViewModel> list = SelectAll();

            return (from item in list
                    where item.ProviderID == ProviderID
                    orderby item.Type, item.Status ascending
                    select item).ToList();
        }

        private List<ViewModel> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        internal List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> list = null;
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            list = (from ProviderResponse in context.ProviderResponseMaster
                    select new ViewModel
                    {
                        ProviderID = ProviderResponse.ProviderID,
                        Type = ProviderResponse.Type,
                        TextMustExists = ProviderResponse.TextMustexits,
                        IsActive = ProviderResponse.IsActive,
                        IsRefCode = ProviderResponse.IsReference,
                        Response = ProviderResponse.Response,
                        Status = ProviderResponse.Status,
                        ProviderResponseMasterID = ProviderResponse.ProviderResponseMasterID
                    }).ToList();

            return list;
        }

        internal ViewModel SelectIsReference(int ProviderID, string textMustExist)
        {
            List<ViewModel> list = SelectAll();

            return (from item in list
                    where item.ProviderID == ProviderID
                    && item.TextMustExists.Contains(textMustExist)
                    && item.IsRefCode == true
                    && item.IsActive == true
                    select item).FirstOrDefault();
        }

        private List<ViewModel> SelectForParsingByProviderID(int ProviderID, string Status, string Type)
        {
            List<ViewModel> list = SelectAll();
            list = (from item in list
                    where item.ProviderID == ProviderID
                    && item.Status == Status
                    && item.Type == Type
                    && item.IsActive == true
                    orderby item.TextMustExists.Length descending, item.Response.Length descending
                    select item).ToList();
            return list;
        }

        private List<ViewModel> SelectForParsingByResponseGroupID(int ProviderID, string Status, string Type)
        {
            int ResponseGroupID = new ProviderMasterDAL().SelectResponseGroupIDByProviderID(ProviderID);

            List<ViewModel> list = SelectAll();
            list = (from item in list
                    where item.ProviderID == ResponseGroupID
                    && item.Status == Status
                    && item.Type == Type
                    && item.IsActive == true
                    orderby item.TextMustExists.Length descending, item.Response.Length descending
                    select item).ToList();
            return list;
        }

        internal List<ViewModel> SelectForParsing(int ProviderID, string Status, string Type)
        {
            List<ViewModel> listByProvider = SelectForParsingByProviderID(ProviderID, Status, Type);
            List<ViewModel> listByProviderResponseGroupID = SelectForParsingByResponseGroupID(ProviderID, Status, Type);

            List<ViewModel> list = listByProvider.Union(listByProviderResponseGroupID).ToList();

            list = (from item in list
                    orderby item.TextMustExists.Length descending, item.Response.Length descending
                    select item).ToList();

            return list;
        }

        internal ProviderResponseMaster SelectByID(int providerResponseMasterID)
        {
            using var context = new DatabaseContext();
            return context.ProviderResponseMaster.FirstOrDefault(F => F.ProviderResponseMasterID == providerResponseMasterID);
        }

        internal ViewModel SelectForDuplicate(ProviderResponseMaster objModel)
        {
            ViewModel obj;
            List<ViewModel> list = SelectAll();
            obj = (from item in list
                   where item.Type == objModel.Type
                           && item.Status == objModel.Status
                           && item.Response == objModel.Response
                           && item.TextMustExists == objModel.TextMustexits
                           && item.ProviderID == objModel.ProviderID
                   orderby item.ProviderResponseMasterID descending
                   select item).FirstOrDefault();
            return obj;
        }
    }
}