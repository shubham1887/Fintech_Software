using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Response.TransactionMaster;
using Project.StaticModel;

namespace Project.Processing.VendorPayProcessing
{
    public class VendorPayUpdateProcessing
    {
        
        internal void UpdateYesBankStatus(YesBankCore.Response.PayoutResponse resp, int transactionMasterID, TransactionResponse retObj)
        {
            retObj.Status = StatusType.Process;
            retObj.Message = "Transaction is in process";

            if (resp == null)
            {
                return;
            }

            if (resp.Status == null)
            {
                return;
            }

            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(transactionMasterID);

            if (new ResponseProcess().CheckFailure(resp.Status))// Failure, it is outer status
            {
                if (!new ResponseProcess().checkProcess(transactionMaster.Status))
                {
                    return;
                }

                if (resp.Message.ToLower().Contains("Your Current Balance is low. Please, Contact to customer care".ToLower()))
                {
                    resp.Message = TransactionMessage.ProviderLowBalance;
                }

                new AccountCoreProcessing().RefundTransaction(transactionMaster, resp.Message, DOCType.VendorPay);

                retObj.Status = StatusType.Failure;
                retObj.Message = resp.Message;

                return;
            }

            transactionMaster.DMT_StatusCode = resp.Status;
            transactionMaster.Description = resp.Message;

            //  retObj.BeneficiaryName = resp.BeneficiaryName;
            retObj.TransactionID = resp.BankRRN;
            transactionMaster.ProviderDOCNo = resp.RequestId;

            if (new ResponseProcess().CheckSuccess(resp.Status))// success
            {
                transactionMaster.Status = StatusType.Success;
                transactionMaster.ReferenceNo = resp.BankRRN;

                new AccountCoreProcessing().SuccessTransaction(transactionMaster);

                retObj.Status = StatusType.Success;
                retObj.Message = resp.Message;
                retObj.TransactionID = resp.BankRRN;
                return;
            }

            if (!new ResponseProcess().checkProcess(transactionMaster.Status))
            {
                return;
            }

            if (new ResponseProcess().CheckFailure(resp.Status))// Failure,  it is inner status
            {
                transactionMaster.Status = StatusType.Failure;
                new AccountCoreProcessing().RefundTransaction(transactionMaster, resp.Message, DOCType.VendorPay);

                retObj.Status = StatusType.Failure;
                retObj.Message = resp.Message;
                if (retObj.Message.ToLower().Contains("Your Current Balance is low. Please, Contact to customer care".ToLower()))
                {
                    retObj.Message = TransactionMessage.ProviderLowBalance;
                }

                return;
            }

            new TransactionDAL().Update(transactionMaster);
        }

    }
}