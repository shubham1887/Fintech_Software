using Project.Model;
using Project.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class DematSettingDal
    {
        private readonly string key = "_DematSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<DematSetting>>();
            cache.Remove(key);
        }

        internal List<DematSetting> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<DematSetting>>();
            List<DematSetting> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<DematSetting> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from dMTBank in context.DematSetting
                        where dMTBank.IsActiveWeb == true
                        select dMTBank).ToList();
            }
        }

        internal int Update(DematSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();

            int result = 1;
            try
            {
                context.DematSetting.Update(ObjModel);
                context.SaveChanges();
                result = ObjModel.DematSettingID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal DematSetting SelectByPrimaryID(int DematSettingID)
        {
            using var context = new DatabaseContext();
            return context.DematSetting.Find(DematSettingID);
        }

        internal List<DematSetting> SelectActiveDematForDeActive()
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                return (from dMTBank in context.DematSetting
                        where dMTBank.IsActiveWeb == true && dMTBank.IsActive == true
                        select dMTBank).ToList();
            }
        }

        internal DematSetting SelectActiveDemat()
        {
            List<DematSetting> model = Select();
            return model.FirstOrDefault(d => d.IsActive == true);
        }
    }
}