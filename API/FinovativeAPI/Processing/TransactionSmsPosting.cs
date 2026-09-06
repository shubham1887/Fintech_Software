using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Project.Processing
{
    public class TransactionSmsPosting
    {
        private static object lockObj = new object();

        public void select()
        {
            if (Monitor.TryEnter(lockObj, 0))
            {
                try
                {
                    SendSms();
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
        }

        private void SendSms()
        {
            try
            {
                List<SmsTransactionMaster> listResponse = new SmsTransactionMasterBal().SelectPending();

                if (listResponse == null || listResponse.Count <= 0)
                {
                    return;
                }
                foreach (SmsTransactionMaster item in listResponse)
                {
                    if (IsAllSuccess(item.BillNumber))
                    {
                        SendSuccessMessageForBillNumber(item.BillNumber);
                        continue;
                    }
                    if (IsSuccess(item.TransactionMasterID))
                    {
                        SendMessageForTransactionMasterID(item.TransactionMasterID, item.SmsTransactionMasterID);
                        continue;
                    }
                    if (IsProcess(item.TransactionMasterID))
                    {
                        SendMessageForTransactionMasterID(item.TransactionMasterID, item.SmsTransactionMasterID);
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void SendSuccessMessageForBillNumber(string billNumber)
        {
            TransactionMaster transactionMaster = new TransactionDAL().SelectTransactionByBillNumber(billNumber);
            if (transactionMaster == null)
                return;
            new MessageSelection().SendDMTTransactionSucessAllOneMessage(transactionMaster);
            new SmsTransactionMasterDal().UpdateAll(billNumber);
        }

        private void SendMessageForTransactionMasterID(int TransactionMasterID, int SmsTransactionMasterID)
        {
            TransactionMaster transactionMaster = new TransactionDAL().SelectTransactionByTransactionMasterID(TransactionMasterID);
            if (transactionMaster == null)
                return;
            new MessageSelection().SendDMTTransactionMessage(transactionMaster);

            bool IsProcess = false;
            bool IsSuccess = false;
            if (transactionMaster.Status.EqualsIgnoreCase(StatusType.Success))
                IsSuccess = true;

            if (transactionMaster.Status.EqualsIgnoreCase(StatusType.Process))
                IsProcess = true;
            new SmsTransactionMasterBal().Update(SmsTransactionMasterID, IsProcess, IsSuccess);
        }

        private bool IsAllSuccess(string billNumber)
        {
            bool IsAllSucessMsg = new SmsTransactionMasterDal().IsSuccessAllSelectByBillNumber(billNumber);
            if (!IsAllSucessMsg)
                return false;

            bool IsSucessTransaction = new TransactionDAL().IsSuccessAllSelectByBillNumber(billNumber);
            if (!IsSucessTransaction)
                return false;
            return true;
        }

        private bool IsSuccess(int TransactionMasterID)
        {
            bool IsSuccessMsg = new SmsTransactionMasterDal().IsSuccessSelectByTransactionMasterID(TransactionMasterID);
            if (!IsSuccessMsg)
                return false;

            bool IsSucessTransaction = new TransactionDAL().IsSuccessSelectByTransactionMasterID(TransactionMasterID);
            if (!IsSucessTransaction)
                return false;
            return true;
        }

        private bool IsProcess(int TransactionMasterID)
        {
            bool IsProcessMsg = new SmsTransactionMasterDal().IsProcessSelectByTransactionMasterID(TransactionMasterID);
            if (!IsProcessMsg)
                return false;

            bool IsProcessTransaction = new TransactionDAL().IsProcessSelectByTransactionMasterID(TransactionMasterID);
            if (!IsProcessTransaction)
                return false;
            return true;
        }
    }
}