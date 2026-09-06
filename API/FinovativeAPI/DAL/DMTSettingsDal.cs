using Project.Model;
using Project.Processing;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class DMTSettingsDal
    {
        private readonly string key = "_DMTSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DMTSetting>>();
            cache.Remove(key);
        }

        internal List<DMTSetting> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<DMTSetting>>();
            List<DMTSetting> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<DMTSetting> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from dMTBank in context.DMTSetting
                        where dMTBank.IsActiveForWebsite == true
                        select dMTBank).ToList();
            }
        }

        internal List<DMTSetting> SelectForProvider(int UserTypeID)
        {
            List<DMTSetting> response = new List<DMTSetting>();

            List<DMTSetting> list = new DMTSettingsDal().Select();

            if (UserTypeID == (int)UserType.APIUser)
            {
                response = (from item in list
                            where item.IsPayoutDMT == true
                            && item.IsActiveForWebsite == true
                            select new DMTSetting
                            {
                                Name = item.Name
                            }).ToList();
            }
            else
            {
                response = (from item in list
                            where item.IsActiveForWebsite == true
                            select new DMTSetting
                            {
                                Name = item.Name
                            }).ToList();
            }

            return response;
        }


        internal int Update(DMTSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();

            int result = 1;
            try
            {
                context.DMTSetting.Update(ObjModel);
                context.SaveChanges();
                result = ObjModel.DMTSettingID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal DMTSetting SelectByPrimaryID(int DMTSettingID)
        {
            using var context = new DatabaseContext();
            return context.DMTSetting.Find(DMTSettingID);
        }

        internal string SelectActiveDMT()
        {
            RemoveCache();
            string ActiveDMT = "";
            List<DMTSetting> DMTList = Select().Where(x => x.IsActive == true && x.IsActiveForWebsite == true).OrderBy(x => x.Sequence).ToList();

            if (DMTList == null)
            {
                return "";
            }
            ActiveDMT = DMTList[0].Name;
            return string.IsNullOrEmpty(ActiveDMT) ? "" : ActiveDMT;
        }

        internal List<DMTSetting> SelectActiveDMTForDeActive()
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                return (from dMTBank in context.DMTSetting
                        where dMTBank.IsActiveForWebsite == true && dMTBank.IsActive == true
                        select dMTBank).ToList();
            }
        }

        internal DMTSetting SelectPayoutDMT(string DMTName)
        {
            List<DMTSetting> DMTList = Select();
            return DMTList.FirstOrDefault(d => d.IsPayoutDMT == true && d.Name == DMTName);
        }

        internal DMTSetting SelectDMTByName(string name)
        {
            List<DMTSetting> DMTList = Select();
            return DMTList.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        internal DMTSetting SelectActiveDMTByName(string name)
        {
            List<DMTSetting> DMTList = Select();
            return DMTList.FirstOrDefault(d => d.Name.ToLower() == name.ToLower() && d.IsActive == true);
        }

        internal List<ActiveDMTList> SelectActiveDMTList()
        {
            List<ActiveDMTList> list = new List<ActiveDMTList>();
            List<DMTSetting> DMTList = Select().Where(x => x.IsActive == true && x.IsActiveForWebsite == true).OrderBy(x => x.Sequence).ToList();

            foreach (DMTSetting d in DMTList)
            {
                ActiveDMTList active = new ActiveDMTList();
                active.Name = d.Name;
                active.DisplayName = d.DisplayName;
                active.IsPayoutDMT = d.IsPayoutDMT;
                active.ProviderCode = d.ProviderCode;
                list.Add(active);
            }
            return list;
        }
       
    }
}