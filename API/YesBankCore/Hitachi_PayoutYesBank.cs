using YesBankCore.Model;

namespace YesBankCore
{
    public class Hitachi_PayoutYesBank
    {
        public void SetCredential(string PartnerKey, string PrivateKey, string BankPublicKey)
        {
            KYCCredentials.PartnerKey = PartnerKey.Trim();
            KYCCredentials.PrivateKey = PrivateKey.Trim();
            KYCCredentials.BankPublicKey = BankPublicKey.Trim();
        }
    
        public Response.Payout.PayoutResponse MoneyTransfer(Request.Payout.FundTransferRequest request)
        {
            Response.Payout.PayoutResponse response = new Response.Payout.PayoutResponse();

            response = new Bal.PayoutBal().MoneyTransfer(request);
            return response;
        }

        public Response.Payout.PayoutResponse StatusCheck(Request.Payout.PayoutStatusCheckRequest request)
        {
            Response.Payout.PayoutResponse response = new Response.Payout.PayoutResponse();
       
            response = new Bal.PayoutBal().StatusCheck(request);
            return response;
        }

        public Response.Payout.PayoutResponse Verification(Request.Payout.VerificationRequest request)
        {
            Response.Payout.PayoutResponse response = new Response.Payout.PayoutResponse();

            response = new Bal.PayoutBal().Verification(request);
            return response;
        }

        public Response.Payout.BalanceFetchResponse FetchBalance(Request.Payout.FetchBalanceRequest request)
        {
            Response.Payout.BalanceFetchResponse response = new Response.Payout.BalanceFetchResponse();
            
            response = new Bal.PayoutBal().FetchBalance(request);
            return response;
        }
    }
}
