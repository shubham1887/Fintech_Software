using Project.Model;
using Project.Processing;
using Project.Response.eKYCSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class eKYCSettingDal
    {
        public static string Key = "_eKYCSetting";

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
                return (from setting in context.eKYCSetting
                        orderby setting.eKYCSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            eKYCSettingID = setting.eKYCSettingID,
                            Name = setting.Name,
                            Slat = setting.Slat,
                            Token = setting.Token,
                            AuthKey = setting.AuthKey,
                            ChargeAmount = setting.ChargeAmount,
                            EncryptionKey = setting.EncryptionKey,
                            MerchantID = setting.MerchantID,
                            IsActive = setting.IsActive,
                            Mode = setting.Mode,
                        }).ToList();
            }
        }

        internal int Update(eKYCSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.eKYCSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal int DeactiveAll()
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.eKYCSetting.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }

        internal eKYCSetting SelectByID(int eKYCSettingID)
        {
            using var context = new DatabaseContext();
            return context.eKYCSetting.Find(eKYCSettingID);
        }

        internal ViewModel SelectByeKYCSettingID(int eKYCSettingID)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.eKYCSettingID == eKYCSettingID);
            return pg;
        }

        internal ViewModel SelectActiveeKYCByName(string Name)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true && obj.Name == Name);
            return pg;
        }

        internal ViewModel SelectActiveeKYC()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }

        internal ViewModel SelecteKYCByName(string name)
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.Name == name);
            return pg;
        }
        internal List<eKYCSetting> SelectActiveEKycForDeActive()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                return (from eKYC in context.eKYCSetting
                        where eKYC.IsActive == true
                        select eKYC).ToList();
            }
        }
    }
}