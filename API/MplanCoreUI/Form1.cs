using MplanCore.Processing;
using MplanCore.Request;
using MplanCore.Request.MobiKwik;
using MplanCore.Response;
using MplanCore.Response.ShighraPay;
using MplanCore.StaticModal;
using MPlanCore.Processing;
using Newtonsoft.Json;

namespace MplanCoreUI
{
    public partial class Form1 : Form
    {
        private string Operator = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DTHRequest input = new DTHRequest();
            OperatorResponse circleResponse = new OperatorResponse();
            ShighraPayCredentials.SetCredentials(input.api_token = textBox1.Text, input.user_id = textBox2.Text);

            input.opid = textBox3.Text;
            //input.mobile_number = textBox8.Text;
            input.circleid = textBox4.Text;
            //input.circleid = circleResponse.circleid;

            if (string.IsNullOrEmpty(input.opid))
            {
                MessageBox.Show("Please enter valid Operator."); textBox3.Focus();
                return;
            }

            DTHRecords response = null;
            try
            {
                response = ShighrapayAPIProcessing.SelectDTHPlan(input, Operator);
            }
            catch (Exception ex)
            {
            }
            txtResult.Text = JsonConvert.SerializeObject(response);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OperatorRequest input = new OperatorRequest();
            ShighraPayCredentials.SetCredentials(input.api_token = textBox5.Text, input.user_id = textBox6.Text);

            input.mobile_number = textBox7.Text;

            //if (string.IsNullOrEmpty(input.opid))
            //{
            //    MessageBox.Show("Please enter valid Operator."); textBox3.Focus();
            //    return;
            //}

            OperatorResponse response = null;
            try
            {
                response = ShighrapayAPIProcessing.GetCircleCode(input);
            }
            catch (Exception ex)
            {
            }
            txtResult.Text = JsonConvert.SerializeObject(response);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PlanViewRequest input = new PlanViewRequest();
           
            //MobiKwikCredentials.SetCredentials();

            input.CircleID = textBox10.Text;
            input.OperatorID = textBox9.Text;
            input.XMClient = textBox11.Text;

            string IPAddress = "";
            string Operator = "";
            Records response = null;
            try
            {
                response = MobiKwikProcessing.SelectPlan(input,Operator);
            }
            catch (Exception ex)
            {
            }
            txtResult.Text = JsonConvert.SerializeObject(response);
        }
    }
}