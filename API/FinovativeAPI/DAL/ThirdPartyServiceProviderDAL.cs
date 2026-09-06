using Project.Model;
using Project.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ThirdPartyServiceProviderDAL
    {
        private readonly string key = "_ThirdPartyServiceProvider";

        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ThirdPartyServiceProvider>>();
            cache.Remove(key);
        }

        private List<ThirdPartyServiceProvider> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from dMTBank in context.ThirdPartyServiceProvider
                        where dMTBank.IsActiveForWebsite == true
                        select dMTBank).ToList();
            }
        }

        internal List<ThirdPartyServiceProvider> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<ThirdPartyServiceProvider>>();
            List<ThirdPartyServiceProvider> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        internal int Insert(ThirdPartyServiceProvider objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ThirdPartyServiceProvider.Add(objModel);
            return context.SaveChanges();
        }

        internal int Update(ThirdPartyServiceProvider ObjModel)
        {
            RemoveCache();
            int result = 1;
            try
            {
                using var context = new DatabaseContext();
                context.ThirdPartyServiceProvider.Update(ObjModel);
                context.SaveChanges();
                result = ObjModel.ThirdPartyServiceProviderID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal ThirdPartyServiceProvider SelectByPrimaryID(int thirdPartyServiceProviderID)
        {
            using var context = new DatabaseContext();
            return context.ThirdPartyServiceProvider.Find(thirdPartyServiceProviderID);
        }

        internal ThirdPartyServiceProvider SelectThirdPartyServiceProviderByName(string name)
        {
            List<ThirdPartyServiceProvider> DMTList = Select();
            return DMTList.FirstOrDefault(d => d.Name == name);
        }
    }
}