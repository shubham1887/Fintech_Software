using Project.DAL;
using Project.Processing;
using Project.Requests.CommissionHold;
using Project.Response.CommissionHold;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CommissionHoldBal
    {
        internal List<ViewModel> SelectCommissionHold(SearchRequest selectRequest, int UserID, out string failureMessage)
        {
            List<ViewModel> response = new List<ViewModel>();
            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            try
            {
                response = new CommissionHoldDal().SelectCommissionHold(selectRequest, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }
    }
}