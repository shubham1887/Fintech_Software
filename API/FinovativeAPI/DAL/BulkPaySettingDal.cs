using Project.Model;
using Project.Processing;
using Project.Response.BulkPaySetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class BulkPaySettingDal
    {
        public static string Key = "_BulkPaySetting";

        public int BulkPaySettingID { get; private set; }

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
                return (from setting in context.BulkPaySetting
                        orderby setting.BulkPaySettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            BulkPaySettingID = setting.BulkPaySettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            Token = setting.Token,
                            ActionURL = setting.ActionURL,
                            StatusCheckURL = setting.StatusCheckURL,
                            MinimumAmount = setting.MinimumAmount,
                            MaximumAmount = setting.MaximumAmount,
                            IsActive = setting.IsActive,
                            Mode = setting.Mode,
                            IsPendingForFailed = setting.IsPendingForFailed
                        }).ToList();
            }
        }

        internal ViewModel SelectByName(string name)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.Name == name);
            return pg;
        }

        internal int Update(BulkPaySetting model)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.BulkPaySetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal int DeactiveAll()
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.BulkPaySetting.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }

        internal BulkPaySetting SelectByID(int BulkPaySettingID)
        {
            using var context = new DatabaseContext();

            return context.BulkPaySetting.Find(BulkPaySettingID);
        }

        internal ViewModel SelectByPGSettingID(int BulkPaySettingID)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.BulkPaySettingID == BulkPaySettingID);
            return pg;
        }

        internal ViewModel SelectActiveBulkPay()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }
    }
}