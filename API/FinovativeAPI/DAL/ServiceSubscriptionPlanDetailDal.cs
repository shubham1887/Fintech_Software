using Project.Model;
using Project.Processing;
using Project.Response.ServiceSubscription;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ServiceSubscriptionPlanDetailDal
    {
        private readonly string key = "_ServiceSubscriptionPlanDetail";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<SubscriptionDetailResponse>>();
            cache.Remove(key);
        }

        internal int Insert(ServiceSubscriptionPlanDetails ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ServiceSubscriptionPlanDetails.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.ServiceSubscriptionPlanDetailsID;
        }

        internal int Update(ServiceSubscriptionPlanDetails model)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ServiceSubscriptionPlanDetails.Update(model);
                return context.SaveChanges();
            }
        }
        internal List<SubscriptionDetailResponse> SelectServiceList(int ServiceSubscriptionCreatePlanMasterID)
        {
            List<SubscriptionDetailResponse> list = Select();
            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();          

            List<SubscriptionDetailResponse> listRet = list
           .Where(sp => sp.ServiceSubscriptionCreatePlanMasterID == ServiceSubscriptionCreatePlanMasterID)
           .Select(sp => new SubscriptionDetailResponse
           {
             ServiceID = sp.ServiceID,
             ServiceName = sp.ServiceID == -999 ? "BBPS" : services.FirstOrDefault(s => s.ServiceID == sp.ServiceID)?.ServiceName ?? "Unknown"
           })
            .OrderBy(sp => sp.ServiceID)
            .ToList();

            return listRet;
        }

        internal void DeleteRange(int ServiceSubscriptionCreatePlanMasterID)
        {
            RemoveCache();
            using var context = new DatabaseContext();

            List<ServiceSubscriptionPlanDetails> existingRecord = context.ServiceSubscriptionPlanDetails.Where(U => U.ServiceSubscriptionCreatePlanMasterID == ServiceSubscriptionCreatePlanMasterID).ToList();

            if (existingRecord == null) return;

            context.ServiceSubscriptionPlanDetails.RemoveRange(existingRecord);
            context.SaveChanges();
            return;
        }

        internal List<SubscriptionDetailResponse> Select()
        {
            var _list = new MemoryCacheProcessing<List<SubscriptionDetailResponse>>();
            List<SubscriptionDetailResponse> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<SubscriptionDetailResponse> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.ServiceSubscriptionPlanDetails
                        orderby setting.ServiceSubscriptionPlanDetailsID descending
                        select new SubscriptionDetailResponse
                        {
                            ServiceSubscriptionPlanDetailsID = setting.ServiceSubscriptionPlanDetailsID,
                            ServiceSubscriptionCreatePlanMasterID = setting.ServiceSubscriptionCreatePlanMasterID,
                            ServiceID = setting.ServiceID,
                        }).ToList();
            }
        }

        internal bool CheckDuplicate(int PlanDetailID, int SreviceID)
        {
            List<SubscriptionDetailResponse> list = Select();

            SubscriptionDetailResponse resp = (from item in list
                                               where item.ServiceSubscriptionPlanDetailsID == PlanDetailID
                                               && item.ServiceID == SreviceID
                                               select item).FirstOrDefault();
            if (resp == null)
                return true;

            return false;
        }
    }
}