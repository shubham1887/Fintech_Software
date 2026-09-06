using Project.Model;
using Project.Processing;
using Project.Requests.LoginHistory;
using Project.Response.LoginHistory;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class LoginHistoryDal
    {
        internal int Insert(LoginHistrory history)
        {
            int Result = -1;
            using (var context = new DatabaseContext())
            {
                context.LoginHistrory.Add(history);
                context.SaveChanges();
                Result = history.LoginHistoryID;
            }
            return Result;
        }

        internal List<LoginHistoryViewModel> SelectHistory(SelectRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<LoginHistoryViewModel> list = new List<LoginHistoryViewModel>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from txn in context.LoginHistrory
                        where txn.DOCInt >= From && txn.DOCInt <= To
                        join user in context.UserMaster on txn.UserID equals user.UserID
                        join usertype in context.UserTypeMaster on user.UserTypeID equals usertype.UserTypeID
                        orderby txn.DOC descending
                        select new LoginHistoryViewModel
                        {
                            User = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            UserType = usertype.UserType,
                            Through = txn.Through,
                            UserID = txn.UserID,
                            IPAddress = txn.IPAddress,
                            LoginMessage = txn.LoginMessage,
                            Date = txn.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        }).ToList();
                if (selectRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == selectRequest.UserID
                            select item).ToList();
                }

                return list;
            }
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.LoginHistrory.Where(U => U.DOCInt <= DeleteDOCInt);
                context.LoginHistrory.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}