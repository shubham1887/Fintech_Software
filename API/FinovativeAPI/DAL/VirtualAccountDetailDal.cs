using Project.Model;
using Project.Processing;
using Project.Response.VirtualAccountDetails;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class VirtualAccountDetailDal
    {
        private readonly string key = "_UserPaymentCollectMapping";

        internal int Insert(UserPaymentCollectMapping ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserPaymentCollectMapping.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.UserPaymentCollectMappingID;
        }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from Details in context.UserPaymentCollectMapping
                        orderby Details.UserPaymentCollectMappingID descending
                        select new ViewModel
                        {
                            UserID = Details.UserID,
                            ProviderName = Details.ProviderName,
                            Key = Details.Key,                        
                            Value = Details.Value,                                         
                        }).ToList();
            }
        }

        internal ViewModel SelectByID(int UserID, string ProviderName, string Key)
        {
            ViewModel Record = null;
            List<ViewModel> list = Select();
          
            return Record = (from Detail in list
                             where Detail.UserID == UserID
                              && Detail.ProviderName == ProviderName
                              && Detail.Key == Key

                             select new ViewModel
                             {
                                 UserID = Detail.UserID,
                                 ProviderName = Detail.ProviderName,
                                 Key = Detail.Key,
                                 Value = Detail.Value,
                             }).FirstOrDefault();
        }

        internal ViewModel CheckDuplicate(int UserID, string ProviderNmae, string Key)
        {
            List<ViewModel> list = Select();

            ViewModel response = null;

            response = (from Detail in list
                        where Detail.UserID == UserID
                        && Detail.ProviderName == ProviderNmae
                        && Detail.Key == Key
                        select Detail).FirstOrDefault();

            return response;
        }

        internal ViewModel SelectByVirtualID(string VirtualID, string Key)
        {          
            return Select().FirstOrDefault(U => U.Value == VirtualID && U.Key == Key);
        }
    }
}