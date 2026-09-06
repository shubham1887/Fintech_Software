using Project.Model;
using Project.Processing;
using Project.Requests.ServiceAndUserwiseSubscription;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ServiceSubscriptionPlanMasterDal
    {
        private readonly string key = "_ServiceSubscriptionPlanMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ServiceSubscriptionPlan>>();
            cache.Remove(key);
        }

        internal List<ServiceSubscriptionPlan> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<ServiceSubscriptionPlan>>();
            List<ServiceSubscriptionPlan> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<ServiceSubscriptionPlan> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
           

            return (from setting in context.ServiceSubscriptionPlanMaster
                    select new ServiceSubscriptionPlan
                    {
                        ServiceSubscriptionPlanMasterID = setting.ServiceSubscriptionPlanMasterID,
                        PlanName = setting.PlanName,
                       
                    }).ToList();
        }

    }
}
