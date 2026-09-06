using Project.Model;
using Project.Processing;
using Project.Response.ServiceAndUserwiseSubscription;
using Project.Requests.ServiceAndUserwiseSubscription;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Project.DAL
{
    public class UserServiceSubscriptionChargeDal
    {
        private readonly string key = "_UserServiceSubscriptionCharge";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserServiceSubscriptionCharge>>();
            cache.Remove(key);
        }

        public List<UserServiceSubscriptionCharge> Select()
        {
            var _List = new MemoryCacheProcessing<List<UserServiceSubscriptionCharge>>();
            List<UserServiceSubscriptionCharge> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;

        }

        private List<UserServiceSubscriptionCharge> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.UserServiceSubscriptionCharge.ToList();
            }
        }

        public int Insert(UserServiceSubscriptionCharge ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UserServiceSubscriptionCharge.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.UserServiceSubscriptionChargeID;
            }
        }
        public int Update(UserServiceSubscriptionCharge ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.UserServiceSubscriptionCharge.Update(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.UserServiceSubscriptionChargeID;
            }
        }


        internal UserServiceSubscriptionCharge SelectByID(int UserServiceSubscriptionChargeID)
        {
            List<UserServiceSubscriptionCharge> list = Select();

            UserServiceSubscriptionCharge existingRecord = list.Where(S => S.UserServiceSubscriptionChargeID == UserServiceSubscriptionChargeID).FirstOrDefault();
            return existingRecord;
        }

        internal List<UserServiceSubscriptionCharge> CheckDuplicate(int UserID, int ServiceID)
        {

            List<UserServiceSubscriptionCharge> list = Select();
            list = list.Where(S => S.UserID == UserID && S.ServiceID == ServiceID).ToList();
            return list;

        }

        internal UserServiceSubscriptionCharge SelectByUserID(int UserID, int ServiceID, int ServiceAndUserwiseSubscriptionID)
        {
            List<UserServiceSubscriptionCharge> list = Select();

            return list.Where(S => S.UserID == UserID
            && S.ServiceAndUserwiseSubscriptionID == ServiceAndUserwiseSubscriptionID
            && S.ServiceID == ServiceID).LastOrDefault();
        }

        internal UserServiceSubscriptionCharge SelectLastPayment(int UserID, int ServiceAndUserwiseSubscriptionID)
        {
            List<UserServiceSubscriptionCharge> list = Select();

            return list.Where(S => S.UserID == UserID
            && S.ServiceAndUserwiseSubscriptionID == ServiceAndUserwiseSubscriptionID).LastOrDefault();
        }


        internal List<ViewModel> SelectSubscriptionPaymentReport(int UserID, SelectRequest selectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<UserServiceSubscriptionCharge> list = Select();

            List<ViewModel> chargelist = new ServiceAndUserwiseSubscriptionDal().SelectAll();
            List<UserMaster> userlist = new UserDAL().Select();

            List<ViewModel> result = new();

            result = (from setting in list
                      join user in userlist on setting.UserID equals user.UserID
                      join charge in chargelist on setting.ServiceAndUserwiseSubscriptionID equals charge.ServiceAndUserwiseSubscriptionID
                      where setting.DOCInt >= FromDate
                             && setting.DOCInt <= ToDate
                             && (selectRequest.ServiceID != 0 ? setting.ServiceID == selectRequest.ServiceID : 2 > 1)
                              && (!selectRequest.SubscriptionType.EqualsIgnoreCase(StatusType.ALL) ? charge.SubscriptionType == selectRequest.SubscriptionType : 2 > 1)

                      orderby setting.ServiceAndUserwiseSubscriptionID descending

                      select new ViewModel
                      {
                          IsAdminApproved = setting.IsAdminApproved,
                          UserServiceSubscriptionChargeID = setting.UserServiceSubscriptionChargeID,
                          ServiceAndUserwiseSubscriptionID = setting.ServiceAndUserwiseSubscriptionID,
                          UserName = charge.UserName,
                          UserID = setting.UserID,
                          ParentID = user.ParentID,
                          ServiceID = setting.ServiceID,
                          ServiceName = charge.ServiceName,
                          Charge = setting.Charge,
                          LoginID = charge.LoginID,
                          IsAutoDebit = charge.IsAutoDebit,
                          SubscriptionType = charge.SubscriptionType,
                          Remark = charge.Remark,
                          EndDate = charge.EndDate,
                          DOC = charge.DOC,
                          PaymentDate = setting.DOC.LongFormatDate(),
                          Status = charge.Status

                      }).ToList();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserID > Company.AdminUserID && selectRequest.UserID <= 0)
            {
                return result = (from item in result
                                 where item.UserID == UserID
                                 select item).ToList();
            }

            if (Util.isRoleAdminOrSDORDist(UserTypeID) && selectRequest.UserID > 0)
            {
                return result = (from item in result
                                 where item.UserID == selectRequest.UserID
                                 select item).ToList();

            }

            if (Util.isRoleAdminOrSupport(UserTypeID) && selectRequest.ParentID > 0)
            {
                return result = (from item in result
                                 where item.ParentID == selectRequest.ParentID
                                 select item).ToList();

            }

            return result;
        }
    }
}
