using Project.Model;
using Project.Processing;
using Project.Response.LeadGenerationSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class LeadGenerationSettingDal
    {
        private readonly string Key = "_LeadGenerationSetting";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal LeadGenerationSetting SelectByID(int LeadGenerationSettingID)
        {
            using var context = new DatabaseContext();
            return context.LeadGenerationSetting.Find(LeadGenerationSettingID);
        }

        internal int Update(LeadGenerationSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.LeadGenerationSetting.Update(model);
                return context.SaveChanges();
            }
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
                return (from setting in context.LeadGenerationSetting
                        orderby setting.LeadGenerationSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            LeadGenerationSettingID = setting.LeadGenerationSettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            PublicKey = setting.PublicKey,
                            Token = setting.Token,
                            IsActive = setting.IsActive,
                            Mode = setting.Mode,
                            Sequence = setting.Sequence,
                            DisplayName = setting.DisplayName,
                        }).ToList();
            }
        }

        internal ViewModel SelectActiveLeadGeneration()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal LeadGenerationSetting SelectByName(string Name)
        {
            using var context = new DatabaseContext();
            return context.LeadGenerationSetting.FirstOrDefault(d => d.Name.ToLower() == Name.ToLower());
        }

        internal List<ActiveList> SelectActiveList()
        {
            List<ActiveList> list = new List<ActiveList>();
            List<ViewModel> List = Select().Where(x => x.IsActive == true).ToList();

            foreach (ViewModel d in List)
            {
                ActiveList active = new ActiveList();
                active.Name = d.Name;
                active.DisplayName = d.DisplayName;
                list.Add(active);
            }
            return list;
        }

        internal List<LeadGenerationSetting> SelectActiveAccountOpeningList()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                return (from upi in context.LeadGenerationSetting
                        where upi.IsActiveWeb == true && upi.IsActive == true
                        select upi).ToList();
            }
        }
    }
}