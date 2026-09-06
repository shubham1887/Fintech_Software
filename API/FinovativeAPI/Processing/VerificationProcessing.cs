using Project.BAL;
using Project.Message;
using Project.Model;
using Project.Requests.Transaction;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class VerificationProcessing
    {
        public DMTTransactionResponse HitToOnlyAccountVerification(MoneyTransferRequest mainRequest, TransactionMaster transactionMaster, out string failureMessage)
        {        
            DMTTransactionResponse response = new DMTTransactionResponse();
            try
            {
                 
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                response.Status = StatusType.Process;
                response.Message = TransactionMessage.TransactionInProcess;
                failureMessage = StatusType.Ok;
                return response;
            }

            if (new ResponseProcess().CheckSuccess(response.Status))
            {
                Task.Run(() => new TransactionBal().UpdateProviderID(mainRequest.ProviderID, transactionMaster.TransactionMasterID));
            }
            failureMessage = StatusType.Ok;
            return response;
        }

    }
}
