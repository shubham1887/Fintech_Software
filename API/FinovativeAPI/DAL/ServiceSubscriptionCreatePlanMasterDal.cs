using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using Project.Response.ServiceSubscription;
using System.Linq;
using Project.StaticModel;
using System;

namespace Project.DAL
{
    public class ServiceSubscriptionCreatePlanMasterDal
    {
        private readonly string key = "_ServiceSubscriptionCreatePlanMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal int Insert(ServiceSubscriptionCreatePlanMaster ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.ServiceSubscriptionCreatePlanMaster.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.ServiceSubscriptionCreatePlanMasterID;
        }

        internal ServiceSubscriptionCreatePlanMaster SelectByID(int ServiceSubscriptionCreatePlanMasterID)
        {
            using var context = new DatabaseContext();
            return context.ServiceSubscriptionCreatePlanMaster.Find(ServiceSubscriptionCreatePlanMasterID);
        }

        internal ViewModel SelectByplanID(int ServiceSubscriptionCreatePlanMasterID)
        {
            List<ViewModel> List = Select();
            ViewModel resp = List.FirstOrDefault(obj => obj.ServiceSubscriptionCreatePlanMasterID == ServiceSubscriptionCreatePlanMasterID);
            return resp;
        }

        internal AssignPlanViewModel SelectByplanDetailID(int ServiceSubscriptionCreatePlanMasterID)
        {
            List<ViewModel> List = Select();
            return (from item in List
                    where item.ServiceSubscriptionCreatePlanMasterID == ServiceSubscriptionCreatePlanMasterID
                    select new AssignPlanViewModel
                    {
                        ServiceSubscriptionCreatePlanMasterID = ServiceSubscriptionCreatePlanMasterID,
                        PlanName = item.PlanName,
                        TotalAmount = item.Amount,
                        GST = item.Amount * TaxInfo.Inclusion18Percentage,
                        Amount = getAmount(item.Amount)
                    }).FirstOrDefault();
        }

        private decimal getAmount(decimal Amount)
        {
            decimal CalAmount = 0;

            decimal GST = Amount * TaxInfo.Inclusion18Percentage;
            CalAmount = Amount - GST;
            return CalAmount;
        }

        internal int Update(ServiceSubscriptionCreatePlanMaster model)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ServiceSubscriptionCreatePlanMaster.Update(model);
                return context.SaveChanges();
            }
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        internal List<ViewModel> SelectPlan(int ServiceID, int UserID)
        {
            List<ViewModel> retlist = new List<ViewModel>();
            List<ViewModel> list = Select();
            if (ServiceID > 0)
            {
                List<SubscriptionDetailResponse> detaillist = new ServiceSubscriptionPlanDetailDal().Select();
                return (from item in detaillist
                        join Mainlist in list on item.ServiceSubscriptionCreatePlanMasterID equals Mainlist.ServiceSubscriptionCreatePlanMasterID
                        where item.ServiceID == ServiceID                       
                        select new ViewModel
                        {
                            ServiceSubscriptionCreatePlanMasterID = item.ServiceSubscriptionCreatePlanMasterID,
                            PlanName = Mainlist.PlanName,
                            Amount = Mainlist.Amount,
                            ImageURL = Mainlist.ImageURL
                        }).ToList();
            }

            List<Project.Response.ServiceSubscription.AssignPlanViewModel> planmaplist = new AssignSubscriptionChargeDal().SelectwithlistbyuserID(UserID);
            return (from item in list
                    join planmap in planmaplist
                    on item.ServiceSubscriptionCreatePlanMasterID equals planmap.ServiceSubscriptionCreatePlanMasterID
                    into planGroup
                    from plan in planGroup.DefaultIfEmpty()
                    select new ViewModel
                    {
                        ServiceSubscriptionCreatePlanMasterID = item.ServiceSubscriptionCreatePlanMasterID,
                        PlanName = item.PlanName,
                        Amount = item.Amount,
                        ImageURL = item.ImageURL,
                        IsPlanPurchase = (plan?.UserID ?? 0) == 0 ? false : true,
                        Services = new ServiceSubscriptionPlanDetailDal().SelectServiceList(item.ServiceSubscriptionCreatePlanMasterID) // Fetch service list here
                    }).ToList();
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.ServiceSubscriptionCreatePlanMaster
                        orderby setting.ServiceSubscriptionCreatePlanMasterID descending
                        select new ViewModel
                        {
                            ServiceSubscriptionCreatePlanMasterID = setting.ServiceSubscriptionCreatePlanMasterID,
                            PlanName = setting.PlanName,
                            Amount = setting.Amount,
                            ImageURL = String.Concat(Company.WebAPIURL, setting.ImageURL),
                        }).ToList();
            }
        }

        internal int Delete(int ServiceSubscriptionCreatePlanID)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                var slectedUserID = context.ServiceSubscriptionCreatePlanMaster.Where(U => U.ServiceSubscriptionCreatePlanMasterID == ServiceSubscriptionCreatePlanID);
                context.ServiceSubscriptionCreatePlanMaster.RemoveRange(slectedUserID);
                context.SaveChanges();
            }
            return 1;
        }

    }
}