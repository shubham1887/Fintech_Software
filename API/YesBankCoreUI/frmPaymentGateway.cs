using Newtonsoft.Json;
using QRCoder;
using YESBankCore;
using YESBankCore.Request;
using YESBankCore.Response;

namespace YESBankUAT
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ErrorProcessing log = new ErrorProcessing();

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

        public string GetGUID()
        {
            string guid = Guid.NewGuid().ToString();
            guid = guid.Replace("-", "");
            if (guid.Length >= 20)
                return guid.Substring(0, 20);
            else
                return guid;
        }

        private Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private YesBank mt = new YesBank();

        private void SetCredential()
        {
            mt.SetCredential(txtMID.Text.Trim(), txtMerchantKey.Text.Trim(), txtIBMClientID.Text.Trim(),
                txtIBMClientSecret.Text.Trim(), txtPrivatePassword.Text.Trim(), txtPrivateFilePath.Text.Trim() + "," + txtMode.Text.Trim());
        }

        private void btnQRInit_Click(object sender, EventArgs e)
        {
            SetCredential();

            QRPayRequest input = new QRPayRequest();
            input.OrderNo = GetGUID();
            input.Amount = Convert.ToInt32(txtAmount.Text) + ".00";

            txtOrderID.Text = input.OrderNo;

            QRInitResponse qrResponse = mt.QRInit(input);

            if (qrResponse.Status != "SUCCESS")
            {
                MessageBox.Show("Error " + qrResponse.Message);
                return;
            }

            string UPIURL = "upi://pay?pa=[UPIID]&pn=[PN]&tr=[TR]&am=[Amount]&cu=INR&mc=[MCCCode]";
            UPIURL = UPIURL.Replace("[UPIID]", qrResponse.AdminVPA);
            UPIURL = UPIURL.Replace("[PN]", qrResponse.MerchantName.Trim().Replace(" ", "%20"));
            UPIURL = UPIURL.Replace("[Amount]", txtAmount.Text);
            UPIURL = UPIURL.Replace("[TR]", input.OrderNo);
            UPIURL = UPIURL.Replace("[MCCCode]", qrResponse.MCCCode);

            //log.saveRequest("UPIURL", UPIURL);

            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode(UPIURL, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            byte[] b = BitmapToBytesCode(qrCodeImage);

            picQR.Image = Image.FromStream(new MemoryStream(b));

            string requestJSON = JsonConvert.SerializeObject(qrResponse);
            txtResponse.Text = requestJSON + Environment.NewLine + UPIURL;
        }

        private void btnVPAValidate_Click(object sender, EventArgs e)
        {
            SetCredential();

            VPAValidateRequest input = new VPAValidateRequest();
            input.OrderNo = GetGUID();
            input.VPA = txtVPA.Text;

            QRPayResponse senderResp = mt.VPAValidate(input);

            string requestJSON = JsonConvert.SerializeObject(senderResp);
            txtResponse.Text = requestJSON;
        }

        private void btnStatusCheck_Click(object sender, EventArgs e)
        {
            SetCredential();

            StatusCheckRequest input = new StatusCheckRequest();
            input.OrderNo = txtOrderID.Text;

            StatusCheckResponse senderResp = mt.StatusCheck(input);

            string requestJSON = JsonConvert.SerializeObject(senderResp);
            txtResponse.Text = requestJSON;
        }

        private void btnCallbAck_Click(object sender, EventArgs e)
        {
            SetCredential();

            StatusCheckResponse senderResp = mt.Callback(txtCallback.Text);

            string requestJSON = JsonConvert.SerializeObject(senderResp);
            txtResponse.Text = requestJSON;
        }
    }
}