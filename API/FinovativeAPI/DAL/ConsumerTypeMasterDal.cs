using Project.Model;
using Project.Processing;
using Project.Requests.LeadGenerationMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ConsumerTypeMasterDal
    {
        private readonly string key = "_ConsumerTypeMasterDal";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ConsumerTypes>>();
            cache.Remove(key);
        }

        internal List<ConsumerTypes> SelectConsumerTypes()
        {
            var _settingList = new MemoryCacheProcessing<List<ConsumerTypes>>();
            List<ConsumerTypes> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<ConsumerTypes> SelectWithoutCache()
        {
            using var context = new DatabaseContext();


            return (from setting in context.ConsumerTypeMaster
                    select new ConsumerTypes
                    {
                        ConsumerTypeMasterID = setting.ConsumerTypeMasterID,
                        ConsumerType = setting.ConsumerType,

                    }).ToList();
        }
    }
}
