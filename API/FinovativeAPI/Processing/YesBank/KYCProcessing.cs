using Newtonsoft.Json;
using Project.BAL;
using Project.Model;
using Project.Response.eKYCSetting;
using Project.StaticModel;
using YesBankCore;
using YesBankCore.Request.KYC;
using YesBankCore.Response.KYC;

namespace Project.Processing.YesBank
{
    public class KYCProcessing
    {
        private YesBankKYCSOR mt = new YesBankKYCSOR();

        public KYCProcessing()
        {
            ViewModel eKYC = new eKYCSettingBal().SelectActiveeKYCByName(eKycCode.YesBank);
            string PartnerKey = eKYC.MerchantID;
            string PrivateKey = eKYC.AuthKey;
            string BankPublicKey = eKYC.Token;

            mt.SetCredential(PartnerKey, PrivateKey,BankPublicKey);
        }

        public (CreateAgentResponse,string) KYCInitiate(UserMaster User)
        {

            AgentCreationRequest input = new AgentCreationRequest();
            input.requestId = new Util().GetRandomNumberWithDatePrefix();
            input.p1 = User.MobileNumber;
            input.p2 = User.PersonName;
            input.p3 =  User.PANNo;
            input.p4 = "10/10/1971";// User.DOC.ToString();
            input.p5 = "M";// User.Gender;
            input.p7 = User.UserID.ToString();
            input.p8 = User.FirmName;

            CreateAgentResponse resp = mt.AgentCreation(input);
            string resStr = JsonConvert.SerializeObject(resp);
            string reqStr = JsonConvert.SerializeObject(input);
            new Logger().Info("YesBank KYC", "AgentCreation", reqStr + "::" + resStr);

            return (resp, reqStr);
        }

        public OTPValidateResponse OTPValidate(Requests.eKYC.eKYCValidateRequest mainRequest, UserMaster User)
        {
            OTPValidateRequest input = new OTPValidateRequest();
            input.requestId = mainRequest.RequestID;
            input.p1 = User.MobileNumber;
            input.p2 = mainRequest.otpToken;
            input.p3 = mainRequest.Otp;

            OTPValidateResponse resp = mt.VerifyOTP(input);
            string resStr = JsonConvert.SerializeObject(resp);
            string reqStr = JsonConvert.SerializeObject(input);
            new Logger().Info("YesBank KYC", "VerifyOTP", reqStr + "::" + resStr);

            return resp;
        }

        public GetAadharWadhResponse AadharWadh(string requestId,string KycToken, UserMaster User)
        {
            GetAadharWadhRequest input = new GetAadharWadhRequest();
            input.requestId = requestId;
            input.p1 = User.MobileNumber;
            input.p2 = KycToken;
            input.p3 = User.UserID.ToString();

            GetAadharWadhResponse resp = mt.AadharWadh(input);
            string resStr = JsonConvert.SerializeObject(resp);
            string reqStr = JsonConvert.SerializeObject(input);
            new Logger().Info("YesBank KYC", "AadharWadh", reqStr + "::" + resStr);

            return resp;
        }

        public AadharBioMetricKYCResponse AadharBiometricKYC(Project.Requests.eKYC.YesBank_AadharBiometricRequest mainRequest, UserMaster User)
        {
            AadharBiometricKYCRequest input = new AadharBiometricKYCRequest();
            input.requestId = mainRequest.RequestID;
            input.p1 = User.MobileNumber;
            input.p2 = mainRequest.KYCToken;
            input.p3 = User.UserID.ToString();
            input.p4 = mainRequest.AadharNo;
            input.p5 = "0";
            input.p7 = mainRequest.FingerPrint;
            input.p8 = "7~RightIndex";
            input.p9 = mainRequest.SerialNumber;
            input.p12 ="0";
            input.p14 = "Y";
            input.p15 = "EKYC";
            input.p16 = "Null";
            input.p17 = "Authentication";
            input.p18 = mainRequest.WadhValue;

            AadharBioMetricKYCResponse resp = mt.AadharBioMertic(input);
            string resStr = JsonConvert.SerializeObject(resp);
            string reqStr = JsonConvert.SerializeObject(input);
            new Logger().Info("YesBank KYC", "VerifyOTP", reqStr + "::" + resStr);

            return resp;
        }

    }
}