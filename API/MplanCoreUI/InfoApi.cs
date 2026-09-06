using MplanCore.Processing;
using MplanCore.Response;
using MplanCore.StaticModal;
using Newtonsoft.Json;

namespace MplanCoreUI
{
    public partial class InfoApi : Form
    {
        private string Operator;
        private readonly string Circle = "Maharashtra Goa";
        private string ConsumerNumber = "";

        public InfoApi()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InfoapiCredentials.SetCredentials(textBox1.Text);

            Operator = textBox3.Text;

            if (string.IsNullOrEmpty(Operator))
            {
                MessageBox.Show("Please enter valid Operator."); textBox3.Focus();
                return;
            }

            ElectricityViewResponse response = null;
            try
            {
                response = InfoAPIProcessing.ViewElectricityUserInfo(Operator, textBox2.Text);
            }
            catch (Exception ex)
            {
            }
            txtResult.Text = JsonConvert.SerializeObject(response);
        }
    }
}
