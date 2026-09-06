using Project.Model;
using Project.Response.TransactionProviderHistory;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class TransactionProviderHistoryDal
    {
        internal int Insert(TransactionProviderHistory transactionProviderHistory)
        {
            using (var context = new DatabaseContext())
            {
                context.TransactionProviderHistory.Add(transactionProviderHistory);
                return context.SaveChanges();
            }
        }

        internal TransactionHistoryResponse SelectByTransactionMasterIDAndProviderID(int transactionMasterID, int providerID)
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                return (from transactionHistoryResponse in context.TransactionProviderHistory
                        where transactionHistoryResponse.TransactionMasterID == transactionMasterID
                        && transactionHistoryResponse.ProviderID == providerID
                        select new TransactionHistoryResponse
                        {
                            TransactionMasterID = transactionHistoryResponse.TransactionMasterID,
                            ProviderID = transactionHistoryResponse.ProviderID,
                        }).FirstOrDefault();
            }
        }

        internal List<ProviderWithCount> SelectTopNByOperatorID(int OperatorID, int Count)
        {
            List<TransactionProviderHistory> list;
            int CurrentDoc = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                list = (from transactionHistoryResponse in context.TransactionProviderHistory
                        where transactionHistoryResponse.DOCInt == CurrentDoc && 
                        transactionHistoryResponse.OperatorID == OperatorID
                        orderby transactionHistoryResponse.RechargeProviderHistoryID descending
                        select new TransactionProviderHistory
                        {
                            TransactionMasterID = transactionHistoryResponse.TransactionMasterID,
                            ProviderID = transactionHistoryResponse.ProviderID,
                        }).Take(Count).ToList();
            }

            List<ProviderWithCount> countList;
            countList = (from txn in list
                         group txn by new { txn.ProviderID } into grp
                         select new ProviderWithCount
                         {
                             ProviderID = grp.Key.ProviderID,
                             Count = grp.Count(),
                         }).ToList();

            return countList;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.TransactionProviderHistory.Where(U => U.DOCInt <= DeleteDOCInt);
                context.TransactionProviderHistory.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}