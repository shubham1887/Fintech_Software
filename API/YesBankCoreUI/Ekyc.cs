using Newtonsoft.Json;
using YesBankCore;
using YesBankCore.Request.KYC;
using YesBankCore.Response.KYC;

namespace YesBankCoreUI
{
    public partial class Ekyc : Form
    {
        public Ekyc()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            YesBankKYCSOR ekyc = new YesBankKYCSOR();
            ekyc.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            AgentCreationRequest input = new AgentCreationRequest();
            input.partnerKey = PartnerKey.Text;
            input.requestId = txtRequestID.Text;
            input.p1 = txtMobile.Text;
            input.p2 = txtFullName.Text;
            input.p3 = txtPan.Text;
            input.p4 = txtDOB.Text;
            input.p5 = txtGender.Text;
            input.p7 = txtRefID.Text;
            input.p8 = txtShopName.Text;

            CreateAgentResponse createAgentResponse = new YesBankKYCSOR().AgentCreation(input);
            string requestJSON = JsonConvert.SerializeObject(createAgentResponse);
            Result.Text = requestJSON;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            YesBankKYCSOR ekyc = new YesBankKYCSOR();
            ekyc.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            OTPValidateRequest input = new OTPValidateRequest();
            input.partnerKey = PartnerKey.Text;
            input.requestId = txtRequestID.Text;
            input.p1 = txtMobile.Text;
            input.p2 = otp_token.Text;
            input.p3 = otp.Text;

            OTPValidateResponse oTPValidateResponse = new YesBankKYCSOR().VerifyOTP(input);
            string requestJSON = JsonConvert.SerializeObject(oTPValidateResponse);
            Result.Text = requestJSON;

        }

        private void BtnSubmitToken_Click(object sender, EventArgs e)
        {
            YesBankKYCSOR ekyc = new YesBankKYCSOR();
            ekyc.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            GetAadharWadhRequest input = new GetAadharWadhRequest();
            input.partnerKey = PartnerKey.Text;
            input.requestId = txtRequestID.Text;
            input.p1 = txtMobile.Text;
            input.p2 = txtkyc_token.Text;
            input.p3 = txtRefID.Text;

            GetAadharWadhResponse response = new YesBankKYCSOR().AadharWadh(input);
            string requestJSON = JsonConvert.SerializeObject(response);
            Result.Text = requestJSON;
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            YesBankKYCSOR ekyc = new YesBankKYCSOR();
            ekyc.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            AadharBiometricKYCRequest input = new AadharBiometricKYCRequest();
            input.partnerKey = PartnerKey.Text;
            input.requestId = txtRequestID.Text;
            input.p1 = txtMobile.Text;
            input.p2 = txtkyc_token.Text;
            input.p3 = txtRefID.Text;
            input.p4 = txtaadhaar_number.Text;
            input.p5 = txtaadhaar_type.Text;
            input.p7 = txtdevice_data_xml.Text;
            input.p8 = txtfinger_data.Text;
            input.p9 = txtserial_number.Text;
            input.p12 = txttype.Text;
            input.p14 = txtconsent.Text;
            input.p15 = txtEKYC_type.Text;
            input.p16 = txtcaptured.Text;
            input.p17 = txtpurpose.Text;
            input.p18 = txtwadh_value.Text;

            AadharBioMetricKYCResponse response = new YesBankKYCSOR().AadharBioMertic(input);
            string requestJSON = JsonConvert.SerializeObject(response);
            Result.Text = requestJSON;
        }

        private void btnEYC_Click(object sender, EventArgs e)
        {
            YesBankKYCSOR ekyc = new YesBankKYCSOR();
            ekyc.SetCredential(PartnerKey.Text, txtPrivateKey.Text.Trim(), txtPublicKey.Text.Trim());
            AgentCreationRequest input = new AgentCreationRequest();
            input.partnerKey = PartnerKey.Text;
            input.requestId = txtRequestID.Text;
            input.p1 = txtMobile.Text;
            input.p2 = txtFullName.Text;
            input.p3 = txtPan.Text;
            input.p4 = txtDOB.Text;
            input.p5 = txtGender.Text;
            input.p7 = txtRefID.Text;
            input.p8 = txtShopName.Text;

            CreateAgentResponse createAgentResponse = new YesBankKYCSOR().AgentCreation(input);
            string requestJSON = JsonConvert.SerializeObject(createAgentResponse);
            Result.Text = requestJSON;
        }
    }
}
