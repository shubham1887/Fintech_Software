using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.StaticModel;

namespace Project.BAL
{
    public class InboxBAL
    {
        private bool isValidCheckForDuplicate(int userID, int ServiceID, int OperatorID, decimal Amount)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (UserTypeID == (int)UserType.APIUser && ServiceID == (int)ServiceCode.Mobile)
            {
                return false;
            }

            if (OperatorID == (int)OperatorCode.Balance_AEPS)
            {
                return false;
            }

            if (OperatorID == (int)OperatorCode.MiniStatement_AEPS)
            {
                return false;
            }

            if (OperatorID == (int)OperatorCode.Verify_DMT)
            {
                return false;
            }

            if (UserTypeID == (int)UserType.APIUser && ServiceID == (int)ServiceCode.MoneyTransfer)
            {
                return false;
            }

            if (OperatorID == (int)OperatorCode.Balance_MicroATM)
            {
                return false;
            }

            if (OperatorID == (int)OperatorCode.MiniStatement_MicroATM)
            {
                return false;
            }

            if (Amount == 0)
            {
                return false;
            }

            return true;
        }

        private bool isCheckInTransactionMaster(int ServiceID, int OperatorID)
        {

            if (OperatorID == (int)OperatorCode.Withdraw_AEPS)
            {
                return true;
            }

            if (OperatorID == (int)OperatorCode.AadharPay_AEPS)
            {
                return true;
            }

            if (OperatorID == (int)OperatorCode.Withdraw_MicroATM)
            {
                return true;
            }
            if (ServiceID == (int)ServiceCode.CMS)
            {
                return true;
            }

            return false;
        }

        internal int Insert(int userID, int ServiceID, int OperatorID, decimal Amount, string ConsumerNumber, string Through, string process, string iP, out string failureMessage)
        {
            Inbox inbox = new Inbox
            {
                Amount = Amount,
                ConsumerNumber = ConsumerNumber,
                OperatorID = OperatorID,
                ServiceID = ServiceID,
                Through = Through,
                UserID = userID,
                Status = process,
                IPAddress = iP,
                Param1 = ""
            };

            int InboxID = 0;
            using (ConcurrencyControl.AcquireLock(userID, userID.ToString()))
            {
                if (isValidCheckForDuplicate(userID, ServiceID, OperatorID, Amount))
                {
                    //----  check in inbox  for last 1 min (OperatorID    UserID    Amount    ConsumerNumber)
                    bool IsDuplicate = new InboxDAL().IsDuplicateInbox(inbox);

                    if (IsDuplicate)
                    {
                        bool chkInTxnmaster = isCheckInTransactionMaster(ServiceID, OperatorID);

                        if (chkInTxnmaster == false)
                        {
                            failureMessage = Message.TransactionMessage.DuplicateInbox;
                            return 0;
                        }
                    }
                }

                InboxID = new InboxDAL().Insert(inbox);
                if (InboxID <= 0)
                {
                    failureMessage = Messages.ExceptionOccured;
                    return InboxID;
                }
            }
            failureMessage = StatusType.Ok;
            return InboxID;
        }
    }
}