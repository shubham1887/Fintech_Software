using YesBankCore.KYCBAL;
using YesBankCore.Model;
using YesBankCore.Request.KYC;
using YesBankCore.Response.KYC;
using YESBankCore.Controller;

namespace YesBankCore
{
    public class YesBankKYCSOR
    {
        public void SetCredential(string PartnerKey, string PrivateKey, string BankPublicKey)
        {
            KYCCredentials.PartnerKey = PartnerKey;

            KYCCredentials.PrivateKey = PrivateKey;
            KYCCredentials.BankPublicKey = BankPublicKey;
        }

        public CreateAgentResponse AgentCreation(AgentCreationRequest input)
        {
            CreateAgentResponse response = new CreateAgentResponse();
            response.Status = Message.FAILURE;
            response.Message = "";

            input.actionName = "CREATE_YM_AGENT";
            input.partnerKey = KYCCredentials.PartnerKey;

            KycBAL bal = new KycBAL();
            response = bal.AgentCreation(input);
            return response;
        }

        public OTPValidateResponse VerifyOTP(OTPValidateRequest input)
        {
            OTPValidateResponse response = new OTPValidateResponse();
            response.Status = Message.FAILURE;
            response.Message = "";

            input.actionName = "VALIDATE_OTP";
            input.partnerKey = KYCCredentials.PartnerKey;
            KycBAL bal = new KycBAL();
            response = bal.VerifyOTP(input);
            return response;
        }

        public GetAadharWadhResponse AadharWadh(GetAadharWadhRequest input)
        {
            GetAadharWadhResponse response = new GetAadharWadhResponse();
            response.Status = Message.FAILURE;
            response.Message = "";

            input.actionName = "GET_AADHAAR_WADH";
            input.partnerKey = KYCCredentials.PartnerKey;
            KycBAL bal = new KycBAL();
            response = bal.AadharWadh(input);
            return response;
        }

        public AadharBioMetricKYCResponse AadharBioMertic(AadharBiometricKYCRequest input)
        {
            AadharBioMetricKYCResponse response = new AadharBioMetricKYCResponse();
            response.Status = Message.FAILURE;
            response.Message = "";

            input.actionName = "BIOMETRIC_KYC";
            input.partnerKey = KYCCredentials.PartnerKey;
            input.p14 = "Y";
            input.p15 = "EKYC";
            input.p16 = "Null";
            input.p17 = "Authentication";
            if (input.p4.Length > 12)
            {
                input.p5 = "2";
            }
            else
            {
                input.p5 = "0";
            }
            KycBAL bal = new KycBAL();
            response = bal.AadharBioMertic(input);
            return response;
        }
    }
}
