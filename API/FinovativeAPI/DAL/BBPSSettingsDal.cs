using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class BBPSSettingsDal
    {
        private readonly string key = "_BBPSSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<BBPSSettings>>();
            cache.Remove(key);
        }

        internal List<BBPSSettings> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<BBPSSettings>>();
            List<BBPSSettings> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<BBPSSettings> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from BBPS in context.BBPSSettings
                        where BBPS.IsActiveForWebsite == true
                        select BBPS).ToList();
            }
        }

        internal BBPSSettings SelectBBPSByName(string name)
        {
            List<BBPSSettings> List = Select();
            return List.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }
    }
}
