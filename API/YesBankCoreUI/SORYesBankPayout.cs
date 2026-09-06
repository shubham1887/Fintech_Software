using Org.BouncyCastle.Asn1.Ocsp;
using YesBankCore;
using YesBankCore.Bal;
using YesBankCore.Request.Payout;
using YesBankCore.Response.Payout;

namespace YesBankCoreUI
{
    public partial class SORYesBankPayout : Form
    {
        public SORYesBankPayout()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hitachi_PayoutYesBank credential = new Hitachi_PayoutYesBank();
            credential.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            FetchBalanceRequest request = new FetchBalanceRequest();
            request.RequestNumber = txtRequestID.Text;

            BalanceFetchResponse response = new PayoutBal().FetchBalance(request);

            Result.Text = response.Message;

        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            Hitachi_PayoutYesBank credential = new Hitachi_PayoutYesBank();
            credential.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            FundTransferRequest request = new FundTransferRequest();
            request.partnerReferenceNo = txtParnerRefenceNo.Text;
            request.RequestNumber = txtRequestID.Text;

            request.AccountNumber = txtAccountNumber.Text;
            request.AccountIFSC = txtIFSC.Text;
            request.Mode = txtMode.Text;
            request.Amount = txtAmount.Text;
            request.BeneficiaryName = txtCreditorName.Text;



            PayoutResponse response = new PayoutBal().MoneyTransfer(request);

            Result.Text = response.Message;
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            Hitachi_PayoutYesBank credential = new Hitachi_PayoutYesBank();
            credential.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            PayoutStatusCheckRequest request = new PayoutStatusCheckRequest();
            request.PartnerReferenceNumber = txtParnerRefenceNo.Text;
            request.RequestNumber = txtRequestID.Text;
            PayoutResponse response = new PayoutBal().StatusCheck(request);
            Result.Text = response.Message;
        }

        private void btnVerification_Click(object sender, EventArgs e)
        {
            Hitachi_PayoutYesBank credential = new Hitachi_PayoutYesBank();
            credential.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            VerificationRequest request = new VerificationRequest();
            request.AccountIFSC = txtVIFSC.Text;
            request.AccountNumber = txtAccountNumber.Text;
            request.RequestNumber = txtRequestID.Text;
            PayoutResponse response = new PayoutBal().Verification(request);
            Result.Text = response.Message;
        }

     
    }
}
