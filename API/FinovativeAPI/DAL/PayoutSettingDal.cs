using Project.Model;
using Project.Processing;
using Project.Response.PayoutSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PayoutSettingDal
    {
        public static string Key = "_PayoutSetting";

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

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.PayoutSetting
                        orderby setting.PayoutSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            PayoutSettingID = setting.PayoutSettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            Token = setting.Token,
                            ActionURL = setting.ActionURL,
                            StatusCheckURL = setting.StatusCheckURL,
                            MinimumAmount = setting.MinimumAmount,
                            MaximumAmount = setting.MaximumAmount,
                            IsActive = setting.IsActive,
                            Mode = setting.Mode
                        }).ToList();
            }
        }

        internal int Update(PayoutSetting model)  
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.PayoutSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal int DeactiveAll()
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.PayoutSetting.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }

        internal PayoutSetting SelectByID(int PayoutSettingID)
        {
            using var context = new DatabaseContext();
            return context.PayoutSetting.Find(PayoutSettingID);
        }

        internal ViewModel SelectByPGSettingID(int PayoutSettingID)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.PayoutSettingID == PayoutSettingID);
            return pg;
        }

        internal ViewModel SelectActivePayout()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal ViewModel SelectActivePayoutByName(string Name)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true && obj.Name == Name);
            return pg;
        }

        internal ViewModel SelectPayoutByName(string name)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.Name == name);
            return pg;
        }
    }
}