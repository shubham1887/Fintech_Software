using Project.Model;
using Project.Processing;
using Project.Requests.ServiceAndUserwiseSubscription;
using Project.Response.ServiceAndUserwiseSubscription;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class ServiceAndUserwiseSubscriptionDal
    {
        private readonly string key = "_ServiceAndUserwiseSubscription";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ServiceAndUserwiseSubscription>>();
            cache.Remove(key);
        }

        public List<ServiceAndUserwiseSubscription> Select()
        {
            var _List = new MemoryCacheProcessing<List<ServiceAndUserwiseSubscription>>();
            List<ServiceAndUserwiseSubscription> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;

        }

        private List<ServiceAndUserwiseSubscription> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.ServiceAndUserwiseSubscription.ToList();
            }
        }

        public int Insert(ServiceAndUserwiseSubscription ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ServiceAndUserwiseSubscription.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.ServiceAndUserwiseSubscriptionID;
            }
        }

        internal int Update(ServiceAndUserwiseSubscription ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ServiceAndUserwiseSubscription.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.ServiceAndUserwiseSubscriptionID;
            }
        }

        internal int Delete(int ServiceAndUserwiseSubscriptionID)
        {
            RemoveCache();
            using var context = new DatabaseContext();

            ServiceAndUserwiseSubscription existingRecord = context.ServiceAndUserwiseSubscription.Find(ServiceAndUserwiseSubscriptionID);

            if (existingRecord == null) return 0;

            context.ServiceAndUserwiseSubscription.Remove(existingRecord);
            context.SaveChanges();
            return existingRecord.ServiceAndUserwiseSubscriptionID;

        }


        internal ServiceAndUserwiseSubscription CheckDuplicate(ServiceAndUserwiseSubscription request)
        {

            List<ServiceAndUserwiseSubscription> list = Select();
            ServiceAndUserwiseSubscription existingRecord = list.Where(S => S.UserID == request.UserID
            && S.UserTypeID == request.UserTypeID
            && S.ServiceID == request.ServiceID && S.EndDate >= DateTime.Now).FirstOrDefault();
            return existingRecord;

        }


        internal List<ViewModel> SelectAll()
        {
            List<ServiceAndUserwiseSubscription> list = Select();

            List<UserMaster> userlist = new UserDAL().Select();

            List<UserTypeMaster> usertypelist = new UserTypeDAL().Select();

            List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();

            List<ServiceSubscriptionPlan> planlist = new ServiceSubscriptionPlanMasterDal().Select();

            return (from setting in list

                    join plantype in planlist on setting.ServiceSubscriptionPlanMasterID equals plantype.ServiceSubscriptionPlanMasterID into PlDetails
                    from leftPlans in PlDetails.DefaultIfEmpty()

                    join user in userlist on setting.UserID equals user.UserID into UDetails
                    from leftUser in UDetails.DefaultIfEmpty()

                    join usertype in usertypelist on setting.UserTypeID equals usertype.UserTypeID into UTypeDetails
                    from leftUsertype in UTypeDetails.DefaultIfEmpty()

                    join service in servicelist on setting.ServiceID equals service.ServiceID into SDetails
                    from leftService in SDetails.DefaultIfEmpty()

                    orderby setting.ServiceAndUserwiseSubscriptionID descending

                    select new ViewModel
                    {
                        ServiceAndUserwiseSubscriptionID = setting.ServiceAndUserwiseSubscriptionID,
                        UserName = leftUser?.FirmName.ConcatNameMobileAliasName(leftUser?.MobileNumber, leftUser?.AliasName) ?? StatusType.ALL,

                        UserID = setting.UserID,
                        LoginID = setting.LoginID,
                        UserTypeID = setting.UserTypeID,
                        UserType = leftUsertype?.UserType ?? StatusType.ALL,
                        IsAutoDebit = setting.IsAutoDebit,
                        ServiceID = setting.ServiceID,
                        ServiceName = leftService?.ServiceName ?? StatusType.ALL,
                        Charge = setting.Charge,
                        SubscriptionType = setting.SubscriptionType,
                        Remark = setting.Remark,
                        EndDate = setting.EndDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                        DOC = setting.DOC.LongFormatDate(),
                        OriginalCharge = setting.OriginalCharge,
                        ServiceSubscriptionPlanMasterID = setting.ServiceSubscriptionPlanMasterID,
                        PlanName = leftPlans?.PlanName ?? "",
                        Status = setting.Status
                    }).ToList();
        }

        internal ServiceAndUserwiseSubscription SelectByID(int ServiceAndUserwiseSubscriptionID)
        {
            List<ServiceAndUserwiseSubscription> list = Select();

            ServiceAndUserwiseSubscription existingRecord = list.Where(S => S.ServiceAndUserwiseSubscriptionID == ServiceAndUserwiseSubscriptionID).FirstOrDefault();
            return existingRecord;
        }

        internal ServiceAndUserwiseSubscription SelectByUserIDandServiceID(int UserID ,int ServiceID)
        {
            List<ServiceAndUserwiseSubscription> list = Select();

            ServiceAndUserwiseSubscription existingRecord = list.Where(S => S.UserID == UserID && S.ServiceID == ServiceID).FirstOrDefault();
            return existingRecord;
        }

        internal ServiceChargeResponse selectUserServiceSubscriptionByUserIDAndServiceID(int UserID, int ServiceID)
        {
            List<ServiceAndUserwiseSubscription> list = Select();
            List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();
            List<ServiceSubscriptionPlan> planlist = new ServiceSubscriptionPlanMasterDal().Select();
            return (from change in list

                    join plantype in planlist on change.ServiceSubscriptionPlanMasterID equals plantype.ServiceSubscriptionPlanMasterID into PlDetails
                    from leftPlans in PlDetails.DefaultIfEmpty()

                    join service in servicelist on change.ServiceID equals service.ServiceID into SDetails
                    from leftService in SDetails.DefaultIfEmpty()
                    where change.UserID == UserID
                    && change.ServiceID == ServiceID

                    select new ServiceChargeResponse
                    {
                        ServiceAndUserwiseSubscriptionID = change.ServiceAndUserwiseSubscriptionID,
                        UserID = change.UserID,
                        ServiceID = change.ServiceID,
                        ServiceName = leftService?.ServiceName ?? "",
                        Charge = change.Charge,
                        StartDate = change.DOC.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                        SubscriptionType = change.SubscriptionType,
                        EndDate = new Util().getEndDate(change.DaysCount, change.EndDate.ToString()),
                        OriginalCharge = change.OriginalCharge,
                        PlanName = leftPlans?.PlanName ?? "",
                    }).FirstOrDefault();
        }

        internal List<ViewModel> SelectChargeReport(int UserID, SelectRequest selectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ServiceAndUserwiseSubscription> list = Select();

            List<UserTypeMaster> usertypelist = new UserTypeDAL().Select();
            List<UserMaster> userlist = new UserDAL().Select();


            List<ServiceSubscriptionPlan> planlist = new ServiceSubscriptionPlanMasterDal().Select();
            List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();
            List<ViewModel> result = new();

            result = (from setting in list


                      join plantype in planlist on setting.ServiceSubscriptionPlanMasterID equals plantype.ServiceSubscriptionPlanMasterID into PlDetails
                      from leftPlans in PlDetails.DefaultIfEmpty()

                      join user in userlist on setting.UserID equals user.UserID into UDetails
                      from leftUser in UDetails.DefaultIfEmpty()

                      join usertype in usertypelist on setting.UserTypeID equals usertype.UserTypeID into UTypeDetails
                      from leftUsertype in UTypeDetails.DefaultIfEmpty()

                      join service in servicelist on setting.ServiceID equals service.ServiceID into SDetails
                      from leftService in SDetails.DefaultIfEmpty()

                      where setting.DOCInt >= FromDate
                       && setting.DOCInt <= ToDate
                       && (selectRequest.ServiceID != 0 ? setting.ServiceID == selectRequest.ServiceID : 2 > 1)
                       && (!selectRequest.SubscriptionType.EqualsIgnoreCase(StatusType.ALL) ? setting.SubscriptionType == selectRequest.SubscriptionType : 2 > 1)

                      orderby setting.ServiceAndUserwiseSubscriptionID descending

                      select new ViewModel
                      {
                          ServiceAndUserwiseSubscriptionID = setting.ServiceAndUserwiseSubscriptionID,
                          UserName = leftUser?.FirmName.ConcatNameMobileAliasName(leftUser?.MobileNumber, leftUser?.AliasName) ?? StatusType.ALL,
                          LoginID = setting.LoginID,
                          UserID = setting.UserID,
                          IsAutoDebit = setting.IsAutoDebit,
                          UserTypeID = setting.UserTypeID,
                          UserType = leftUsertype?.UserType ?? StatusType.ALL,
                          ServiceID = setting.ServiceID,
                          ServiceName = leftService?.ServiceName ?? StatusType.ALL,
                          Charge = setting.Charge,
                          SubscriptionType = setting.SubscriptionType,
                          Remark = setting.Remark,
                          EndDate = setting.EndDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                          DOC = setting.DOC.LongFormatDate(),
                          OriginalCharge = setting.OriginalCharge,
                          ServiceSubscriptionPlanMasterID = setting.ServiceSubscriptionPlanMasterID,
                          PlanName = leftPlans?.PlanName ?? "",
                      }).ToList();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserID > Company.AdminUserID)
            {
                return result = (from item in result
                                 where item.UserID == UserID
                                 select item).ToList();
            }

            if (Util.isRoleAdminOrSupport(UserTypeID) && selectRequest.UserID > 0)
            {
                return result = (from item in result
                                 where item.UserID == selectRequest.UserID
                                 select item).ToList();

            }

            return result;
        }


        internal List<UserServiceMapping> SelectUserWithServiceEnabledForSubscription(int UserTypeID)
        {

            List<UserMaster> userlist = new UserDAL().Select();

            if (UserTypeID > 0)
                userlist = userlist.Where(U => U.UserTypeID == UserTypeID).ToList();


            List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();
            List<UserServiceMapping> userservicelist = new UserServiceMappingDal().Select();


            userservicelist = (from Userservice in userservicelist
                               join service in servicelist on Userservice.ServiceID equals service.ServiceID
                               join user in userlist on Userservice.UserID equals user.UserID

                               where user.IsActive == true
                               && Userservice.IsActive == true
                               select new UserServiceMapping
                               {
                                   UserID = Userservice.UserID,
                                   ServiceID = service.ServiceID,
                               }).ToList();

            return userservicelist;
        }
    }
}
