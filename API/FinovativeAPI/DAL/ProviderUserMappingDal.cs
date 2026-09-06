using Project.Model;
using Project.Processing;
using Project.Response.ProviderUserMapping;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Project.DAL
{
    public class ProviderUserMappingDal
    {
        private readonly string ProviderUserMappingKey = "_ProviderUserMapping";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(ProviderUserMappingKey);
        }
        internal int Insert(ProviderUserMapping model)
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.ProviderUserMapping.Add(model);
                return context.SaveChanges();
            }
        }

        internal List<ViewModel> Select()
        {
            var _settings = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> settings = _settings.GetOrCreate(ProviderUserMappingKey, () => SelectWithoutCache());
            return settings;
        }


        private List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> MappingList = null;

            try
            {
                using (var context = new DatabaseContext())
                {
                    return (from Obj in context.ProviderUserMapping
                            select new ViewModel
                            {
                                ProviderUserMappingID = Obj.ProviderUserMappingID,
                                Key = Obj.Key,
                                Value = Obj.Value,
                                ProviderID = Obj.ProviderID,
                                UserID = Obj.UserID,
                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return MappingList;
        }

        internal string SelectByKey(int UserID, int ProviderID, string Key)
        {
            ViewModel obj = Select().FirstOrDefault(S => S.UserID == UserID && S.ProviderID == ProviderID && S.Key== Key);
            if (obj == null)
            {
                return "";
            }
            return obj.Value;
        }
        internal bool CheckDuplicate(int UserID, int ProviderID, string Key)
        {
            List<ViewModel> list = Select();
            ViewModel obj = null;
            if (list != null)
            {
                obj = (from item in list
                       where item.UserID == UserID
                       && item.ProviderID== ProviderID
                       && item.Key== Key
                       select item).FirstOrDefault();
            }
            if (obj != null)
                return true;
            return false;
        }

    }
}
