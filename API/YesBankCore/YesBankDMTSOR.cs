using YesBankCore.MoneyTransferBAL;
using YesBankCore.Request.MoneyTransfer;
using YesBankCore.Response.MoneyTransfer;
using YESBankCore.Controller;
using YESBankCore.Model;

namespace YesBankCore
{
    public class YesBankDMTSOR
    {
        public void SetCredential( string PartnerID, string AgentID, string PrivateKey,string PublicKey, string Mode)
        {
            MoneyTransferCredential.PARTNERID = PartnerID;
            MoneyTransferCredential.AGENTID = AgentID;
            MoneyTransferCredential.PRIVATEKEY = PrivateKey;
            MoneyTransferCredential.PUBLICKEY = PublicKey;
            MoneyTransferCredential.Mode = Mode;
        }

        public SenderSelectResponse SelectSenderDetail(SenderSelectRequest input)
        {
            SenderSelectResponse response = new SenderSelectResponse();
            response.Status = Message.FAILURE;
            response.Message = "";
            SenderBal bal = new SenderBal();

            input.ProductCode = new List<string> {"DMT"};
            input.Channel = "1";
            input.IsOTPValidated = "0";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            return bal.SelectSenderDetail(input);
        }

        public SenderAddResponse SenderRegistration(SenderAddRequest input)
        {
            SenderAddResponse response = new SenderAddResponse();
            response.Status = Message.FAILURE;
            response.Message = "";
            SenderBal bal = new SenderBal();

            input.Channel = "1";
            input.IsOTPValidated = "0";
            input.ProductCode = new List<string> { "DMT" };
            input.PancardStatus ="0";
            input.KycStatus ="0";
            input.CardStatus ="1";
            input.MaritalStatus ="1";
            input.VersionNo ="1";
            input.IdentityDocType ="1";
            input.IdentityDocNo ="9823448848";
            input.AddressDocType ="3";
            input.AddressDocNo ="9823448848";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            return bal.SenderRegistration(input);
        }

        public AddBeneOTPResponse AddBeneficiary(BeneAddSendOTPRequest input)
        {
            AddBeneOTPResponse response = new AddBeneOTPResponse();
            response.Status = Message.FAILURE;
            response.Message = "";
            BeneficiaryBAL bal = new BeneficiaryBAL();

            input.Channel = "1";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;

            return bal.AddBeneficiary(input);
        }

        public AddBeneResponse ValidateBeneficiaryOTP(BeneAddRequest input)
        {
            AddBeneResponse response = new AddBeneResponse();
            response.Status = Message.FAILURE;
            response.Message = "";
            BeneficiaryBAL bal = new BeneficiaryBAL();

            input.ProductCode = "DMT";
            input.Channel = "1";
            input.Title = "Mr";
            input.Country = "IN";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;

            return bal.ValidateBeneficiaryOTP(input);
        }

        public DeActivateBeneResponse DeleteBeneficiary(BeneDeactivationRequest input)
        {
            DeActivateBeneResponse response = new DeActivateBeneResponse();
            response.Status = Message.FAILURE;
            response.Message = "";
            BeneficiaryBAL bal = new BeneficiaryBAL();

            input.ProductCode = "DMT";  
            input.Channel = "1";
            input.Status = "0";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;

            return bal.DeleteBeneficiary(input);
        }

        public BeneListResponse GetAllBeneficiary(BeneListRequest input)
        {
            BeneListResponse response = new BeneListResponse();
            response.Status = Message.FAILURE;
            response.Message = "";
            BeneficiaryBAL bal = new BeneficiaryBAL();

            input.Channel="1";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            return bal.GetAllBeneficiary(input);
        }

        public TransferResponse VerifyBeneficiaryAccount(VerifyRequest input)
        {
            TransferResponse mainResponse = new TransferResponse();
            mainResponse.Status = Message.FAILURE;
            DMTBAL bal = new DMTBAL();

            input.IsUpi = "0";
            input.Narration = "Beneficiary Verification";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            return bal.VerifyBeneficiaryAccount(input);
        }

        public TransferResponse Transfer(TransferRequest input)
        {
            TransferResponse response = new TransferResponse();
            response.Status = Message.FAILURE;
            response.Message = "";
            DMTBAL bal = new DMTBAL();
         
            input.Narration = "Fund Transfer";
            input.CurrencyCode = "INR";
            input.Channel = "1";
            input.PurposeCode = "GST";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            return bal.Transfer(input);
        }

        public TransferResponse TransctionStatusCheck(StatusCheckRequest input)
        {
            TransferResponse mainResponse = new TransferResponse();
            mainResponse.Status = Message.PROCESS;
            mainResponse.Message = "";
            DMTBAL bal = new DMTBAL();

            input.Channel = "1";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;     
            return bal.GetTransctionStatus(input);
        }

        public TransferResponse VerifyTransctionStatusCheck(VerifyStatusCheckRequest input)
        {
            TransferResponse mainResponse = new TransferResponse();
            mainResponse.Status = Message.PROCESS;
            mainResponse.Message = "";
            DMTBAL bal = new DMTBAL();
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;

            return bal.GetVerifyTransctionStatus(input);
        }

        public RefundTransferResponse SendOTPRefund(RefundOTPRequest input)
        {
            RefundTransferResponse response = new RefundTransferResponse();
            response.Status = Message.PROCESS;
            response.Message = "";
            DMTBAL bal = new DMTBAL();

            input.Narration = "REFUND TRANSFER";
            input.Channel = "1";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            response = bal.SendOTPRefund(input);
            return response;
        }

        public RefundTransferResponse VerifyOTPRefund(RefundTransactionRequest input)
        {
            RefundTransferResponse response = new RefundTransferResponse();
            response.Status = Message.PROCESS;
            response.Message = "";

            input.Narration = "REFUND TRANSFER";
            input.AgentDistrictCode = "600";
            input.AgentStateCode = "27";
            input.CustomerDistrictCode = "600";
            input.CustomerStateCode = "27";   
            input.PartnerDistrictCode = "600";
            input.PartnerStateCode = "27";
            input.Channel = "1";
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            DMTBAL bal = new DMTBAL();
            response = bal.VerifyOTPRefund(input);

            return response;
        }

        public RefundTransferResponse VerifyBeneSendOTPRefund(VerifyBeneRefundOTPRequest input)
        {
            RefundTransferResponse response = new RefundTransferResponse();
            response.Status = Message.PROCESS;
            response.Message = "";
            DMTBAL bal = new DMTBAL();

            input.Narration = "REFUND TRANSFER";          
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            response = bal.VerifyBeneSendOTPRefund(input);
            return response;
        }

        public RefundTransferResponse BeneVerifyOTPRefund(VerifyBeneRefundTransactionRequest input)
        {
            RefundTransferResponse response = new RefundTransferResponse();
            response.Status = Message.PROCESS;
            response.Message = "";

            input.Narration = "REFUND TRANSFER";        
            input.AgentId = MoneyTransferCredential.AGENTID;
            input.PartnerId = MoneyTransferCredential.PARTNERID;
            DMTBAL bal = new DMTBAL();
            response = bal.BeneVerifyOTPRefund(input);

            return response;
        }
    }
}
