using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Response.Beneficiary;
using Project.StaticModel;
using System;
using YesBankCore.Response.MoneyTransfer;

namespace Project.BAL.YesBankDMR
{
    public class DMTRefundBAL
    {
        public RefundResponse RefundSendOTP(Requests.Beneficiary.RefundSendOTPRequest mainRequest)
        {
            RefundResponse resp = new RefundResponse();

            string failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Failure;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            TransactionMaster model = null;
            model = new TransactionDAL().SelectByID(Convert.ToInt32(mainRequest.ID));
            if (model == null)
            {
                resp.Status = StatusType.Process;
                resp.Message = "No transaction found";
                return resp;
            }

            try
            {
                RefundTransferResponse refundres = new RefundTransferResponse();

                if (model.Amount == 1)
                {
                    refundres = new Processing.YesBank.MoneyTransferProcessing().VerifyBeneSendOTPRefund(model);
                }
                else
                {
                    refundres = new Processing.YesBank.MoneyTransferProcessing().TransferSendOTPRefund(model);
                }

                if (refundres == null)
                {
                    resp.Message = BeneficiaryMessage.InsertFailure;
                    return resp;
                }

                if (!refundres.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    resp.Message = refundres.Message;
                    resp.Status = refundres.Status;
                    return resp;
                }
                resp.IsOTP = true;
                resp.Message = BeneficiaryMessage.BeneOTP;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }
    }
}