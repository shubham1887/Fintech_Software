using Newtonsoft.Json;
using YesBankCore.Controller;
using YesBankCore.Request.MoneyTransfer;
using YesBankCore.Response.MoneyTransfer;
using YESBankCore.Controller;
using YESBankCore.Model;
using YESBankCore.Response;

namespace YesBankCore.MoneyTransferBAL
{
    internal class BeneficiaryBAL
    {
        public AddBeneOTPResponse AddBeneficiary(BeneAddSendOTPRequest input)
        {
            AddBeneOTPResponse response = new AddBeneOTPResponse();
            MoneyTransferCredential cred = new MoneyTransferCredential();
            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            BeneAddSendOTPAPIRequest request = new BeneAddSendOTPAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.benefAccNo = input.AccountNo;
            request.channel = input.Channel;
            request.name = input.Name;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);
            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();

            MoneyTransferURL url = new MoneyTransferURL();
            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.GenerateOTP),requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            AddBeneOTPAPIResponse res = JsonConvert.DeserializeObject<AddBeneOTPAPIResponse>(netwotk.Response);
            if (res == null)
            {
                response.Status = Message.FAILURE;
                response.Message = res.responseMsg;
                return response;
            }
            if (res.responseMsg.Length<=0)
            {
                response.Status = Message.PROCESS;
                response.Message = Message.NotFound;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = res.responseMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg?? "";
                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.responseMsg;
            return response;
        }

        public AddBeneResponse ValidateBeneficiaryOTP(BeneAddRequest input)
        {
            AddBeneResponse response = new AddBeneResponse();
            MoneyTransferCredential cred = new MoneyTransferCredential();
            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            BeneAddAPIRequest request = new BeneAddAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.benefAccNo = input.BenefAccNo;
            request.benefIfscCode = input.IfscCode;
            request.pincode = input.PinCode;
            request.channel = input.Channel;
            request.productCode = input.ProductCode;
            request.country = input.Country;
            request.address1 = input.Address;
            request.title = input.Title;
            request.city = input.City;
            request.name = input.Name;
            request.otp = input.Otp;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);
            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();
            MoneyTransferURL url = new MoneyTransferURL();
            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.AddBene), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            AddBeneAPIResponse res = JsonConvert.DeserializeObject<AddBeneAPIResponse>(netwotk.Response);
            if (res == null)
            {
                response.Status = Message.FAILURE;
                response.Message = res.responseMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = res.responseMsg;
                response.BeneficiaryID = res.data?.masterBeneficiaryId ?? "";
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg ?? "";
                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.responseMsg;
            response.BeneficiaryID = res.data?.masterBeneficiaryId ?? "";
            return response;
        }

        public DeActivateBeneResponse DeleteBeneficiary(BeneDeactivationRequest input)
        {
            DeActivateBeneResponse response = new DeActivateBeneResponse();
            MoneyTransferCredential cred = new MoneyTransferCredential();
            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            BeneDeactivationAPIRequest request = new BeneDeactivationAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId =input.PartnerId;
            request.benefAccNo =input.AccountNo;
            request.benefIfscCode =input.IfscCode;
            request.productCode = input.ProductCode;
            request.channel = input.Channel;
            request.status = input.Status;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);
            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();
            MoneyTransferURL url = new MoneyTransferURL();
            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.DeleteBene), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            DeactivateBeneAPIResponse res = JsonConvert.DeserializeObject<DeactivateBeneAPIResponse>(netwotk.Response);
            if (res == null)
            {
                response.Status = Message.FAILURE;
                response.Message = res.responseMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = res.responseMsg ?? "";
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = res.responseMsg ?? "";
                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.responseMsg ?? "";
            return response;
        }

        public BeneListResponse GetAllBeneficiary(BeneListRequest input)
        {
            BeneListResponse response = new BeneListResponse();
            MoneyTransferCredential cred = new MoneyTransferCredential();
            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            BeneListAPIRequest request = new BeneListAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.channel = input.Channel;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);
            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();
            MoneyTransferURL url = new MoneyTransferURL();
            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.GetAllBene), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            BeneListAPIResponse res = JsonConvert.DeserializeObject<BeneListAPIResponse>(netwotk.Response);
            if (res == null)
            {
                response.Status = Message.FAILURE;
                response.Message = Message.FAILURE;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = Message.SUCCESS;
                response.beneficiaryDetails = res.data?.beneficiaryDetails;
                return response;
            }

            response.Message = res.errorResponse[0].errorMsg ?? "";
            return response;
        }

    }
}
