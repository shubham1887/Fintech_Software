using Project.Model;
using Project.Processing;
using Project.Response.SDKSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SDKSettingDal
    {
        private readonly string Key = "_SDKSetting";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        internal ViewModel SelectActiveSDK()
        {
            List<ViewModel> model = Select().Where(x => x.IsActive == true).ToList();
            return model.FirstOrDefault();
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.SDKSetting
                        orderby setting.SDKSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            SDKSettingID = setting.SDKSettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            URL = setting.URL,
                            PublicKey = setting.PublicKey,
                            Token = setting.Token,
                            IsActive = setting.IsActive,
                        }).ToList();
            }
        }

        internal SDKSetting SelectByID(int SettingID)
        {
            using var context = new DatabaseContext();
            return context.SDKSetting.Find(SettingID);
        }

        internal ViewModel SelectActiveCMS()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal int Update(SDKSetting objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.SDKSetting.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal int DeactiveAll()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.SDKSetting.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }
    }
}