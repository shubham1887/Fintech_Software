using Project.DAL;
using Project.Processing;
using Project.Requests.LoginHistory;
using Project.Response.LoginHistory;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class LoginHistoryBal
    {
        internal List<LoginHistoryViewModel> SelectHistory(SelectRequest selectRequest, int UserID, out string failureMessage)
        {
            failureMessage = Messages.DataNotFound;
            List<LoginHistoryViewModel> report = new List<LoginHistoryViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return report;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            try
            {
                report = new LoginHistoryDal().SelectHistory(selectRequest);
                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return report;
                }

                if (UserTypeID != (int)UserType.Admin)
                {
                    report = (from item in report
                              where item.UserID == UserID
                              select item).ToList();
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return report;
        }
    }
}