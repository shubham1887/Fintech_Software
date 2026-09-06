using Project.Model;
using Project.Processing;
using Project.Response.PlanSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PlanSettingDAL
    {
        private readonly string PlanSettingKey = "_PlanSetting";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(PlanSettingKey);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(PlanSettingKey, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.PlanSetting
                        orderby setting.PlanSettingID descending
                        where setting.IsActiveWeb == true
                        select new ViewModel
                        {
                            PlanSettingID = setting.PlanSettingID,
                            Name = setting.Name,
                            UserID = setting.UserID,
                            Token = setting.Token,
                            Circle = setting.Circle,
                            IsActive = setting.IsActive,
                            Password = setting.Password,
                        }).ToList();
            }
        }

        internal int Update(PlanSetting model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.PlanSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal PlanSetting SelectByID(int PaymentGatewaySettingID)
        {
            using var context = new DatabaseContext();
            return context.PlanSetting.Find(PaymentGatewaySettingID);
        }

        internal ViewModel SelectActivePlan()
        {
            List<ViewModel> pgList = Select();
            ViewModel pg = pgList.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }


        internal void UpdateUserPlanUtilization(int UserID, bool isPlanApi, bool resetCounts = false)
        {
            using (var dbContext = new DatabaseContext())
            {
                UserPlanUtilization utilization = dbContext.UserPlanUtilization.FirstOrDefault(u => u.UserID == UserID);

                if (utilization == null)
                {
                    utilization = new UserPlanUtilization
                    {
                        UserID = UserID
                    };

                    dbContext.UserPlanUtilization.Add(utilization);
                }

                if (resetCounts)
                {
                    utilization.PlanFetchCount = 0;
                    utilization.CircleFetchCount = 0;
                }
                else
                {
                    if (isPlanApi)
                    {
                        utilization.PlanFetchCount++;
                        utilization.TotalPlanCount++;
                    }
                    else
                    {
                        utilization.CircleFetchCount++;
                        utilization.TotalCircleCount++;
                    }
                }

                dbContext.SaveChanges();
            }
        }


        internal void UpdateUserBillFetchUtilization(int UserID, bool resetCounts = false)
        {
            using (var dbContext = new DatabaseContext())
            {
                UserPlanUtilization utilization = dbContext.UserPlanUtilization.FirstOrDefault(u => u.UserID == UserID);

                if (utilization == null)
                {
                    utilization = new UserPlanUtilization
                    {
                        UserID = UserID
                    };

                    dbContext.UserPlanUtilization.Add(utilization);
                }

                if (resetCounts)
                {
                    utilization.BillFetchCount = 0;
                }
                else
                {
                    utilization.BillFetchCount++;
                    utilization.TotalBillFetchCount++;
                }

                dbContext.SaveChanges();
            }
        }
    }
}