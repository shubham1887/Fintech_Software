using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Response.TransactionProviderHistory;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class TransactionProviderHistoryBal
    {
        internal string Insert(TransactionMaster transactionMaster, int ProviderID, int OperatorID)
        {
            try
            {
                TransactionProviderHistory transactionProviderHistory = new TransactionProviderHistory
                {
                    TransactionMasterID = transactionMaster.TransactionMasterID,
                    ProviderID = ProviderID,
                    OperatorID = OperatorID,
                    ProviderRequestID = transactionMaster.ProviderRequestID,
                };

                int Result = new TransactionProviderHistoryDal().Insert(transactionProviderHistory);

                if (Result <= 0)
                    return TransactionHistoryMessage.InsertFailure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal TransactionHistoryResponse Select(int TransactionMasterID, int ProviderID)
        {
            TransactionHistoryResponse View = null;

            try
            {
                View = new TransactionProviderHistoryDal().SelectByTransactionMasterIDAndProviderID(TransactionMasterID, ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return View;
        }

        internal List<ProviderWithCount> SelectTopNByOperatorID(int OperatorID, int Count)
        {
            List<ProviderWithCount> list = null;
            try
            {
                list = new TransactionProviderHistoryDal().SelectTopNByOperatorID(OperatorID, Count);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }
    }
}