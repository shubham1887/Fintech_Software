using Project.Model;
using Project.Processing;
using Project.Response.AEPSSetting;
using Project.Response.User;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class AEPSSettingDal
    {
        private readonly string Key = "_AEPSSetting";

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

        internal ViewModel SelectAEPSByName(string name)
        {
            List<ViewModel> AEPSList = Select();
            return AEPSList.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.AEPSSetting
                        orderby setting.AEPSSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            AEPSSettingID = setting.AEPSSettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,                      
                            PublicKey = setting.PublicKey,
                            Token = setting.Token,
                            MinimumAmount = setting.MinimumAmount,
                            MaximumAmount = setting.MaximumAmount,
                            IsActive = setting.IsActive,
                            DisplayName = setting.DisplayName,
                            Sequence = setting.Sequence,
                            IsActiveWeb = setting.IsActiveWeb,
                            Mode = setting.Mode,
                            IsMerchantVerify = setting.IsMerchantVerify
                        }).ToList();
            }
        }

        internal int Update(AEPSSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.AEPSSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal AEPSSetting SelectByID(int AEPSSettingID)
        {
            using var context = new DatabaseContext();
            return context.AEPSSetting.Find(AEPSSettingID);
        }

        internal ViewModel SelectActiveAEPS()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal List<ViewModel> SelectMultiActiveAEPS()
        {
            List<ViewModel> pgList = Select();

            pgList = pgList.Where(x => x.IsActive == true && x.IsActiveWeb == true).OrderBy(x => x.Sequence).ToList();
            return pgList;
        }

        public string SelectActiveAEPSName()
        {
            string ActiveAEPS = "";
            List<ViewModel> List = Select();

            ViewModel sett = List.FirstOrDefault(d => d.IsActive == true);
            if (sett == null)
            {
                return "";
            }
            ActiveAEPS = sett.Name;
            return string.IsNullOrEmpty(ActiveAEPS) ? "" : ActiveAEPS;
        }

        internal List<AEPSSetting> SelectActiveAEPSForDeActive()
        {
            using (var context = new DatabaseContext())
            {
                return (from obj in context.AEPSSetting
                        where obj.IsActiveWeb == true && obj.IsActive == true
                        select obj).ToList();
            }
        }
        public bool IsMerchantVerifyByAEPSName(string name)
        {
            string ActiveAEPS = "";
            List<ViewModel> List = Select();
            ViewModel sett = List.FirstOrDefault(d => d.IsActive == true && d.Name.EqualsIgnoreCase(name));
            if (sett == null)
            {
                return false;
            }
            return sett.IsMerchantVerify;
        }

        internal List<ActiveAEPSList> SelectActiveAEPSList()
        {
            List<ActiveAEPSList> list = new List<ActiveAEPSList>();
            List<ViewModel> AEPSList = Select().Where(x => x.IsActive == true && x.IsActiveWeb == true).OrderBy(x => x.Sequence).ToList();

            foreach (ViewModel d in AEPSList)
            {
                ActiveAEPSList active = new ActiveAEPSList();
                active.Name = d.Name;
                active.DisplayName = d.DisplayName;
                list.Add(active);
            }
            return list;
        }
         
    }
}