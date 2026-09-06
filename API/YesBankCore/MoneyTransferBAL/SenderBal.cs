using Newtonsoft.Json;
using YesBankCore.Controller;
using YesBankCore.Request.MoneyTransfer;
using YesBankCore.Response.MoneyTransfer;
using YESBankCore.Controller;
using YESBankCore.Model;
using YESBankCore.Response;

namespace YesBankCore.MoneyTransferBAL
{
    internal class SenderBal
    {
        public SenderSelectResponse SelectSenderDetail(SenderSelectRequest input)
        {
            SenderSelectResponse response = new SenderSelectResponse();

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

            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

           string HashKey = Enc.CreateMD5(PlainHashKeyText);

            SenderSelectAPIRequest request = new SenderSelectAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.productCode = input.ProductCode;
            request.channel = input.Channel;
            request.isOTPValidated = "0";
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.SelectSender), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            SelectSenderAPIResponse apiResponse = JsonConvert.DeserializeObject<SelectSenderAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMsg;
                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = apiResponse.responseMsg;
                response.Name = apiResponse.data?.name ?? "";
                response.LastName = apiResponse.data?.custLastName ?? "";
                response.RemainingLimit = apiResponse.data?.balance[0]?.monthlyAvailLimit ?? "";
                response.ComsumedLimit = apiResponse.data?.balance[0]?.monthlyAvailTxnCount ?? "";
                return response;
            }

            response.Message = apiResponse.errorResponse[0].errorMsg;
            response.Status = Message.PROCESS;
            return response;
        }

        public SenderAddResponse SenderRegistration(SenderAddRequest input)
        {
            SenderAddResponse response = new SenderAddResponse();
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
         
            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob + input.KycStatus ;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            SenderAddAPIRequest request = new SenderAddAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.name = input.Name;
            request.address = input.Address;
            request.dateOfBirth = input.DateOfBirth;
            request.isOTPValidated = input.IsOTPValidated;
            request.productCode = input.ProductCode;
            request.pancardStatus = input.PancardStatus;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.gender = input.Gender;
            request.title = input.Title;
            request.pincode = input.Pincode;
            request.district = input.District;
            request.state = input.State;
            request.kycStatus = input.KycStatus;
            request.cardStatus = input.CardStatus;
            request.city = input.City;
            request.channel = input.Channel;
            request.versionNo = input.VersionNo;
            request.maritalStatus = input.MaritalStatus;
            request.identityDocNo = input.IdentityDocNo;
            request.identityDocType = input.IdentityDocType;
            request.addressDocNo = input.AddressDocNo;
            request.addressDocType = input.AddressDocType;       
            request.maritalStatus = input.MaritalStatus;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            MoneyTransferURL url = new MoneyTransferURL();

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.SenderRegistration), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            SenderAddAPIResponse res = JsonConvert.DeserializeObject<SenderAddAPIResponse>(netwotk.Response);
            
            if (res == null)
            {
                response.Status = Message.FAILURE;
                response.Message = res.responseMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = res.responseMsg;
                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.errorResponse[0].errorMsg;
            return response;
        }
    }
}
