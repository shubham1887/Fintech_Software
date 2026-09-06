using Project.Model;
using Project.Processing;
using Project.Response.CMSSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CMSSettingDal
    {
        private readonly string Key = "_CMSSetting";

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

        internal ViewModel SelectActiveCMSByName(string name)
        {
            List<ViewModel> model = Select();
            return model.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.CMSSetting
                        orderby setting.CMSSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            CMSSettingID = setting.CMSSettingID,
                            Name = setting.Name,
                            MerchantID = setting.MerchantID,
                            Slat = setting.Slat,
                            PublicKey = setting.PublicKey,
                            Token = setting.Token,
                            IsActive = setting.IsActive,
                            Mode = setting.Mode
                        }).ToList();
            }
        }

        internal CMSSetting SelectByID(int CMSSettingID)
        {
            using var context = new DatabaseContext();
            return context.CMSSetting.Find(CMSSettingID);
        }

        internal ViewModel SelectActiveCMS()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal List<ViewModel> SelectMultiActiveCMS()
        {
            List<ViewModel> pgList = Select();

            pgList = pgList.Where(x => x.IsActive == true).ToList();
            return pgList;
        }

        internal int Update(CMSSetting objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.CMSSetting.Update(objModel);
                return context.SaveChanges();
            }
        }

        internal int DeactiveAll()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.CMSSetting.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }
    }
}