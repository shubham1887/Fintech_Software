using Newtonsoft.Json;
using YesBankCore.Controller;
using YesBankCore.Request.MoneyTransfer;
using YesBankCore.Response.MoneyTransfer;
using YESBankCore.Controller;
using YESBankCore.Model;
using YESBankCore.Response;

namespace YesBankCore.MoneyTransferBAL
{
    internal class DMTBAL
    {
        public TransferResponse VerifyBeneficiaryAccount(VerifyRequest input)
        {
            TransferResponse response = new TransferResponse();
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

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            VerifyAPIRequest request = new VerifyAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.beneAccountNo = input.AccountNo;
            request.beneIfsc = input.Ifsc;
            request.beneName = input.BeneName;
            request.isUpi = input.IsUpi;
            request.narration = input.Narration;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);
            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();
            MoneyTransferURL url = new MoneyTransferURL();

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURLForVerify(MoneyTransferURL.BeneAccVerify), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            VerifyAPIResponse res = JsonConvert.DeserializeObject<VerifyAPIResponse>(netwotk.Response);
            if (res.data == null)
            {
                response.Status = Message.PROCESS;
                response.Message = res.responseMsg;
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = res.errorResponse[0].errorMsg?? "";
                return response;
            }

            if (res.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = Message.SUCCESS;
                response.BeneName = res.data?.beneficiaryName ?? "";
                response.TransactionID = res.data?.rrn ?? "";
                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = res.errorResponse[0].errorMsg ?? "";
            response.BeneName = res.data?.beneficiaryName ?? "";
            return response;
        }

        public TransferResponse Transfer(TransferRequest input)
        {
            TransferResponse response = new TransferResponse();

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

            string PlainHashKeyText = input.TxnId + input.OrderId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob + input.ProductCode + input.Amount + ".0";

            EncryptDecrypt Enc = new EncryptDecrypt();
            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            TransferAPIRequest request = new TransferAPIRequest();
            request.txnId = input.TxnId;
            request.orderId = input.OrderId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.channel = input.Channel;
            request.productCode = input.ProductCode;
            request.purposeCode = input.PurposeCode;
            request.paymentMode = input.PaymentMode;
            request.amount = input.Amount;
            request.currencyCode = input.CurrencyCode;
            request.beneAccountNo = input.AccountNo;
            request.beneIfsc = input.Ifsc;
            request.beneMobileNo = input.MobileNo;
            request.beneEmailId = input.EmailId;
            request.beneName = input.Name;
            request.narration = input.Narration;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();
            MoneyTransferURL url = new MoneyTransferURL();

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.Transfer), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            TransferAPIResponse apiResponse = JsonConvert.DeserializeObject<TransferAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = apiResponse.responseMsg;
                response.ReferenceNumber = apiResponse.txnId;
                response.PaymentMode = apiResponse.data?.paymentMode ?? "";
                response.Date = apiResponse.data?.transactionDate ?? "";
                response.Amount = apiResponse.data?.transferAmount ?? "";
                response.TransactionID = apiResponse.data?.rrn ?? "";
                response.BeneName = apiResponse.data?.beneficiaryName ?? "";

                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.errorResponse[0].errorMsg?? "";

                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = apiResponse.responseMsg;
            response.ReferenceNumber = apiResponse.txnId;
            response.Amount = apiResponse.data?.transferAmount?? "";
            return response;
        }

        public TransferResponse GetTransctionStatus(StatusCheckRequest input)
        {
            TransferResponse response = new TransferResponse();

            MoneyTransferCredential cred = new MoneyTransferCredential();
            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            string PlainHashKeyText = input.TxnId + input.OrderId + input.PartnerId + input.AgentId + input.CustomerMob;
         
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            StatusCheckAPIRequest request = new StatusCheckAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.orgTxnId = input.OrgTxnId;
            request.orderId = input.OrderId;
            request.channel = input.Channel;
            request.fromDate = input.FromDate;
            request.toDate = input.ToDate;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            MoneyTransferURL url = new MoneyTransferURL();

            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.StatusCheck), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            StatusCheckAPIResponse apiResponse = JsonConvert.DeserializeObject<StatusCheckAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = apiResponse.responseMsg;
                response.TransactionID = apiResponse?.data.rrn ?? "";
                response.BeneName = apiResponse?.data.beneficiaryName ?? "";

                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.REJECTED))
            {
                response.Status = Message.REFUND;
                response.Message = apiResponse.responseMsg;
                response.TransactionID = apiResponse.data?.rrn ?? "";
                response.SortTransactionID = apiResponse.data?.sorTxnId ?? "";
                response.IsRefund = apiResponse.data?.isRefund ?? "";            
                
                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.errorResponse[0].errorMsg ?? "";

                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = apiResponse.errorResponse[0].errorMsg?? "";

            return response;
        }

        public TransferResponse GetVerifyTransctionStatus(VerifyStatusCheckRequest input)
        {
            TransferResponse response = new TransferResponse();

            MoneyTransferCredential cred = new MoneyTransferCredential();
            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.CustomerMob;
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            VerifyStatusAPIRequest request = new VerifyStatusAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.orgTxnId = input.OrgTxnId;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            MoneyTransferURL url = new MoneyTransferURL();

            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURLForVerify(MoneyTransferURL.VerifyStatusCheck), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            VerifyStatusCheckAPIResponse apiResponse = JsonConvert.DeserializeObject<VerifyStatusCheckAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = apiResponse.responseMsg;
                response.TransactionID = apiResponse?.data.rrn ?? "";
                response.BeneName = apiResponse?.data.beneficiaryName ?? "";
                response.IsRefund = apiResponse.data?.isRefund ?? "";

                if (response.IsRefund == "0")
                {
                    response.Status = Message.REFUND;
                }

                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.REJECTED))
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMsg;
                response.TransactionID = apiResponse.data?.rrn ?? "";

                return response;
            }

            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMsg;

                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = apiResponse.responseMsg;

            return response;
        }

        internal RefundTransferResponse SendOTPRefund(RefundOTPRequest input)
        {
            RefundTransferResponse response = new RefundTransferResponse();

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
            MoneyTransferURL url = new MoneyTransferURL();

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob;
            
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            TransferSendOTPAPIRequest request = new TransferSendOTPAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.orderId = input.OrderId;
            request.channel = input.Channel;
            request.sortxnId = input.SortxnId;
            request.narration = input.Narration;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.RefundOTP), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            TransferRefundOTPAPIResponse apiResponse = JsonConvert.DeserializeObject<TransferRefundOTPAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }
            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {          
                response.IsOTP = true;                
                response.Status = Message.SUCCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = apiResponse.responseMsg;
            return response;
        }

        internal RefundTransferResponse VerifyOTPRefund(RefundTransactionRequest input)
        {
            RefundTransferResponse response = new RefundTransferResponse();

            MoneyTransferCredential cred = new MoneyTransferCredential();
            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();
            MoneyTransferURL url = new MoneyTransferURL();

            string PlainHashKeyText = input.TxnId + input.OrderId + input.PartnerId + input.AgentId + input.Channel + input.CustomerMob + input.SortxnId;

            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            TransferVerifyOTPRefundAPIRequest request = new TransferVerifyOTPRefundAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.orderId = input.OrderId;
            request.channel = input.Channel;
            request.sortxnId = input.SortxnId;
            request.partnerStateCode = input.PartnerStateCode;
            request.partnerDistrictCode = input.PartnerDistrictCode;
            request.agentStateCode = input.AgentStateCode;
            request.agentDistrictCode = input.AgentDistrictCode;
            request.customerDistrictCode = input.CustomerDistrictCode;
            request.customerStateCode = input.CustomerStateCode;
            request.narration = input.Narration;
            request.otp = input.OTP;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURL(MoneyTransferURL.Refund), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            TransferRefundAPIResponse apiResponse = JsonConvert.DeserializeObject<TransferRefundAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }

            response.Status = Message.REFUNDED;
            response.Message = "Refunded successfully";

            response.Message = apiResponse.responseMsg;
            response.BeneName = apiResponse.data?.beneficiaryName?? "";
            response.TransactionID = apiResponse.data?.rrn?? "";

            return response;
        }

        internal RefundTransferResponse VerifyBeneSendOTPRefund(VerifyBeneRefundOTPRequest input)
        {
            RefundTransferResponse response = new RefundTransferResponse();

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
            MoneyTransferURL url = new MoneyTransferURL();

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.CustomerMob;
           
            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            VerifyBeneRefundAPIRequest request = new VerifyBeneRefundAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.orgTxnId = input.OrgTxnId;         
            request.narration = input.Narration;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURLForVerify(MoneyTransferURL.BeneVerifyRefundOTP), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            TransferRefundOTPAPIResponse apiResponse = JsonConvert.DeserializeObject<TransferRefundOTPAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }
            if (apiResponse.responseMsg.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.IsOTP = true;
                response.Status = Message.SUCCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }

            response.Status = Message.PROCESS;
            response.Message = apiResponse.responseMsg;
            return response;
        }

        internal RefundTransferResponse BeneVerifyOTPRefund(VerifyBeneRefundTransactionRequest input)
        {
            RefundTransferResponse response = new RefundTransferResponse();

            MoneyTransferCredential cred = new MoneyTransferCredential();
            string failureMessage = cred.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            failureMessage = input.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            DMTHTTPProcessingSOR http = new DMTHTTPProcessingSOR();
            MoneyTransferURL url = new MoneyTransferURL();

            string PlainHashKeyText = input.TxnId + input.PartnerId + input.AgentId + input.CustomerMob;

            EncryptDecrypt Enc = new EncryptDecrypt();

            string HashKey = Enc.CreateMD5(PlainHashKeyText);

            VerifyBeneRefundTransactionAPIRequest request = new VerifyBeneRefundTransactionAPIRequest();
            request.txnId = input.TxnId;
            request.customerMob = input.CustomerMob;
            request.agentId = input.AgentId;
            request.partnerId = input.PartnerId;
            request.orgTxnId = input.OrgTxnId;
            request.narration = input.Narration;
            request.otp = input.Otp;
            request.hashKey = HashKey;

            string requestJSON = JsonConvert.SerializeObject(request);

            NetworkResponse netwotk = http.PostForDMT(MoneyTransferURL.getURLForVerify(MoneyTransferURL.BeneVerifyRefund), requestJSON);

            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            TransferRefundAPIResponse apiResponse = JsonConvert.DeserializeObject<TransferRefundAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMsg;
                return response;
            }

            response.Status = Message.FAILURE;
            response.Message = "Refunded successfully";

            response.Message = apiResponse.responseMsg;
            response.BeneName = apiResponse.data?.beneficiaryName ?? "";
            response.TransactionID = apiResponse.data?.rrn ?? "";

            return response;
        }
    }
}
