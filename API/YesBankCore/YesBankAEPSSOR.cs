using YesBankCore.AEPSBal;
using YesBankCore.Controller;
using YesBankCore.Model;
using YesBankCore.Request.AEPS;
using YesBankCore.Response.AEPS;

namespace YesBankCore
{
    public class YesBankAEPSSOR
    {
        public void SetCredential(string AgentID, string PartnerID, string PrivateKey, string PublicKey, string Mode)
        {
            AEPSCredentials.AGENTID = AgentID;
            AEPSCredentials.PARTNERID = PartnerID;
            AEPSCredentials.PRIVATEKEY = PrivateKey;
            AEPSCredentials.PUBLICKEY = PublicKey;
            AEPSCredentials.Mode = Mode;
        }

        public AuthenticationResponse Authentication(AuthenticationRequest input)
        {
            input.AgentCountry = "IN";
            input.TxnType = "10";
            input.CustomerConsent = "1";
            input.Latitude = "18.5747";
            input.Longitude = "73.7708";
            input.AppId = "1";
            input.PartnerId = AEPSCredentials.PARTNERID;
            input.AgentDistrict = "Pune";
            input.authType = "N";

            BAL bal = new BAL();
            AuthenticationResponse response = bal.Authentication(input);
            return response;
        }

        public CashWithdrawalResponse Withdraw(CashWithdrawalRequest input)
        {
            input.AgentCountry = "IN";
            input.Channel = "1";
            input.CustomerConsent = "1";
            input.AgentDistrict = "Pune";
            input.AppId = "1";
            input.authType = "N";
            input.PartnerId = AEPSCredentials.PARTNERID;
            BAL bal = new BAL();
            CashWithdrawalResponse response = bal.Withdraw(input);
            return response;
        }

        public CashWithdrawalResponse BalanceEnquiry(BalanceEnquiryRequest input)
        {
            input.AgentCountry = "IN";
            input.Channel = "1";
            input.CustomerConsent = "1";
            input.AgentDistrict = "Pune";
            input.AppId = "1";
            input.authType = "N";
            input.PartnerId = AEPSCredentials.PARTNERID;
            BAL bal = new BAL();
            CashWithdrawalResponse response = bal.BalanceEnquiry(input);
            return response;
        }

        public MiniStatementResponse MiniStatement(MiniStatementRequest input)
        {
            input.AgentCountry = "IN";
            input.Channel = "1";
            input.CustomerConsent = "1";
            input.AgentDistrict = "Pune";
            input.AppId = "1";
            input.authType = "N";
            input.PartnerId = AEPSCredentials.PARTNERID;
            BAL bal = new BAL();
            MiniStatementResponse response = bal.MiniStatement(input);
            return response;
        }

        public string isValidCapturedFingerPrint(string FingerPrint)
        {
            string res = new FingerPrintProcessing().isValidCapturedFingerPrint(FingerPrint);
            return res;
        }
    }
}
