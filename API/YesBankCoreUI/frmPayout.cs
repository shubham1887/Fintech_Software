using Newtonsoft.Json;
using YesBankCore;
using YesBankCore.Request;
using YesBankCore.Response;
using YESBankUAT;

namespace YesBankCoreUI
{
    public partial class frmPayout : Form
    {
        public frmPayout()
        {
            InitializeComponent();
        }

        private void btnFileSelect_Click(object sender, EventArgs e)
        {
            if (ofdFile.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = ofdFile.FileName;

                string lines = File.ReadAllText(txtFilePath.Text);

                UserInput ui = JsonConvert.DeserializeObject<UserInput>(lines);

                txtMID.Text = ui.MID;
                txtMerchantKey.Text = ui.MerchantKey;

                txtIBMClientID.Text = ui.XIBMClientID;
                txtIBMClientSecret.Text = ui.XIBMClientSecret;

                txtPrivatePassword.Text = ui.PrivateKeyFilePassword;
                txtPrivateFilePath.Text = ui.PrivateKeyFile;

                txtMerchantName.Text = ui.MerchantName;
                txtAdminVPA.Text = ui.MerchantVPA;
                txtMCC.Text = ui.MCC;

                txtAuthUserName.Text = ui.AuthUserName;
                txtAuthPassword.Text = ui.AuthPassword;
            }
        }

        private YesBankPayout mt = new YesBankPayout();

        private void SetCredential()
        {
            mt.SetCredential(txtMID.Text.Trim(), txtMerchantKey.Text.Trim(), txtIBMClientID.Text.Trim(),
                txtIBMClientSecret.Text.Trim(), txtPrivatePassword.Text.Trim(), txtPrivateFilePath.Text.Trim(),
                txtAuthUserName.Text, txtAuthPassword.Text, txtMode.Text);
        }

        private void btnGetBalance_Click(object sender, EventArgs e)
        {
            SetCredential();

            BalanceResponse response = mt.GetBalance();

            string requestJSON = JsonConvert.SerializeObject(response);
            txtResponse.Text = requestJSON;
        }

        private void btnPayout_Click(object sender, EventArgs e)
        {
            SetCredential();
            PayoutRequest input = new PayoutRequest();
            input.accountNumber = txtAccountNo.Text;
            input.beneIfscCode = txtIFSC.Text;
            input.transactionMode = txtPaymode.Text;
            input.amount = txtAmount.Text;
            input.orderID = txtAPIRequest.Text;

            PayoutResponse response = mt.DomesticPayment(input);

            string requestJSON = JsonConvert.SerializeObject(response);
            txtResponse.Text = requestJSON;
        }

        private void btnStatusCheck_Click(object sender, EventArgs e)
        {
            SetCredential();
            PayoutStatusCheckRequest input = new PayoutStatusCheckRequest();

            input.orderID = txtAPIRequestStatusCheck.Text;

            PayoutResponse response = mt.StatusCheck(input);

            string requestJSON = JsonConvert.SerializeObject(response);
            txtResponse.Text = requestJSON;
        }

      
    }
}