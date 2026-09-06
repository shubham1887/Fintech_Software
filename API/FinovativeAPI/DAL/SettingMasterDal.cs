using Project.Model;
using Project.Processing;
using Project.Response.SettingMaster;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SettingMasterDal
    {
        private readonly string SettingMasterKey = "_SettingMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(SettingMasterKey);
        }

        internal int Update(SettingMaster ObjModel)
        {
            RemoveCache();

            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.SettingMaster.Update(ObjModel);
                context.SaveChanges();
                Result = ObjModel.SettingMasterID;
            }
            return Result;
        }

        internal List<ViewModel> Select()
        {
            var _settings = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> settings = _settings.GetOrCreate(SettingMasterKey, () => SelectWithoutCache());
            return settings;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            List<ViewModel> settingList = null;

            try
            {
                using (var context = new DatabaseContext())
                {
                    return (from settingMaster in context.SettingMaster
                            select new ViewModel
                            {
                                SettingMasterID = settingMaster.SettingMasterID,
                                KeyText = settingMaster.Key,
                                ValueText = settingMaster.Value,
                                IsSecure = settingMaster.IsSecure,
                                Description = settingMaster.Description,
                                IsPassword = settingMaster.IsPassword
                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return settingList;
        }

        internal string SelectByKey(string key)
        {
            ViewModel obj = Select().FirstOrDefault(S => S.KeyText == key);
            if (obj == null)
            {
                return "";
            }
            return obj.ValueText;
        }

        internal SettingMaster SelectByID(int SettingMasterID)
        {
            using var context = new DatabaseContext();
            return context.SettingMaster.Find(SettingMasterID);
        }

        internal SettingMaster SelectByOnlineDMT()
        {
            using var context = new DatabaseContext();
            return context.SettingMaster.FirstOrDefault(S => S.Key == "OnlineDMT");
        }

        internal string SelectByUnSecureKey(string key)
        {
            ViewModel model = Select().FirstOrDefault(S => S.KeyText == key && S.IsSecure == false);
            if (model == null)
            {
                return "";
            }
            return model.ValueText;
        }
    }
}