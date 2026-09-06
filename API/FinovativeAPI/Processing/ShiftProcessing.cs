using Project.BAL;
using Project.Model;
using Project.Requests.Transaction;
using Project.Response.ProviderResponse;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;

namespace Project.Processing
{
    public class ShiftProcessing
    {
        internal string Shift(TransactionMaster objModel, ShiftRequest mainRequest)
        {
            string status = "";
            if (mainRequest.IsStatusCheck)
            {
                string failureMessage = new StatusCheckProcessing().StatusCheckHit(objModel, needStatusInReturn: true, out status);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;

                if (new ResponseProcess().CheckSuccess(status))
                {
                    return "Transaction is success";
                }
            }

            try
            {
                ProviderReturnResponse providerReturnResponse = new ProviderProcessing().Hit(objModel,"",mainRequest.ProviderID);
                TransactionResponse response = new TransactionResponse();

                new TransactionBal().UpdateRechargeStatus(providerReturnResponse, objModel.TransactionMasterID, response);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }
    }
}