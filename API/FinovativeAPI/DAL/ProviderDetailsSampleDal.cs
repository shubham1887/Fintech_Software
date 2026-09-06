using Project.Model;
using Project.Processing;
using Project.Response.Provider;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ProviderDetailsSampleDal
    {
        private readonly string ProviderDetailsSampleKEY = "_ProviderDetailsSample";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ProviderDetailsSample>>();
            cache.Remove(ProviderDetailsSampleKEY);
        }

        internal List<ProviderDetailsSample> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<ProviderDetailsSample>>();
            List<ProviderDetailsSample> list = _list.GetOrCreate(ProviderDetailsSampleKEY, () => SelectWithoutCache());
            return list;
        }

        private List<ProviderDetailsSample> SelectWithoutCache()
        {
            using var context = new DatabaseContext();

            return context.ProviderDetailsSample.ToList();
        }

        internal List<ProviderSampleViewModel> Select()
        {
            List<ProviderDetailsSample> APISample = SelectAll();
            using (var context = new DatabaseContext())
            {
                return (from API in APISample
                        orderby API.ProviderDetailsSampleID descending
                        select new ProviderSampleViewModel
                        {
                            ProviderDetailsSampleID = API.ProviderDetailsSampleID,
                            ProviderName = API.ProviderName,
                            ProviderURL = API.ProviderURL,
                            UrlType = API.UrlType,
                            MethodType = API.MethodType,
                            ContentType = API.ContentType,
                        }).ToList();
            }
        }

        internal ProviderSampleViewModel SelectByID(int ProviderDetailsSampleID)
        {
            List<ProviderDetailsSample> APISample = SelectAll();
            List<UrlTypeMaster> urltype = new UrlTypeMasterDal().SelectAll();

            return (from API in APISample
                    join type in urltype on API.UrlType equals type.UrlType
                    where API.ProviderDetailsSampleID == ProviderDetailsSampleID
                    select new ProviderSampleViewModel
                    {
                        UrlTypeID = type.UrlTypeID,
                        ProviderDetailsSampleID = API.ProviderDetailsSampleID,
                        ProviderName = API.ProviderName,
                        ProviderURL = API.ProviderURL,
                        UrlType = API.UrlType,
                        MethodType = API.MethodType,
                        ContentType = API.ContentType,
                        ServiceID = API.ServiceID,
                    }).FirstOrDefault();
        }

        internal int Delete()
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ProviderDetailsSample.RemoveRange(context.ProviderDetailsSample);
            return context.SaveChanges();
        }

        internal int Insert(ProviderDetailsSample Providerdetailsdample)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ProviderDetailsSample.Add(Providerdetailsdample);
                context.SaveChanges();
                return Providerdetailsdample.ProviderDetailsSampleID;
            }
        }

        internal List<DeveloperViewModel> SelectDeveloperForDropDown()
        {
            List<ProviderDetailsSample> list = SelectAll();
            var result = list.GroupBy(test => test.ProviderName)
                   .Select(grp => grp.FirstOrDefault())
                   .ToList();

            List<DeveloperViewModel> objList = null;
            objList = (from model in result
                       select new DeveloperViewModel
                       {
                           Developer = model.ProviderName
                       }).Distinct().ToList();

            return objList;
        }

        internal List<ProviderSampleViewModel> SelectByDeveloper(string Developer)
        {
            List<ProviderDetailsSample> APISample = SelectAll();
            List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();

            using (var context = new DatabaseContext())
            {
                return (from API in APISample
                        join service in Servicelist on API.ServiceID equals service.ServiceID
                        where API.ProviderName == Developer
                        select new ProviderSampleViewModel
                        {
                            //  UrlTypeID = type.UrlTypeID,
                            ProviderDetailsSampleID = API.ProviderDetailsSampleID,
                            ProviderName = API.ProviderName,
                            ProviderURL = API.ProviderURL,
                            UrlType = API.UrlType,
                            MethodType = API.MethodType,
                            ContentType = API.ContentType,
                            ServiceID = API.ServiceID,
                            ServiceName = service.Name,
                            DateFormat = API.DateFormat,
                        }).ToList();
            }
        }
    }
}