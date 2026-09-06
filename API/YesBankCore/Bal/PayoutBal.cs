using Newtonsoft.Json;
using YesBankCore.Controller;
using YesBankCore.Model;
using YESBankCore.Controller;
using YESBankCore.Model;

namespace YesBankCore.Bal
{
    public class PayoutBal
    {
        public Response.Payout.PayoutResponse MoneyTransfer(Request.Payout.FundTransferRequest request)
        {
            Response.Payout.PayoutResponse response = new Response.Payout.PayoutResponse();
            KYCCredentials cred = new KYCCredentials();
            string failureMessage = cred.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }
            failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = failureMessage;
                return response;
            }
            Request.Payout.FundTransferAPIRequest fundTransferAPIRequest = new Request.Payout.FundTransferAPIRequest();
            //fundTransferAPIRequest.requestNumber = request.RequestNumber;
            fundTransferAPIRequest.partnerReferenceNo = request.partnerReferenceNo;
            fundTransferAPIRequest.actionName = "VA_FUND_TRANSFER";
            fundTransferAPIRequest.p1 = request.AccountNumber;
            fundTransferAPIRequest.p2 = request.AccountIFSC;
            fundTransferAPIRequest.p3 = request.Mode;
            fundTransferAPIRequest.p4 = request.Amount;

            fundTransferAPIRequest.p5 = request.BeneficiaryName;

            fundTransferAPIRequest.partnerKey = KYCCredentials.PartnerKey;
            string requestJSON = JsonConvert.SerializeObject(fundTransferAPIRequest);
            SORHTTPProcessing http = new SORHTTPProcessing();
            YESBankCore.Response.NetworkResponse netwotk = http.Post(URL.getPayoutURL(URL.FundTransfer), requestJSON, ServiceName.Payout);
            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            Response.Payout.PayoutAPIResponse apiResponse = JsonConvert.DeserializeObject<Response.Payout.PayoutAPIResponse>(netwotk.Response);
            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMessage;
                return response;
            }

            if (apiResponse.status.EqualsIgnoreCase(Message.SUCCESS) && apiResponse.channelReferenceNumber != string.Empty)
            {
                response.Status = Message.SUCCESS;
                response.ReferenceNo = apiResponse.channelReferenceNumber;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            response.Status = Message.PROCESS;
            response.Message = apiResponse.responseMessage;
            return response;

        }

        public Response.Payout.PayoutResponse StatusCheck(Request.Payout.PayoutStatusCheckRequest request)
        {
            Response.Payout.PayoutResponse response = new Response.Payout.PayoutResponse();
            KYCCredentials cred = new KYCCredentials();
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }
            failureMessage = cred.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }
            Request.Payout.PayoutStatusCheckAPIRequest aPIRequest = new Request.Payout.PayoutStatusCheckAPIRequest();
            aPIRequest.requestId = request.RequestNumber;

            aPIRequest.actionName = "FUND_TRANSFER_STATUS";
            aPIRequest.partnerKey = KYCCredentials.PartnerKey;
            aPIRequest.p1 = request.PartnerReferenceNumber;
            string requestJSON = JsonConvert.SerializeObject(aPIRequest);
            SORHTTPProcessing http = new SORHTTPProcessing();
            YESBankCore.Response.NetworkResponse netwotk = http.Post(URL.getPayoutURL(URL.FundTransfer), requestJSON, ServiceName.Payout);
            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            Response.Payout.PayoutAPIResponse apiResponse = JsonConvert.DeserializeObject<Response.Payout.PayoutAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            if (apiResponse.txnStatus.EqualsIgnoreCase(Message.FAIL))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            if (apiResponse.txnStatus.EqualsIgnoreCase(Message.SUCCESS) && apiResponse.channelReferenceNumber != string.Empty)
            {
                response.Status = Message.SUCCESS;
                response.ReferenceNo = apiResponse.channelReferenceNumber;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            response.Status = Message.PROCESS;
            response.Message = apiResponse.responseMessage ?? "";
            return response;


        }

        public Response.Payout.PayoutResponse Verification(Request.Payout.VerificationRequest request)
        {
            Response.Payout.PayoutResponse response = new Response.Payout.PayoutResponse();
            KYCCredentials cred = new KYCCredentials();
            string failureMessage = cred.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }
            failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }

            Request.Payout.VerificationAPIRequest aPIRequest = new Request.Payout.VerificationAPIRequest();
            aPIRequest.partnerReferenceNo = request.RequestNumber;
            aPIRequest.actionName = "VA_PENNY_LESS_ACC_VERIFICATION";
            aPIRequest.partnerKey = KYCCredentials.PartnerKey;
            aPIRequest.p1 = request.AccountNumber;
            aPIRequest.p2 = request.AccountIFSC;
            aPIRequest.p3 = request.RemitterName;
            aPIRequest.p4 = request.RemitterMobile;
            aPIRequest.p5 = request.Remarks;
            string requestJSON = JsonConvert.SerializeObject(aPIRequest);
            SORHTTPProcessing http = new SORHTTPProcessing();
            YESBankCore.Response.NetworkResponse netwotk = http.Post(URL.getPayoutURL(URL.FundTransfer), requestJSON, ServiceName.Payout);
            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            Response.Payout.PennyVerificationAPIResponse apiResponse = JsonConvert.DeserializeObject<Response.Payout.PennyVerificationAPIResponse>(netwotk.Response);


            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMessage;
                return response;
            }

            if (apiResponse.status.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Message = apiResponse?.responseMessage ?? "";
                response.BeneficiarName = apiResponse.beneficiarName;
                response.ReferenceNo = apiResponse.rrn;
                return response;
            }
            response.Status = Message.PROCESS;
            response.Message = apiResponse?.responseMessage ?? "";
            return response;
        }

        public Response.Payout.BalanceFetchResponse FetchBalance(Request.Payout.FetchBalanceRequest request)
        {
            Response.Payout.BalanceFetchResponse response = new Response.Payout.BalanceFetchResponse();
            KYCCredentials cred = new KYCCredentials();
            string failureMessage = cred.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }
            failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = failureMessage;
                return response;
            }
            Request.Payout.FetchBalanceAPIRequest aPIRequest = new Request.Payout.FetchBalanceAPIRequest();
            aPIRequest.requestNumber = request.RequestNumber;
            aPIRequest.actionName = "FETCH_VA_BALANCE";
            aPIRequest.partnerKey = KYCCredentials.PartnerKey;
            string requestJSON = JsonConvert.SerializeObject(aPIRequest);
            SORHTTPProcessing http = new SORHTTPProcessing();
            YESBankCore.Response.NetworkResponse netwotk = http.Post(URL.getPayoutURL(URL.VirtalAccountBalance), requestJSON, ServiceName.Payout);
            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.PROCESS;
                response.Message = netwotk.Response;
                return response;
            }

            Response.Payout.BalanceFetchAPIResponse apiResponse = JsonConvert.DeserializeObject<Response.Payout.BalanceFetchAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.PROCESS;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            if (apiResponse.status.EqualsIgnoreCase(Message.SUCCESS))
            {
                response.Status = Message.SUCCESS;
                response.Message = apiResponse?.responseMessage ?? "";
                response.balance = apiResponse.balance;
                return response;
            }
            response.Status = Message.PROCESS;
            response.Message = apiResponse?.responseMessage ?? "";
            return response;
        }
    }
}
