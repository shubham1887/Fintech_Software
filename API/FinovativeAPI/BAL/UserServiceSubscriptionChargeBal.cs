using Project.DAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.ServiceAndUserwiseSubscription;
using Project.Response.ServiceAndUserwiseSubscription;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class UserServiceSubscriptionChargeBal
    {
        internal ReportWrapper SelectSubscriptionPaymentReport(SelectRequest selectRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            List<ViewModel> list = new List<ViewModel>();

            try
            {
                list = new UserServiceSubscriptionChargeDal().SelectSubscriptionPaymentReport(keyDataModel.UserID, selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            if (list == null || list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            HeaderViewModel HeaderSummary = new HeaderViewModel();
            HeaderSummary.TotalCount = list.Count;
            HeaderSummary.TotalCharge = list.Sum(e => e.Charge);

            wrapper.HeaderSummary = HeaderSummary;
            wrapper.Count = list.Count;

            list = list.AsQueryable().WhereDynamic(selectRequest.Search).ToList();

            list = list.AsQueryable().OrderByDynamic(selectRequest.SortColumnName, selectRequest.SortDirection).ToList();

            list = list.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();

            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }


    }
}
