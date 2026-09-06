using Project.Model;
using Project.Processing;
using Project.Requests.UserActionHistory;
using Project.Response.UserActionHistory;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class UserActionHistoryDal
    {
        internal int Insert(UserActionHistory objModel)
        {
            using var context = new DatabaseContext();
            context.UserActionHistory.Add(objModel);
            context.SaveChanges();
            return objModel.UserHistoryID;
        }

        internal List<ViewModel> SelectByUserID(SelectByUserIDRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));

            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            using var context = new DatabaseContext();
            return (from history in context.UserActionHistory
                    where history.UserID == selectRequest.UserID
                    && history.DOCInt >= From
                    && history.DOCInt <= To
                    orderby history.UserHistoryID descending
                    select new ViewModel
                    {
                        UserID = history.UserID,
                        FirmName = history.UserMaster.FirmName.ConcatNameMobileAliasName(history.UserMaster.MobileNumber, history.UserMaster.AliasName),
                        Action = history.Action,
                        Description = history.Description,
                        DOC = history.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        Status = history.Status,
                    }).ToList();
        }
    }
}