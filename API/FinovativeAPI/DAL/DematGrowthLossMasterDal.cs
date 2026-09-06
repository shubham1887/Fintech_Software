using Project.Model;
using Project.Processing;
using Project.Requests.DematGrowthLoss;
using Project.Requests.ServiceAndUserwiseSubscription;
using Project.Response.DematGrowthLossMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace Project.DAL
{
    public class DematGrowthLossMasterDal
    {
        public static readonly string Key = "_DematGrowthLossMaster";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }
        internal int Insert(DematGrowthLossMaster model)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.DematGrowthLossMaster.Add(model);
                context.SaveChanges();
                return model.DematGrowthLossMasterID;
            }
        }
        internal int Update(DematGrowthLossMaster model)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.DematGrowthLossMaster.Update(model);
                context.SaveChanges();
                return model.DematGrowthLossMasterID;
            }
        }
        internal DematGrowthLossMaster SelectByID(int DematGrowthLossMasterID)
        {
            using var context = new DatabaseContext();
            return context.DematGrowthLossMaster.Find(DematGrowthLossMasterID);
        }
        internal ViewModel SelectByIDForEdit(int DematGrowthLossMasterID)
        {
            List<ViewModel> list = null;
            List<ViewModel> DematGrowthLossMaster = Select();

            List<UserMaster> Userlist = new UserDAL().Select();
            List<ServiceSubscriptionPlan> planlist = new ServiceSubscriptionPlanMasterDal().Select();

           return   (from call in DematGrowthLossMaster
                     join users in Userlist on call.UserID equals users.UserID

            

                    join plantype in planlist on call.ServiceSubscriptionPlanMasterID equals plantype.ServiceSubscriptionPlanMasterID into PlDetails
                    from leftPlans in PlDetails.DefaultIfEmpty()
                     where call.DematGrowthLossMasterID == DematGrowthLossMasterID

                     orderby call.DematGrowthLossMasterID descending
                    select new Response.DematGrowthLossMaster.ViewModel
                    {
                        DematGrowthLossMasterID = call.DematGrowthLossMasterID,
                        UserID = call.UserID,
                        FirmName = users.FirmName,


                        ServiceID = call.ServiceID,
                        ServiceName = call.ServiceName,

                        Amount = call.Amount,

                        ServiceSubscriptionPlanMasterID = call.ServiceSubscriptionPlanMasterID,
                        Plan = leftPlans?.PlanName ?? "",
                 
                        GrowthLoss = call.GrowthLoss,
                        APIProviderId = call.APIProviderId,
                        DOC = call.DOC,
                        DOCInt = call.DOCInt,
                        ManualDate= call.ManualDate,
                    }).FirstOrDefault();

        }
        internal List<ViewModel> SelectByUserID(int UserID)
        {
            List<ViewModel> list = Select();
            List<UserMaster> userList = new UserDAL().Select();

            return (from item in list
                    join user in userList on item.ServiceSubscriptionPlanMasterID equals user.UserTypeID
                    where item.UserID == UserID
                    select new ViewModel
                    {
                        DematGrowthLossMasterID = item.DematGrowthLossMasterID,
                        UserID = user.UserID,
                        Amount = item.Amount,
                        ServiceSubscriptionPlanMasterID = item.ServiceSubscriptionPlanMasterID,
                        GrowthLoss = item.GrowthLoss,
                        APIProviderId = item.APIProviderId,
                    }).ToList();
        }
        internal List<ViewModel> Select()
        {
            RemoveCache();
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }
        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from call in context.DematGrowthLossMaster
                        orderby call.ManualDate descending
                        where call.IsDelete == false

                        select new ViewModel
                        {
                            DematGrowthLossMasterID = call.DematGrowthLossMasterID,
                            UserID = call.UserID,
                            Amount = call.Amount,
                            ServiceSubscriptionPlanMasterID = call.ServiceSubscriptionPlanMasterID,
                            ServiceID = call.ServiceMaster.ServiceID,
                            ServiceName = call.ServiceMaster.Name,
                            GrowthLoss = call.GrowthLoss,
                            APIProviderId = call.APIProviderId,
                            DOC = call.ManualDate.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            DOCInt = call.DOCInt,
                            ManualDate=call.ManualDate
                        }).ToList();
            }
        }
        internal List<ViewModel> SelectAll()
        {
            List<ViewModel> list = null;
            List<ViewModel> DematGrowthLossMaster = Select();

            List<UserMaster> Userlist = new UserDAL().Select();
            List<ServiceSubscriptionPlan> planlist = new ServiceSubscriptionPlanMasterDal().Select();

           

            list = (from call in DematGrowthLossMaster
                    join users in Userlist on call.UserID equals users.UserID  

                    join plantype in planlist on call.ServiceSubscriptionPlanMasterID equals plantype.ServiceSubscriptionPlanMasterID into PlDetails
                    from leftPlans in PlDetails.DefaultIfEmpty()
                                

                    orderby call.DematGrowthLossMasterID descending
                    select new Response.DematGrowthLossMaster.ViewModel
                    {
                        DematGrowthLossMasterID = call.DematGrowthLossMasterID,
                        UserID = call.UserID,
                     
                        FirmName = users.FirmName,
                        ServiceID = call.ServiceID,
                        ServiceName = call.ServiceName,

                        Amount = call.Amount,
                        ServiceSubscriptionPlanMasterID = call.ServiceSubscriptionPlanMasterID,
                        Plan = leftPlans?.PlanName ?? "",
                  
                        GrowthLoss = call.GrowthLoss,
                        APIProviderId = call.APIProviderId,
                        DOC = call.DOC,
                        DOCInt = call.DOCInt,
                       

                    }).ToList();
            return list;
        }

        internal List<ViewModel> SelectReport(int UserID, SelectReportRequest selectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = null;
            List<ViewModel> DematGrowthLossMaster = Select();

            List<UserMaster> Userlist = new UserDAL().Select();
            List<ServiceSubscriptionPlan> planlist = new ServiceSubscriptionPlanMasterDal().Select();



            list = (from call in DematGrowthLossMaster
                    join users in Userlist on call.UserID equals users.UserID

                    join plantype in planlist on call.ServiceSubscriptionPlanMasterID equals plantype.ServiceSubscriptionPlanMasterID into PlDetails
                    from leftPlans in PlDetails.DefaultIfEmpty()

                    where call.DOCInt >= FromDate
                    && call.DOCInt <= ToDate

                    orderby call.DematGrowthLossMasterID descending
                    select new Response.DematGrowthLossMaster.ViewModel
                    {
                        DematGrowthLossMasterID = call.DematGrowthLossMasterID,
                        UserID = call.UserID,

                        FirmName = users.FirmName,

                        ServiceID = call.ServiceID,
                        ServiceName = call.ServiceName,

                        Amount = call.Amount,
                        ServiceSubscriptionPlanMasterID = call.ServiceSubscriptionPlanMasterID,
                        Plan = leftPlans?.PlanName ?? "",

                        GrowthLoss = call.GrowthLoss,
                        APIProviderId = call.APIProviderId,
                        DOC = call.DOC,
                        DOCInt = call.DOCInt
                    }).ToList();



            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserID > Company.AdminUserID)
            {
                return list = (from item in list
                               where item.UserID == UserID
                                 select item).ToList();
            }

            if (Util.isRoleAdminOrSupport(UserTypeID) && selectRequest.UserID > 0)
            {
                return list = (from item in list
                                 where item.UserID == selectRequest.UserID
                                 select item).ToList();

            }
            return list;
        }

    }
}

