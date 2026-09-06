using Project.Model;
using Project.Processing;
using Project.Requests.CashCollection;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CashCollectionMasterDal
    {
        internal int Insert(CashCollectionMaster ObjModel)
        {
            int result = -1;
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.CashCollectionMaster.Add(ObjModel);
                    context.SaveChanges();
                    result = ObjModel.CashCollectionMasterID;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal int Update(CashCollectionMaster ObjModel)
        {
            int result = -1;
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.CashCollectionMaster.Update(ObjModel);
                    context.SaveChanges();
                    result = ObjModel.CashCollectionMasterID;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal CashCollectionMaster SelectByUserID(int UserID)
        {
            using (var context = new DatabaseContext())
            {
                return context.CashCollectionMaster.FirstOrDefault(C => C.UserID == UserID);
            }
        }

        internal List<CashCollectionMaster> Select()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.CashCollectionMaster.ToList();
            }
        }

        internal float SelectOutstandingByUserID(int userID)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            float outstanding = (float)context.CashCollectionMaster
                .Where(c => c.UserID == userID)
                .Select(c => c.Outstanding)
                .FirstOrDefault();

            return (float)System.Math.Round(outstanding, 2);
        }

        internal float SelectTotalOutstandingByUserIDs(List<int> userIDs)
        {
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

            float totalOutstanding = (float)context.CashCollectionMaster
                .Where(c => userIDs.Contains(c.UserID))
                .Select(c => c.Outstanding)
                .DefaultIfEmpty()
                .Sum();

            return (float)System.Math.Round(totalOutstanding, 2);
        }

        internal float SelectOutstandingForUIByUserID(int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            float Outstanding =
            (UserTypeID == (int)UserType.Retailer ||
               UserTypeID == (int)UserType.APIUser ) ?
                SelectOutstandingByUserID(UserID) :
              SelectOutstandingForDownLine(UserID);

            return Outstanding;
        }

        internal float SelectOutstanding(int UserID)
        {
            float Outstanding = SelectOutstandingByUserID(UserID);

            return Outstanding;
        }

        private float SelectOutstandingForDownLine(int UserID)
        {
            float outstanding = 0;
            List<Response.User.DownlineForDropDownResponse> users = new UserDAL().SelectDownlineForDropDown(UserID);
            List<int> userids = users.Select(u => u.UserID).ToList();
            outstanding = SelectTotalOutstandingByUserIDs(userids);
            return outstanding;
        }

        internal float SelectOutstandingForDeletedUser(int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeIDForDeletedUsers(UserID);

            float Outstanding =
           (UserTypeID == (int)UserType.Retailer ||
              UserTypeID == (int)UserType.APIUser) ?
               SelectOutstandingByUserID(UserID) :
             SelectOutstandingForDownLine(UserID);

            return Outstanding;
        }
        internal List<Project.Response.CashCollection.OutstandingViewModel> OutstandingReport(SelectByUserIDRequest mainRequest)
        {
            List<Project.Response.CashCollection.OutstandingViewModel> list = new List<Response.CashCollection.OutstandingViewModel>();
            using var context = new DatabaseContext();
            context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            list = (from cashCollection in context.CashCollectionMaster
                    where cashCollection.Outstanding != 0
                    orderby cashCollection.Outstanding descending
                    select new Project.Response.CashCollection.OutstandingViewModel { 
                        UserID =  cashCollection.UserID,
                        UserName = cashCollection.User.FirmName.ConcatNameAliasName(cashCollection.User.MobileNumber),
                        Outstanding = cashCollection.Outstanding,
                    }).ToList();
            if(mainRequest.UserID > 0)
            {
                list = list.Where(txn => txn.UserID == mainRequest.UserID).ToList();
            }
            return list;
        }
    }
}