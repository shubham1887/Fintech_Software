using Project.Model;
using Project.Processing;
using Project.Response.BusSettingMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class BusSettingMasterDal
    {
        public static string key = "_BusSettingMaster";

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
                return (from setting in context.BusSettingMaster
                        orderby setting.BusSettingMasterID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            BusSettingMasterID = setting.BusSettingMasterID,
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

        internal int Update(BusSettingMaster model)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.BusSettingMaster.Update(model);
                return context.SaveChanges();
            }
        }

        internal int DeactiveALL()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.BusSettingMaster.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }

        internal BusSettingMaster SelectByID(int BusSettingMasterID)
        {
            using var context = new DatabaseContext();
            return context.BusSettingMaster.Find(BusSettingMasterID);
        }

        internal ViewModel SelectByPGSettingID(int BusSettingMasterID)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.BusSettingMasterID == BusSettingMasterID);
            return pg;
        }

        internal ViewModel SelectActiveBusSetting()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal List<ViewModel> SelectMultiActiveBusSetting()
        {
            List<ViewModel> pgList = Select();

            pgList = pgList.Where(x => x.IsActive == true && x.IsActiveForWebsite == true).OrderBy(x => x.Sequence).ToList();
            return pgList;
        }

        internal List<BusSettingMaster> SelectActiveBusSettingForDeActive()
        {
            using (var context = new DatabaseContext())
            {
                return (from obj in context.BusSettingMaster
                        where obj.IsActiveWeb == true && obj.IsActive == true
                        select obj).ToList();
            }
        }

        internal List<ActiveList> SelectActiveBusSettingList()
        {
            List<ActiveList> list = new List<ActiveList>();
            List<ViewModel> BusList = Select().Where(x => x.IsActive == true).OrderBy(x => x.Sequence).ToList();

            foreach (ViewModel d in BusList)
            {
                ActiveList active = new ActiveList();
                active.Name = d.Name;
                active.DisplayName = d.DisplayName;
                list.Add(active);
            }
            return list;
        }

        internal BusSettingMaster SelectByName(string Name)
        {
            using var context = new DatabaseContext();
            return context.BusSettingMaster.FirstOrDefault(d => d.Name.ToLower() == Name.ToLower());
        }
    }
}