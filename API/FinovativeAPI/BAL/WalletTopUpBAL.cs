using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.Transaction;
using Project.Response.WalletTopUp;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class WalletTopUpBAL
    {
        internal WalletTopUpTransactionResponse StatusCheck(SelectByIDRequest mainRequest, int UserID, out string failureMessage)
        {
            WalletTopUpTransactionResponse response = new WalletTopUpTransactionResponse();
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
                response.Message = transaction.Description;
                return response;
            }

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.APIUser)
            {
                Response.TransactionMaster.StatusCheckResponse resp = new TransactionDAL().SelectByAPIUserRequestIDAndUserID(mainRequest.ID.ToString(), UserID);
                if (resp == null)
                {
                    response.Status = StatusType.Process;
                    response.Message = "No transaction found";
                    return response;
                }
                mainRequest.ID = resp.TransactionMasterID;
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(transaction.UserID);

            if (user == null)
                return response;

            Response.CashDepositSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new CashDepositSettingDal().SelectActiveCashDeposit();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
            {
                failureMessage = AEPSMessage.NotActive;
                response.Message = failureMessage;
                return response;
            }

            if (transaction.ProviderID == Company.PaysprintProviderID)
            {

            }
            else
            {
            }
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}