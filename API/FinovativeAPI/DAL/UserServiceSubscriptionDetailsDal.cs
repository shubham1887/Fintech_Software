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
    public class UserServiceSubscriptionDetailsDal
    {
        private readonly string key = "_UserServiceSubscriptionDetails";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserServiceSubscriptionDetails>>();
            cache.Remove(key);
        }

        private List<UserServiceSubscriptionDetails> Select()
        {
            var _List = new MemoryCacheProcessing<List<UserServiceSubscriptionDetails>>();
            List<UserServiceSubscriptionDetails> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;

        }

        private List<UserServiceSubscriptionDetails> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.UserServiceSubscriptionDetails.ToList();
            }
        }

        public int Insert(UserServiceSubscriptionDetails ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UserServiceSubscriptionDetails.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.UserServiceSubscriptionDetailsID;
            }
        }

        internal int Update(UserServiceSubscriptionDetails ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UserServiceSubscriptionDetails.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.UserServiceSubscriptionDetailsID;
            }
        }


        internal void UpdateRange(List<UserServiceSubscriptionDetails> ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UserServiceSubscriptionDetails.UpdateRange(ObjModel);
                context.SaveChanges();
                return;
            }
        }

        internal void DeleteRange(int ServiceAndUserwiseSubscriptionID)
        {
            RemoveCache();
            using var context = new DatabaseContext();

            List<UserServiceSubscriptionDetails> existingRecord = context.UserServiceSubscriptionDetails.Where(U => U.ServiceAndUserwiseSubscriptionID == ServiceAndUserwiseSubscriptionID).ToList();

            if (existingRecord == null) return;

            context.UserServiceSubscriptionDetails.RemoveRange(existingRecord);
            context.SaveChanges();
            return;

        }


        internal void InsertRange(List<UserServiceSubscriptionDetails> ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UserServiceSubscriptionDetails.AddRange(ObjModel);
                context.SaveChanges();
            }
        }


        internal UserServiceSubscriptionDetails CheckDuplicate(int UserID, int ServiceID)
        {

            List<UserServiceSubscriptionDetails> list = Select();

            UserServiceSubscriptionDetails existingRecord = list.Where(S => S.UserID == UserID && S.ServiceID == ServiceID).FirstOrDefault();

            return existingRecord;
        }

        internal List<ViewModel> SelectUserWiseSubscriptionCharge(int UserID)
        {
            int datebefore1days = Convert.ToInt32(DateTime.Now.AddDays(-1).ToString(Messages.DOCIntegerFomrat));

            List<ServiceAndUserwiseSubscription> settinglist = new ServiceAndUserwiseSubscriptionDal().Select();

            List<UserServiceSubscriptionDetails> userServiceChargeList = Select();

            List<Requests.ServiceAndUserwiseSubscription.ServiceSubscriptionPlan> planlist = new ServiceSubscriptionPlanMasterDal().Select();

            List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();


            DateTime today = DateTime.Now.Date;

            List<ViewModel> listOfUserServiceWise = (from setting in settinglist
                                                     join user in userServiceChargeList on setting.ServiceAndUserwiseSubscriptionID equals user.ServiceAndUserwiseSubscriptionID

                                                     join plantype in planlist on setting.ServiceSubscriptionPlanMasterID equals plantype.ServiceSubscriptionPlanMasterID into PlDetails
                                                     from leftPlans in PlDetails.DefaultIfEmpty()

                                                     join service in servicelist on setting.ServiceID equals service.ServiceID into SDetails
                                                     from leftService in SDetails.DefaultIfEmpty()

                                                     where                                                         
                                                           setting.UserID == UserID &&
                                                           setting.IsAutoDebit == false
                                                     orderby setting.ServiceAndUserwiseSubscriptionID descending
                                                     select new ViewModel
                                                     {
                                                         ServiceAndUserwiseSubscriptionID = setting.ServiceAndUserwiseSubscriptionID,
                                                         UserID = user.UserID,
                                                         ServiceName = leftService?.ServiceName ?? StatusType.ALL,
                                                         IsAutoDebit = setting.IsAutoDebit,
                                                         LoginID = setting.LoginID,
                                                         Charge = user.Charge,
                                                         SubscriptionType = setting.SubscriptionType,
                                                         DayCount = setting.DaysCount,
                                                         EndDate = new Util().getEndDate(setting.DaysCount, setting.EndDate.ToString()),
                                                         DOC = setting.DOC.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                                                         //  DOC = setting.DOC.LongFormatDate(),
                                                         StartDate = setting.DOC.Date,
                                                         OriginalCharge = setting.OriginalCharge,
                                                         ServiceSubscriptionPlanMasterID = setting.ServiceSubscriptionPlanMasterID,
                                                         PlanName = leftPlans?.PlanName ?? "",
                                                     }
                                                    ).GroupBy(vm => vm.ServiceAndUserwiseSubscriptionID)
                                                    .Select(group => group.First())
                                                    .ToList();
            return listOfUserServiceWise;
        }

        internal List<ViewModel> SelectPendingSubscriptionCharge(SelectPendingRequest Request, int UserID)
        {
            int datebefore1days = Convert.ToInt32(DateTime.Now.AddDays(-1).ToString(Messages.DOCIntegerFomrat));

            List<ServiceAndUserwiseSubscription> settinglist = new ServiceAndUserwiseSubscriptionDal().Select();

            List<UserServiceSubscriptionDetails> userServiceChargeList = Select();

            List<Requests.ServiceAndUserwiseSubscription.ServiceSubscriptionPlan> planlist = new ServiceSubscriptionPlanMasterDal().Select();

            List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();

            List<UserMaster> userList = new UserDAL().Select();

            List<UserTypeMaster> usertypelist = new UserTypeDAL().Select();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            DateTime today = DateTime.Now.Date;

            List<ViewModel> listOfUserServiceWise = (from setting in settinglist
                                                     join usercharge in userServiceChargeList on setting.ServiceAndUserwiseSubscriptionID equals usercharge.ServiceAndUserwiseSubscriptionID

                                                     join users in userList on setting.UserID equals users.UserID
                                                     join plantype in planlist on setting.ServiceSubscriptionPlanMasterID equals plantype.ServiceSubscriptionPlanMasterID into PlDetails
                                                     from leftPlans in PlDetails.DefaultIfEmpty()
                                                     join service in servicelist on setting.ServiceID equals service.ServiceID into SDetails
                                                     from leftService in SDetails.DefaultIfEmpty()

                                                     join usertype in usertypelist on setting.UserTypeID equals usertype.UserTypeID into UTypeDetails
                                                     from leftUsertype in UTypeDetails.DefaultIfEmpty()
                                                     where
                                                     setting.EndDate <= today
                                                     orderby setting.ServiceAndUserwiseSubscriptionID descending
                                                     select new ViewModel
                                                     {
                                                         ServiceAndUserwiseSubscriptionID = setting.ServiceAndUserwiseSubscriptionID,
                                                         UserID = usercharge.UserID,
                                                         ParentID = users.ParentID,
                                                         UserName = users?.FirmName.ConcatNameMobileAliasName(users?.MobileNumber, users?.AliasName) ?? StatusType.ALL,
                                                         ServiceName = leftService?.ServiceName ?? StatusType.ALL,
                                                         IsAutoDebit = setting.IsAutoDebit,
                                                         LoginID = setting.LoginID,
                                                         UserType = leftUsertype?.UserType ?? StatusType.ALL,
                                                         Charge = Util.isRoleAdminOrSupport(UserTypeID) ? usercharge.Charge : 0,
                                                         SubscriptionType = setting.SubscriptionType,
                                                         EndDate = setting.EndDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                                                         DOC = setting.DOC.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                                                         StartDate = setting.DOC.Date,
                                                         OriginalCharge = Util.isRoleAdminOrSupport(UserTypeID) ? setting.OriginalCharge : 0,
                                                         ServiceSubscriptionPlanMasterID = setting.ServiceSubscriptionPlanMasterID,
                                                         PlanName = leftPlans?.PlanName ?? "",
                                                     }
                                                    ).GroupBy(vm => vm.ServiceAndUserwiseSubscriptionID)
                                                    .Select(group => group.First())
                                                    .ToList();

            if (Util.isRoleSuperDistOrDist(UserTypeID))
            {
                if (Request.UserID > 0)
                {
                    listOfUserServiceWise = (from item in listOfUserServiceWise
                                             where item.ParentID == Request.UserID
                                             || item.UserID == Request.UserID
                                             select item).ToList();
                }
                else
                {

                    listOfUserServiceWise = (from item in listOfUserServiceWise
                                             where item.ParentID == UserID
                                             || item.UserID == UserID
                                             select item).ToList();
                }
                return listOfUserServiceWise;
            }

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                if (Request.UserID > 0)
                {
                    listOfUserServiceWise = (from item in listOfUserServiceWise
                                             where item.ParentID == Request.UserID
                                             || item.UserID == Request.UserID
                                             select item).ToList();
                }
                return listOfUserServiceWise;

            }

            else
            {
                listOfUserServiceWise = (from item in listOfUserServiceWise
                                         where item.UserID == UserID
                                         select item).ToList();
            }

            return listOfUserServiceWise;
        }

        internal ViewModel SelectForPaymentByID(int UserID, int ServiceAndUserwiseSubscriptionID)
        {
            List<ViewModel> list = SelectUserWiseSubscriptionCharge(UserID);

            return list.Where(U => U.ServiceAndUserwiseSubscriptionID == ServiceAndUserwiseSubscriptionID).FirstOrDefault();
        }


        internal List<ViewModel> SelectSubscriptionPaymentDueUser()
        {
            int datebefore27days = Convert.ToInt32(DateTime.Now.AddDays(-27).ToString(Messages.DOCIntegerFomrat));

            List<ServiceAndUserwiseSubscription> settinglist = new ServiceAndUserwiseSubscriptionDal().Select();
            List<UserServiceSubscriptionDetails> userServiceChargeList = Select();

            List<Response.Service.ViewModel> servicelist = new ServiceMasterDal().SelectSubscriptionAllowedServices();

            DateTime today = DateTime.Now.Date;

            List<ViewModel> listOfUserServiceWise = (from setting in settinglist
                                                     join user in userServiceChargeList on setting.ServiceAndUserwiseSubscriptionID equals user.ServiceAndUserwiseSubscriptionID

                                                     join service in servicelist on setting.ServiceID equals service.ServiceID into SDetails
                                                     from leftService in SDetails.DefaultIfEmpty()
                                                     where                                                          
                                                            setting.DOCInt <= datebefore27days
                                                     orderby setting.ServiceAndUserwiseSubscriptionID descending
                                                     select new ViewModel
                                                     {
                                                         ServiceAndUserwiseSubscriptionID = setting.ServiceAndUserwiseSubscriptionID,
                                                         UserID = user.UserID,
                                                         ServiceName = leftService?.ServiceName ?? StatusType.ALL,
                                                         IsAutoDebit = setting.IsAutoDebit,
                                                         LoginID = setting.LoginID,
                                                         Charge = user.Charge,
                                                         SubscriptionType = setting.SubscriptionType,
                                                         EndDate = setting.EndDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                                                         DOC = setting.DOC.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                                                         StartDate = setting.DOC.Date,
                                                     }
                                                    ).Where(subscription =>
                                                            (
                                                                (subscription.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Monthly) &&
                                                                 (((today - subscription.StartDate).TotalDays % (int)SubscriptionDays.Monthly) > (int)SubscriptionDays.MonthlyDue)) ||

                                                                (subscription.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Quarterly) &&
                                                                 (((today - subscription.StartDate).TotalDays % (int)SubscriptionDays.Quarterly) > (int)SubscriptionDays.QuarterlyDue)) ||

                                                                (subscription.SubscriptionType.EqualsIgnoreCase(SubscriptionTypes.Yearly) &&
                                                               (((today - subscription.StartDate).TotalDays % (int)SubscriptionDays.Yearly) > (int)SubscriptionDays.YearlyDue))

                                                            )
                                                        ).GroupBy(vm => vm.ServiceAndUserwiseSubscriptionID)
                                                    .Select(group => group.First())
                                                    .ToList();
            return listOfUserServiceWise;
        }

        internal ViewModel SelectUserAndServiceSubscriptionCharge(int UserID, int ServiceID)
        {

            List<ServiceAndUserwiseSubscription> settinglist = new ServiceAndUserwiseSubscriptionDal().Select();
            List<UserServiceSubscriptionDetails> userServiceChargeList = Select();

            ViewModel listOfUserServiceWise = (from setting in settinglist
                                               join user in userServiceChargeList on setting.ServiceAndUserwiseSubscriptionID equals user.ServiceAndUserwiseSubscriptionID

                                               where                                                   
                                                     setting.UserID == UserID &&
                                                     user.ServiceID == ServiceID
                                               orderby setting.ServiceAndUserwiseSubscriptionID descending
                                               select new ViewModel
                                               {
                                                   ServiceAndUserwiseSubscriptionID = setting.ServiceAndUserwiseSubscriptionID,
                                                   UserID = user.UserID,
                                                   IsAutoDebit = setting.IsAutoDebit,
                                                   LoginID = setting.LoginID,
                                                   Charge = user.Charge,
                                                   SubscriptionType = setting.SubscriptionType,
                                                   DOC = setting.DOC.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                                                   StartDate = setting.DOC.Date,
                                               }
                                                    ).GroupBy(vm => vm.ServiceAndUserwiseSubscriptionID)
                                                    .Select(group => group.First())
                                                    .FirstOrDefault();
            return listOfUserServiceWise;
        }

    }
}
