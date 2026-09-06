using Project.Model;
using Project.Processing;
using Project.Response.UPITransferSetting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UPITransferSettingDal
    {
        private readonly string key = "_UPITransferSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UPITransferSetting>>();
            cache.Remove(key);
        }

        internal List<UPITransferSetting> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<UPITransferSetting>>();
            List<UPITransferSetting> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<UPITransferSetting> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from dMTBank in context.UPITransferSetting
                        where dMTBank.IsActiveForWebsite == true
                        select dMTBank).ToList();
            }
        }

        internal int Update(UPITransferSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();

            int result = 1;
            try
            {
                context.UPITransferSetting.Update(ObjModel);
                context.SaveChanges();
                result = ObjModel.UPITransferSettingID;
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

        internal UPITransferSetting SelectByPrimaryID(int SettingID)
        {
            using var context = new DatabaseContext();
            return context.UPITransferSetting.Find(SettingID);
        }

        internal UPITransferSetting SelectActiveUPITransfer()
        {
            List<UPITransferSetting> List = Select();
            UPITransferSetting upiList = List.FirstOrDefault(obj => obj.IsActive == true && obj.IsActiveForWebsite == true);
            return upiList;
        }

        internal List<UPITransferSetting> SelectActiveUPITransferList()
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                return (from upi in context.UPITransferSetting
                        where upi.IsActiveForWebsite == true && upi.IsActive == true
                        select upi).ToList();
            }
        }

        internal List<ActiveUPIList> SelectActiveUPIList()
        {
            List<ActiveUPIList> list = new List<ActiveUPIList>();
            List<UPITransferSetting> DMTList = Select().Where(x => x.IsActive == true && x.IsActiveForWebsite == true).OrderBy(x => x.Sequence).ToList();

            foreach (UPITransferSetting d in DMTList)
            {
                ActiveUPIList active = new ActiveUPIList();
                active.Name = d.Name;
                active.DisplayName = d.DisplayName;
                active.IsCommonUPI = d.IsCommonUPI;
                list.Add(active);
            }
            return list;
        }

        internal List<ActiveUPIList> SelectUPITransferWithSender()
        {
            List<ActiveUPIList> list = new List<ActiveUPIList>();
            List<UPITransferSetting> DMTList = Select().Where(x => x.IsActive == true && x.IsActiveForWebsite == true).OrderBy(x => x.Sequence).ToList();

            foreach (UPITransferSetting d in DMTList)
            {
                ActiveUPIList active = new ActiveUPIList();
                active.Name = d.Name;
                active.DisplayName = d.DisplayName;
                active.IsUPITransferWithSender = d.IsUPITransferWithSender;
                list.Add(active);
            }
            return list;
        }

        internal UPITransferSetting SelectUPITransferByName(string name)
        {
            List<UPITransferSetting> UPITransferList = Select();
            return UPITransferList.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }
    }
}