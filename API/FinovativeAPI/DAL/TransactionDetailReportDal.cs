using Project.Model;
using Project.Processing;
using Project.Requests.Transaction;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Project.DAL
{
    public class TransactionDetailReportDal
    {
        internal List<Response.TransactionMaster.ViewModel> SelectForFilterByOperatorID(SelectByOperatorRequest txReportRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(txReportRequest.ToDate).ToString(Messages.DOCIntegerFomrat));


            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();


            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from transactionMaster in context.TransactionMaster
                        where transactionMaster.DOCInt >= FromDate
                        && transactionMaster.DOCInt <= ToDate
                        && transactionMaster.OperatorID == txReportRequest.OperatorID
                        && transactionMaster.Status == StatusType.Success
                        group transactionMaster by new
                        {
                            transactionMaster.UserMaster.FirmName,
                            transactionMaster.OperatorMaster.OperatorName,
                            transactionMaster.UserID,
                            transactionMaster.OperatorID
                        } into grouped
                        orderby grouped.Key.OperatorName ascending, grouped.Key.UserID ascending
                        select new Response.TransactionMaster.ViewModel
                        {
                            FirmName = grouped.Select(t => t.UserMaster.FirmName.ConcatNameMobileAliasName(t.UserMaster.MobileNumber, t.UserMaster.AliasName)).FirstOrDefault(),
                            Amount = grouped.Sum(t => t.Amount),
                            TransactionMasterID = grouped.Select(t => t.TransactionMasterID).FirstOrDefault(),
                            OperatorName = grouped.Key.OperatorName,
                            UserID = grouped.Key.UserID,
                            DOC = grouped.Select(t => t.DOC).FirstOrDefault().LongFormatDate(),
                            OperatorID = grouped.Key.OperatorID,
                            Status = grouped.Select(t => t.Status).FirstOrDefault(),
                            SuccessCount = grouped.Select(txn => txn.Amount).Count(),
                            RetailerCommission = grouped.Sum(t => t.RetailerCommission),
                            RetailerCommPercentage = (grouped.Sum(t => t.Amount) != 0 ? ((decimal)grouped.Sum(t => t.RetailerCommission) / grouped.Sum(t => t.Amount) * 100).ToString("0.00") : "0") + "%",

                        }).ToList();
            }
            return list;

        }


        internal List<TransactionMaster> SelectTodayFingpayThreeWayTransaction()
        {
            int today = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            List<TransactionMaster> list = new List<TransactionMaster>();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = context.TransactionMaster.Where(c => c.DOCInt == today &&
                    (c.OperatorID == (int)OperatorCode.Withdraw_AEPS || c.OperatorID == (int)OperatorCode.Withdraw_MicroATM || c.OperatorID == (int)OperatorCode.AadharPay_AEPS) &&
                     c.ProviderID == (int)Company.FingpayProviderID).ToList();
            }
            return list;
        }
    }
}
