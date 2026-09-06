using Project.BAL;
using Project.Model;
using Project.Response.ProviderResponse;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;

namespace Project.Processing
{
    internal class ResendProcessing
    {
        internal string ResendRecharge(TransactionMaster objModel, bool IsStatusCheck)
        {
            string status = "";
            if (IsStatusCheck)
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
                TransactionResponse response = new TransactionResponse();

                ProviderReturnResponse providerReturnResponse = new ProviderProcessing().Hit(objModel,"", objModel.ProviderID);

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