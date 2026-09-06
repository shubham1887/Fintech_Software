using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class TransactionMasterBAL
    {
        internal bool isValidCheckForDuplicate(TransactionMaster transactionMaster)
        {
            if (transactionMaster.ServiceID == (int)ServiceCode.Mobile)
            {
                return false;
            }

            if (transactionMaster.ServiceID == (int)ServiceCode.Dth)
            {
                return false;
            }

            if (transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer)
            {
                return false;
            }

            if (transactionMaster.ServiceID == (int)ServiceCode.BusBooking)
            {
                return false;
            }

            if (transactionMaster.OperatorID == (int)OperatorCode.Balance_AEPS)
            {
                return false;
            }
            if (transactionMaster.OperatorID == (int)OperatorCode.MiniStatement_AEPS)
            {
                return false;
            }

            if (transactionMaster.OperatorID == (int)OperatorCode.Balance_MicroATM)
            {
                return false;
            }
            if (transactionMaster.OperatorID == (int)OperatorCode.MiniStatement_MicroATM)
            {
                return false;
            }
            if (transactionMaster.Amount == 0)
            {
                return false;
            }

            return true;
        }

        public int InsertNonDMTCore(TransactionMaster transactionMaster, out string failure)
        {
            failure = "Exception occurred";

            using (ConcurrencyControl.AcquireLock(transactionMaster.UserID, transactionMaster.OperatorID.ToString()))
            {
                if (isValidCheckForDuplicate(transactionMaster))
                {
                    int RepeatRechargeInMinute = new OperatorMasterDAL().SelectRepeatRechargeInMinuteByOperatorID(transactionMaster.OperatorID);
                    if (RepeatRechargeInMinute <= 0)
                    {
                        RepeatRechargeInMinute = 1;
                    }

                    DateTime currentDateTime = DateTime.Now;
                    currentDateTime = currentDateTime.AddMinutes(-1 * RepeatRechargeInMinute);

                    bool IsTransactionDuplicate = new TransactionDAL().CheckDuplicateTransaction(transactionMaster.ServiceID, transactionMaster.OperatorID,
                        transactionMaster.ConsumerNumber, transactionMaster.Amount, currentDateTime, transactionMaster.UserID);

                    if (IsTransactionDuplicate)
                    {
                        failure = $"Duplicate transaction, please try after {RepeatRechargeInMinute} minutes ";
                        return 0;
                    }
                }

                int transactionMasterID = 0;
                try
                {
                    transactionMasterID = new TransactionDAL().Insert(transactionMaster);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (transactionMasterID <= 0)
                {
                    failure = TransactionMessage.Failure;
                    return 0;
                }

                failure = StatusType.Ok;
                return transactionMasterID;
            }
        }

        public int InsertDMTCore(TransactionMaster transactionMaster, out string failure)
        {
            failure = "Exception occurred";

            using (ConcurrencyControl.AcquireLock(transactionMaster.UserID, transactionMaster.OperatorID.ToString()))
            {
                int transactionMasterID = new TransactionDAL().Insert(transactionMaster);

                if (transactionMasterID <= 0)
                {
                    failure = TransactionMessage.Failure;
                    return 0;
                }
                new SmsTransactionMasterBal().Insert(transactionMaster.ServiceID, transactionMasterID, transactionMaster.Amount, transactionMaster.DMT_BillNumber);

                failure = StatusType.Ok;
                return transactionMasterID;
            }
        }
    }
}