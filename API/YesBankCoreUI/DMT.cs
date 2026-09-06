using Newtonsoft.Json;
using YesBankCore;
using YesBankCore.Request.MoneyTransfer;
using YesBankCore.Response.MoneyTransfer;

namespace YesBankCoreUI
{
    public partial class DMT : Form
    {
        public DMT()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            YesBankDMTSOR dmt = new YesBankDMTSOR();

            SenderSelectRequest request = new SenderSelectRequest();
            dmt.SetCredential(PartnerID.Text, AgentID.Text, PrivateKey.Text, PublicKey.Text, Mode.Text);

            request.TxnId = TXNID.Text;
            request.CustomerMob = Mobile.Text;         
       
            SenderSelectResponse response = dmt.SelectSenderDetail(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            YesBankDMTSOR dmt = new YesBankDMTSOR();

            BeneAddSendOTPRequest request = new BeneAddSendOTPRequest();
            dmt.SetCredential(PartnerID.Text, AgentID.Text, PrivateKey.Text, PublicKey.Text, Mode.Text);

            request.TxnId = textTxnID.Text;
            request.CustomerMob = textMobile.Text;
            request.Name = textBeneName.Text;
            request.AccountNo = textAccNo.Text;

            AddBeneOTPResponse response = dmt.AddBeneficiary(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            YesBankDMTSOR dmt = new YesBankDMTSOR();
            BeneAddRequest request = new BeneAddRequest();
            dmt.SetCredential(PartnerID.Text, AgentID.Text, PrivateKey.Text, PublicKey.Text, Mode.Text);

            request.TxnId = textTxn.Text;
            request.CustomerMob = textMOB.Text;
            request.Name = textName.Text;
            request.BenefAccNo = textAccnum.Text;
            request.IfscCode = textIFSC.Text;
            request.PinCode = textPin.Text;
            request.City = textCity.Text;
            request.Address = textAdd.Text;
            request.Otp = textOTP.Text;

            AddBeneResponse response = dmt.ValidateBeneficiaryOTP(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            YesBankDMTSOR dmt = new YesBankDMTSOR();
            BeneListRequest request = new BeneListRequest();
            dmt.SetCredential(PartnerID.Text, AgentID.Text, PrivateKey.Text, PublicKey.Text, Mode.Text);

            request.TxnId = textTXN1.Text;
            request.CustomerMob = textConsumerMob.Text;         

            BeneListResponse response = dmt.GetAllBeneficiary(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;

        }

        private void button5_Click(object sender, EventArgs e)
        {
            YesBankDMTSOR dmt = new YesBankDMTSOR();
            BeneDeactivationRequest request = new BeneDeactivationRequest();
            dmt.SetCredential(PartnerID.Text, AgentID.Text, PrivateKey.Text, PublicKey.Text, Mode.Text);

            request.TxnId = texttxn2.Text;
            request.CustomerMob = textmobile1.Text;
            request.AccountNo = textAccno1.Text;
            request.IfscCode = textifsc4.Text;

            DeActivateBeneResponse response = dmt.DeleteBeneficiary(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            YesBankDMTSOR dmt = new YesBankDMTSOR();
            VerifyRequest request = new VerifyRequest();
            dmt.SetCredential(PartnerID.Text, AgentID.Text, PrivateKey.Text, PublicKey.Text, Mode.Text);

            request.TxnId = textTxnId3.Text;
            request.CustomerMob = textMobile3.Text;
            request.AccountNo = textAccNo3.Text;
            request.Ifsc = textIFSC3.Text;
            request.BeneName = textName3.Text;

            TransferResponse response = dmt.VerifyBeneficiaryAccount(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            YesBankDMTSOR dmt = new YesBankDMTSOR();
            TransferRequest request = new TransferRequest();
            dmt.SetCredential(PartnerID.Text, AgentID.Text, PrivateKey.Text, PublicKey.Text, Mode.Text);

            request.TxnId = textTXN5.Text;
            request.CustomerMob = textMobile5.Text;
            request.AccountNo = textAccnum5.Text;
            request.Ifsc = textifsc5.Text;
            request.OrderId = textOrderID.Text;
            request.PaymentMode = textmode.Text;
            request.Amount = textAMT.Text ;
            request.MobileNo = textBeneMobile.Text;
            request.EmailId = textMail.Text;
            request.Name = textBeneName5.Text;

            TransferResponse response = dmt.Transfer(request);
            string requestJSON = JsonConvert.SerializeObject(response);

        }

        private void button8_Click(object sender, EventArgs e)
        {
            YesBankDMTSOR dmt = new YesBankDMTSOR();
            SenderAddRequest request = new SenderAddRequest();
            dmt.SetCredential(PartnerID.Text, AgentID.Text, PrivateKey.Text, PublicKey.Text, Mode.Text);

            request.TxnId = textTxn6.Text;
            request.CustomerMob = textMob6.Text;
            request.Title = textTitle6.Text;
            request.Name = textName6.Text;
            request.Gender = textGender.Text;
            request.Address = textAdd6.Text;
            request.City = textCity6.Text;
            request.State = textState6.Text;
            request.Pincode = textPin6.Text;
            request.District = textDis.Text;
            request.DateOfBirth = textDob.Text;
            request.IdentityDocNo = textINo.Text;
            request.IdentityDocType = textIType.Text;
            request.AddressDocNo = textAno.Text;
            request.AddressDocType = textAType.Text;

            SenderAddResponse response = dmt.SenderRegistration(request);
            string requestJSON = JsonConvert.SerializeObject(response);
        }
    }
}
