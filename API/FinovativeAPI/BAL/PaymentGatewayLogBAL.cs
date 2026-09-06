using Project.DAL;
using Project.Processing;
using Project.Requests.PaymentGatewayLog;
using Project.Response.PaymentGatewayLog;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class PaymentGatewayLogBAL
    {
        internal PaymentGatewayWrapper SelectByFromDateTodate(SelectPaymentGatewayLogRequest selectRequest, int userID, out string failureMessage)
        {
            PaymentGatewayWrapper wrapper = new PaymentGatewayWrapper();

            List<PaymentGatewayLogViewModel> viewmodel = null;

            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return wrapper;
            }
            try
            {
                viewmodel = new PaymentGatewayLogDAL().SelectByFromDateTodate(selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            if (viewmodel == null || viewmodel.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }

            wrapper.Count = viewmodel.Count();
            viewmodel = viewmodel.AsQueryable().WhereDynamic(selectRequest.SearchText).ToList();

            if (!selectRequest.IsExcelDownload)
            {
                viewmodel = viewmodel.Skip(selectRequest.SkipCount).Take(selectRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = viewmodel;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<PaymentGatewayLogViewModel> SelectByOrderID(SelectByOrderIDRequest searchRequest, out string failureMessage)
        {
            List<PaymentGatewayLogViewModel> list = new List<PaymentGatewayLogViewModel>();
            failureMessage = searchRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return list;
            }

            try
            {
                list = new PaymentGatewayLogDAL().SelectByOrderID(searchRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return list;
            }

            if (list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return list;
            }

            failureMessage = StatusType.Ok;
            return list;
        }
    }
}