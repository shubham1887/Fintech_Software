using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.Transaction;
using Project.Response.CashDeposit;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class CashDepositBAL
    {





        internal CashDepositTransactionResponse StatusCheck(SelectByIDRequest mainRequest, int userID, out string failureMessage)
        {
            CashDepositTransactionResponse response = new CashDepositTransactionResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            TransactionMaster transaction = null;
            try
            {
                transaction = new TransactionDAL().SelectByID(mainRequest.ID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (transaction == null)
                return response;

            if (!new ResponseProcess().checkProcess(transaction.Status))
            {
                response.Status = transaction.Status;
                response.Amount = transaction.Amount;
                response.APIUserRequestID = transaction.APIUserRequestID;
                response.ConsumerNumber = transaction.ConsumerNumber;
                response.TransactionID = transaction.ReferenceNo;
                response.Message = transaction.Description;

                return response;
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(transaction.UserID);

            if (user == null)
                return response;

            if (transaction.ProviderID == Company.PaysprintProviderID)
            {
                failureMessage = "Status check not allowed for this service";
                return response;
            }

            if (transaction.ProviderID == Company.FingpayProviderID)
            {
                //response = PayU_StatusCheck(transaction.ProviderRequestID, transaction, user);
            }
            else
            {
            }
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}