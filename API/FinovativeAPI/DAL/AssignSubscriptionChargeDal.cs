using Project.Model;
using Project.Processing;
using Project.Response.ServiceSubscription;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class AssignSubscriptionChargeDal
    {
        public readonly string key = "UserSubscriptionPlanMapping";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<AssignPlanViewModel>>();
            cache.Remove(key);
        }

        public int Insert(UserSubscriptionPlanMapping ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UserSubscriptionPlanMapping.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.UserSubscriptionPlanMappingID;
            }
        }

        internal AssignPlanViewModel SelectByUserID(int UserID)
        {
            List<AssignPlanViewModel> List = Select();
            AssignPlanViewModel resp = List.FirstOrDefault(obj => obj.UserID == UserID);
            return resp;
        }

        internal List<AssignPlanViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<AssignPlanViewModel>>();
            List<AssignPlanViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<AssignPlanViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.UserSubscriptionPlanMapping
                        orderby setting.UserSubscriptionPlanMappingID descending
                        select new AssignPlanViewModel
                        {
                            AssignSubscriptionPlanID = setting.UserSubscriptionPlanMappingID,
                            ServiceSubscriptionCreatePlanMasterID = setting.ServiceSubscriptionCreatePlanMasterID,
                            UserID = setting.UserID,
                            PlanName = setting.PlanName,
                            Amount = setting.Amount
                        }).ToList();
            }
        }

        internal bool CheckDuplicate(int UserID, int CreatePlanID)
        {
            List<AssignPlanViewModel> list = Select();
            AssignPlanViewModel resp = (from item in list
                                        where item.UserID == UserID
                                        && item.ServiceSubscriptionCreatePlanMasterID == CreatePlanID
                                        select item).FirstOrDefault();
            if (resp == null)
                return true;
            return false;
        }

        internal List<AssignPlanViewModel> SelectwithlistbyuserID(int UserID)
        {
            List<AssignPlanViewModel> list = Select();
            return (from item in list
                    where item.UserID == UserID
                    select item).ToList();

        }
    }
}