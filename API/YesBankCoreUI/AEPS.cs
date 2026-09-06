using Newtonsoft.Json;
using YesBankCore;
using YesBankCore.Request.AEPS;
using YesBankCore.Response.AEPS;

namespace YesBankCoreUI
{
    public partial class AEPS : Form
    {
        public AEPS()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            YesBankAEPSSOR dmt = new YesBankAEPSSOR();

            AuthenticationRequest request = new AuthenticationRequest();
            dmt.SetCredential(textPartnerID.Text, textAgentID.Text, textPrivateKey.Text, textPublicKey.Text, textMode.Text);

            request.TxnId = textTXNID.Text;
            request.AadharNumber = textAadhar.Text;
            request.AppId = textAppid.Text;
            request.AgentName = textName.Text;
            request.AgentAddress = textAdd.Text;
            request.AgentPincode = textPin.Text;
            request.AgentDistrict = textDis.Text;
            request.AgentState = textState.Text;
            request.DeviceType = textType.Text;
            request.DeviceMake = textMake.Text;
            request.DeviceModel = textModel.Text;
            request.DeviceIp = textIP.Text;
            request.BiometricData = textFinger3.Text;
            string File = "<PidData>\r\n  <Resp errCode=\"0\" errInfo=\"Success.\" fCount=\"1\" fType=\"0\" nmPoints=\"48\" qScore=\"81\" />\r\n  <DeviceInfo dpId=\"MANTRA.MSIPL\" rdsId=\"MANTRA.WIN.001\" rdsVer=\"1.0.8\" mi=\"MFS100\" mc=\"MIIEFTCCAv2gAwIBAgIBADANBgkqhkiG9w0BAQsFADCB6jEqMCgGA1UEAxMhRFMgTUFOVFJBIFNPRlRFQ0ggSU5ESUEgUFZUIExURCAzMVUwUwYDVQQzE0xCLTIwMyBTaGFwYXRoIEhleGEgT3Bwb3NpdGUgR3VqYXJhdCBIaWdoIENvdXJ0IFMuRyBIaWdod2F5IEFobWVkYWJhZCAtMzgwMDYwMRIwEAYDVQQJEwlBSE1FREFCQUQxEDAOBgNVBAgTB0dVSkFSQVQxCzAJBgNVBAsTAklUMSUwIwYDVQQKExxNQU5UUkEgU09GVEVDSCBJTkRJQSBQVlQgTFREMQswCQYDVQQGEwJJTjAeFw0yMzExMjYwOTE2MDBaFw0yMzEyMDIxMDMxMDBaMIGwMSUwIwYDVQQDExxNYW50cmEgU29mdGVjaCBJbmRpYSBQdnQgTHRkMR4wHAYDVQQLExVCaW9tZXRyaWMgTWFudWZhY3R1cmUxDjAMBgNVBAoTBU1TSVBMMRIwEAYDVQQHEwlBSE1FREFCQUQxEDAOBgNVBAgTB0dVSkFSQVQxCzAJBgNVBAYTAklOMSQwIgYJKoZIhvcNAQkBFhVzdXBwb3J0QG1hbnRyYXRlYy5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCvZf14J0zJD3gU2Wb3vvR2mDDcggtq5n+4q5SSfFnyRb8Z8RFVbEkX+XaoKzi8YQEbrTkH4SAcIeY66QIEl8zlLPBI+yJmGZpWORIevT2qnwqNJF9ChALYFTfC7KPvNVCpjROpCgVi1o5UY5xyASsCMdSY6TkxL2S3YlKTTccVtXk6mILZe+KAsfDujmcc9t9siw9uIaLmtojNmbRmF3TiCwDVyUeWuzG/xUEc7lujtWlC7WLp0n5sdfsS8mF4uyQQvMt+DIyOsyLN8nd6RwdgKjfsK+ipO5fETJFADE5Lyqxx/B6jRlNwwf7r+3YhgVpsJEr9qazxrLoga2rrpyoHAgMBAAEwDQYJKoZIhvcNAQELBQADggEBABp1eC4v1xvZB86NzAVz4f118KdZLk0p2cCDH3SFRv2smO2R7Vp9BGwGuDlOnB2pYWTNf3Qc26cLz2OppJ6aDcDoOcTmdedG2ajorcO0ApLO7cJvWUlzhsCR+ZvVyZIc07053kr7xCrmyuesLRR+5mfEDH6i5mT6RWlp+fmv8T1upgXTSakX3teJewQXwxjCo/X0oAdgM6unhEa7aTBJbOK01YPn7jfH2lsx7eVcoIds28ERcXvysnss1njp/4mugW0mRrUn5UYm80+Z99TjDhWUSzb5DiRSBf10JjwlhkngIM9eq0I67SnY1mfdCIdpGs3JFRgWMMOblNLvJWZb54A=\" dc=\"955a323b-3f83-45f1-8459-fc43434762d2\">\r\n    <additional_info>\r\n      <Param name=\"srno\" value=\"2150726\" />\r\n      <Param name=\"sysid\" value=\"6C3FEBFC0E49B79FBFF0\" />\r\n      <Param name=\"ts\" value=\"2023-12-01T10:15:38+05:30\" />\r\n    </additional_info>\r\n  </DeviceInfo>\r\n  <Skey XWTXQX+bwe9e6U4R/itSJvxm2KIhtB9FlfRVJPDTIzATFUXbNQA4b2SfXBOl/teXL9gdx7Wvzbr87HA3AiDwzPmr+5N2kMFBojADVgp2vOWQgJ4CYVt1tV5fQ6dd7Db6CMf4C5P1S6A15sT4f4NHoO0frVGL42ylBpxqOgnJf1oo5SQLVMY6OQ5XwtaeSXiaGD8MrgfecFjD+eWi52CO+m+kKkI+hyFDP/o0IPJDNAMIBDblLNkztLOmpnOjHHXWagcAToWUbwn1pEpbyGmFFwGPx2I0i0XnsjUnjXCQoP2MKyywGQAxr1SJBL2/n19hoyfZJruCtea6EpmgIdyz8Q==</Skey>\r\n  <Hmac>q/yoQtkIak7lpPyoKvesTRg8vF7iiGwqGgTwOI4PD74aoG9rMsZOh8l8WZBFchv2</Hmac>\r\n  <Data type=\"X\">MjAyMy0xMi0wMVQxMDoxNTo0MJbV6/ic+C1/M6qaAPfajiaw1n7PUF6Ygm65Xx9RR8eyTuRWUQRF8QI3eNTxejmZmseOTwXA8TI3IUk27BMluY33fu9KIlmBvJSP6XuYGObvT9uJXiWS1F0GuW+DdjszWIEMhXoXYBVb6GMB1jlBy3QVwrM59bVKSiGaa6pCEiFqMIhWY9Bv//p+zBF7LKHGXSQVX3Z34OfTdNXiKi+20Ph6QPktSzPp6NVPmSZgHoN8yv4sW4r0dMNLE+nROEqCnBYXsaMCOZXsDqCqGqBm+3riOQr7UM8tX/R1dQnU1SihEFzI4jfjqDoaYLKrTeWUukDbpxX4is7C8eFkcaSnVish1HPK2vIhGSG9aBc9Ug5ISUTsdW34KnRh/VsDrxRoYy4pV1RQ9BnPICI60fbtZJAz0wK1X3TQfd7VjH0CL8DwlBD90j7LjFInDSnvsagXHiADTaRq6fKpe0JGqkZys29UQooNYAdAF/e+lR4U6Xi9w9VHguyAfY79i4VvM+pBcUaLIuUQMr5MPs0JOttRw4xW/v+O9qxLliXuXj0O7FbsqF4iXJC7QI9Uk5bdeeBIf2PZsXo7wSIoKyUN93Mz5TrpoSSguQMQbdgu7AbdyLcDXjG9I3IylT4o6UCp7gfJK0qkiQldB1jQMzRf7s7M06zypiDMcxKgT3mrcLZdgAqshdGOV1Tq69xrwcjvBAWFarbq3dh8O2g9Ww/MvWSrnjIxD7hc6KPhrqYCmaSLecbFrR8tlxxWEIAeHqQ0Jw58NJOuuGvNM4JxpKpwvgIv7kIPP886ZpvSd9qCb1p+qM2gToUmUJeC0dT9PFXI/LLmqp9gkXSsGtePMDRrl5aByQEyeC7yD80cwIg66W+4/BEpP4BCPqEQrJHC56VniBJ8YmU58QV2A1UOyDhoYwLzeNJ7QBmAbhrkjx82ZK7a54otry5YNVKxP23dUPTU7MOioaarwfeMhsY+oDvHFSsjOb0NHh+1E1b9VIBaMJk9iOUlhYd9OxCinrjol6HxH4oUZUhpN5BnBrG804yJ7wRMXPearNa3dwMUnLgtlb4kCR2oUY6ntSDIq7upfc6fPf6S1ihNmgMFsTv09FrHtsKcnSy0zoOXFuUuNUCbfcofUOpBBDU60abWoVTsjFWykRsYtlFUF8eGqPtWoKtqZByHhPZKHKdIOC09nBBz6VWBwU/c98VceJ361YP+0RHZMa+XCy450eMCdcmAg/gJBKSBI4+EgvtRRNxnnnYnsFjokQ+7VdNrObp6omYlkNsV0kmeVUOu0IrQb7rR7RvfuQWojQ==</Data>\r\n</PidData>";

            AuthenticationResponse response = dmt.Authentication(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            YesBankAEPSSOR dmt = new YesBankAEPSSOR();

            BalanceEnquiryRequest request = new BalanceEnquiryRequest();
            dmt.SetCredential(textPartnerID.Text, textAgentID.Text, textPrivateKey.Text, textPublicKey.Text, textMode.Text);

            request.TxnId = texttxn.Text;
            request.OrderId = textorder.Text;
            request.AadharNumber = textAdhar1.Text;
            request.CustomerName = textname1.Text;
            request.BankIIN = textIIN.Text;
            request.BiometricData = textfinger.Text;
            request.Latitude = textLat.Text;
            request.Longitude = textLong.Text;
            request.AppId = textAppID1.Text;
            request.AgentName = textAgName.Text;
            request.AgentAddress = textAdd1.Text;
            request.AgentPincode = textPinCode1.Text;
            request.AgentDistrict = textDis1.Text;
            request.AgentState = textstate1.Text;
            request.DeviceType = textDevicetype.Text;
            request.DeviceMake = textDMake.Text;
            request.DeviceModel = textModel1.Text;
            request.DeviceIp = textIP1.Text;
            request.CustomerMob = textMobile.Text;

            CashWithdrawalResponse response = dmt.BalanceEnquiry(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            YesBankAEPSSOR dmt = new YesBankAEPSSOR();

            CashWithdrawalRequest request = new CashWithdrawalRequest();
            dmt.SetCredential(textPartnerID.Text, textAgentID.Text, textPrivateKey.Text, textPublicKey.Text, textMode.Text);

            request.TxnId = texttxn2.Text;
            request.OrderId = textorder2.Text;
            request.SorTxnId = textsort2.Text;
            request.Amount = textAmt2.Text;
            request.AadharNumber = textAadhar2.Text;
            request.BankIIN = textIIN2.Text;
            request.CustomerName = textname1.Text;

            request.BiometricData = textFinger2.Text;
            request.Latitude = textLat2.Text;
            request.Longitude = textlong2.Text;
            request.AppVersion = textversion.Text;
            request.AppId = textAppID2.Text;
            request.AgentName = textAgentName2.Text;
            request.AgentAddress = textAgentAdd2.Text;
            request.AgentPincode = textPin1.Text;
            request.AgentDistrict = textDis2.Text;
            request.AgentState = textstate2.Text;
            request.DeviceType = textDocType.Text;
            request.DeviceMake = textDOCMake.Text;
            request.DeviceModel = textModel2.Text;
            request.DeviceIp = textIP2.Text;
            request.CustomerMob = textMOB2.Text;

            CashWithdrawalResponse response = dmt.Withdraw(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            YesBankAEPSSOR dmt = new YesBankAEPSSOR();

            MiniStatementRequest request = new MiniStatementRequest();
            dmt.SetCredential(textPartnerID.Text, textAgentID.Text, textPrivateKey.Text, textPublicKey.Text, textMode.Text);

            request.TxnId = textTXN4.Text;
            request.OrderID = textOrder4.Text;
            request.AadharNumber = textAadhar4.Text;
            request.BankIIN = textIIN4.Text;
            request.CustomerName = textCName4.Text;
            request.BiometricData = textbiometric4.Text;
            request.Latitude = textLat4.Text;
            request.Longitude = textLong4.Text;
            request.AppVersion = textVersion4.Text;
            request.AppId = textApid4.Text;
            request.AgentName = textAgentname4.Text;
            request.AgentAddress = textAdd4.Text;
            request.AgentPincode = textPin4.Text;
            request.AgentDistrict = textDis4.Text;
            request.AgentState = textstate4.Text;
            request.DeviceType = texttype2.Text;
            request.DeviceMake = textmake4.Text;
            request.DeviceModel = textModel4.Text;
            request.DeviceIp = textip4.Text;
            request.CustomerMob = textmobile4.Text;

            MiniStatementResponse response = dmt.MiniStatement(request);
            string requestJSON = JsonConvert.SerializeObject(response);
            txtResult.Text = requestJSON;
        }

        private void textFinger3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
