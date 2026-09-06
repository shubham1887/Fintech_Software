using Project.Model;
using Project.Processing;
using Project.Response.OTTSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OTTSettingDal
    {
        public static string key = "_OTTSetting";

        private void removeCache()
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
                return (from setting in context.OTTSetting
                        orderby setting.OTTSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            OTTSettingID = setting.OTTSettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            Token = setting.Token,
                            ActionURL = setting.ActionURL,
                            StatusCheckURL = setting.StatusCheckURL,
                            MinimumAmount = setting.MinimumAmount,
                            MaximumAmount = setting.MaximumAmount,
                            DisplayName = setting.DisplayName,
                            Sequence = setting.Sequence,
                            IsActive = setting.IsActive,
                            Mode = setting.Mode
                        }).ToList();
            }
        }

        internal int Update(OTTSetting model)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OTTSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal int DeactiveALL()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OTTSetting.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }

        internal OTTSetting SelectByID(int OTTSettingID)
        {
            using var context = new DatabaseContext();
            return context.OTTSetting.Find(OTTSettingID);
        }

        internal OTTSetting SelectByName(string Name)
        {
            using var context = new DatabaseContext();
            return context.OTTSetting.FirstOrDefault(d => d.Name.ToLower() == Name.ToLower());
        }

        internal ViewModel SelectByPGSettingID(int OTTSettingID)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.OTTSettingID == OTTSettingID);
            return pg;
        }

        internal ViewModel SelectActiveOTT()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal List<ViewModel> SelectMultiActiveOTT()
        {
            List<ViewModel> pgList = Select();

            pgList = pgList.Where(x => x.IsActive == true && x.IsActiveForWebsite == true).OrderBy(x => x.Sequence).ToList();
            return pgList;
        }

        internal List<OTTSetting> SelectActiveOTTForDeActive()
        {
            using (var context = new DatabaseContext())
            {
                return (from obj in context.OTTSetting
                        where obj.IsActiveWeb == true && obj.IsActive == true
                        select obj).ToList();
            }
        }

        internal List<ActiveList> SelectActiveOTTList()
        {
            List<ActiveList> list = new List<ActiveList>();
            List<ViewModel> OTTList = Select().Where(x => x.IsActive == true).OrderBy(x => x.Sequence).ToList();

            foreach (ViewModel d in OTTList)
            {
                ActiveList active = new ActiveList();
                active.Name = d.Name;
                active.DisplayName = d.DisplayName;
                list.Add(active);
            }
            return list;
        }
    }
}