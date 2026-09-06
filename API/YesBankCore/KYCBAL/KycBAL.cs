using Newtonsoft.Json;
using YesBankCore.Controller;
using YesBankCore.Model;
using YesBankCore.Request.KYC;
using YesBankCore.Response.KYC;
using YESBankCore.Controller;
using YESBankCore.Response;

namespace YesBankCore.KYCBAL
{
    internal class KycBAL
    {
        internal CreateAgentResponse AgentCreation(AgentCreationRequest request)
        {
            CreateAgentResponse response = new CreateAgentResponse();

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

            string requestJSON = JsonConvert.SerializeObject(request);

            SORHTTPProcessing http = new SORHTTPProcessing();

            NetworkResponse netwotk = http.Post(KYCURL.BaseURL, requestJSON, ServiceName.EKYC);
            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }
            CreateAgentAPIResponse apiResponse = JsonConvert.DeserializeObject<CreateAgentAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.FAILURE;
                return response;
            }

            if (apiResponse.status.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            if (apiResponse.status.EqualsIgnoreCase(Message.ERROR))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            response.Status = Message.SUCCESS;
            response.RequestID = apiResponse.requestid;
            response.OTPToken = apiResponse.otpToken;
            response.Message = apiResponse.responseMessage;
            return response;
        }

        internal OTPValidateResponse VerifyOTP(OTPValidateRequest request)
        {
            OTPValidateResponse response = new OTPValidateResponse();

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

            string requestJSON = JsonConvert.SerializeObject(request);

            SORHTTPProcessing http = new SORHTTPProcessing();

            NetworkResponse netwotk = http.Post(KYCURL.BaseURL, requestJSON, ServiceName.EKYC);
            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }

            OTPValidateAPIResponse apiResponse = JsonConvert.DeserializeObject<OTPValidateAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.FAILURE;
                return response;
            }

            if (apiResponse.status.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            if (apiResponse.status.EqualsIgnoreCase(Message.ERROR))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }

            response.Status = Message.SUCCESS;
            response.Message = apiResponse.responseMessage;
            response.KYCToken = apiResponse.kycToken;
            return response;
        }

        internal GetAadharWadhResponse AadharWadh(GetAadharWadhRequest request)
        {
            GetAadharWadhResponse response = new GetAadharWadhResponse();

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

            string requestJSON = JsonConvert.SerializeObject(request);

            SORHTTPProcessing http = new SORHTTPProcessing();

            NetworkResponse netwotk = http.Post(KYCURL.BaseURL, requestJSON, ServiceName.EKYC);
            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }
            GetAadharWadhAPIResponse apiResponse = JsonConvert.DeserializeObject<GetAadharWadhAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.FAILURE;
                return response;
            }

            if (apiResponse.status.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            if (apiResponse.status.EqualsIgnoreCase(Message.ERROR))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }

            response.Status = Message.SUCCESS;
            response.Message = apiResponse.responseMessage;
            response.Wadh = apiResponse.wadh;
            return response;
        }

        internal AadharBioMetricKYCResponse AadharBioMertic(AadharBiometricKYCRequest request)
        {
            AadharBioMetricKYCResponse response = new AadharBioMetricKYCResponse();

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

            string requestJSON = JsonConvert.SerializeObject(request);

            SORHTTPProcessing http = new SORHTTPProcessing();

            NetworkResponse netwotk = http.Post(KYCURL.BaseURL, requestJSON, ServiceName.EKYC);
            if (!netwotk.Status.EqualsIgnoreCase(Message.OK))
            {
                response.Status = Message.FAILURE;
                response.Message = netwotk.Response;
                return response;
            }
            AadharBioMetricKYCAPIResponse apiResponse = JsonConvert.DeserializeObject<AadharBioMetricKYCAPIResponse>(netwotk.Response);

            if (apiResponse == null)
            {
                response.Status = Message.FAILURE;
                return response;
            }

            if (apiResponse.status.EqualsIgnoreCase(Message.FAILURE))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }
            if (apiResponse.status.EqualsIgnoreCase(Message.ERROR))
            {
                response.Status = Message.FAILURE;
                response.Message = apiResponse.responseMessage;
                return response;
            }


            response.Status = Message.SUCCESS;
            response.Message = apiResponse.responseMessage;
            response.Token = apiResponse.uidToken;
            response.Name = apiResponse.aadhaarName;
            response.Address = apiResponse.address;
            return response;
        }
    }
}
