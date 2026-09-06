using Project.Model;
using Project.Processing;
using Project.Response.AccountOpenSetting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class AccountOpenSettingDal
    {
        private readonly string key = "_AccountOpenSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<AccountOpenSetting>>();
            cache.Remove(key);
        }

        internal List<AccountOpenSetting> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<AccountOpenSetting>>();
            List<AccountOpenSetting> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<AccountOpenSetting> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from dMTBank in context.AccountOpenSetting
                        where dMTBank.IsActiveForWebsite == true
                        select dMTBank).ToList();
            }
        }

        internal int Update(AccountOpenSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();

            int result = 1;
            try
            {
                context.AccountOpenSetting.Update(ObjModel);
                context.SaveChanges();
                result = ObjModel.AccountOpenSettingID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal int DeactiveAll()
        {
            RemoveCache();

            using (var context = new DatabaseContext())
            {
                context.UPITransferSetting.ToList().ForEach(a => a.IsActive = false);
                return context.SaveChanges();
            }
        }

        internal AccountOpenSetting SelectByPrimaryID(int SettingID)
        {
            using var context = new DatabaseContext();
            return context.AccountOpenSetting.Find(SettingID);
        }

        internal AccountOpenSetting SelectActiveAccountOpening()
        {
            List<AccountOpenSetting> List = Select();
            AccountOpenSetting upiList = List.FirstOrDefault(obj => obj.IsActive == true && obj.IsActiveForWebsite == true);
            return upiList;
        }

        internal List<AccountOpenSetting> SelectActiveAccountOpeningList()
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                return (from upi in context.AccountOpenSetting
                        where upi.IsActiveForWebsite == true && upi.IsActive == true
                        select upi).ToList();
            }
        }

        internal List<ActiveList> SelectActiveList()
        {
            List<ActiveList> list = new List<ActiveList>();
            List<AccountOpenSetting> DMTList = Select().Where(x => x.IsActive == true && x.IsActiveForWebsite == true).ToList();

            foreach (AccountOpenSetting d in DMTList)
            {
                ActiveList active = new ActiveList();
                active.Name = d.Name;
                list.Add(active);
            }
            return list;
        }

        internal AccountOpenSetting SelectAccountOpeningByName(string name)
        {
            List<AccountOpenSetting> UPITransferList = Select();
            return UPITransferList.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }
    }
}