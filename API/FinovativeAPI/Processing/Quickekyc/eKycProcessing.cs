using Newtonsoft.Json;
using Project.BAL;
using Project.Requests.Quickekyc;
using Project.Response.Quickekyc;
using Project.StaticModel;

namespace Project.Processing.Quickekyc
{
    public class eKycProcessing
    {
         
        internal AadharGenerateOtpResponse Quick_AadharGenerateOtp(string Key, string aadhaarNo)
        {
            VerificationRequest request = new VerificationRequest
            {
                key = Key,
                id_number = aadhaarNo
            };

            AadharGenerateOtpResponse response = new AadharGenerateOtpResponse();
            string requestjson = JsonConvert.SerializeObject(request);

            HTTPProcessing http = new HTTPProcessing();
            NetworkResponse netwotk = http.POST(eKycUrl.QuickGenerateOtpUrl, requestjson);
            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = netwotk.Response;
                return response;
            }
            AadharGenerateOtpAPIResponse apiResponse = JsonConvert.DeserializeObject<AadharGenerateOtpAPIResponse>(netwotk.Response);
            if (!apiResponse.status.EqualsIgnoreCase(StatusType.Success))
            {
                response.Status = StatusType.Failure;
                response.Message = apiResponse.message;
                return response;
            }
            response.Status = StatusType.Success;
            response.ClientID = apiResponse.request_id.ToString();
            response.Message = apiResponse.message;
            return response;
        }

        internal AadharVerifyOtpResponse Quick_AadharVerifyOtp(string Key, string RequestID, string otp)
        {
            AadharOtpVerifyRequest request = new AadharOtpVerifyRequest
            {
                key = Key,
                request_id = RequestID,
                otp = otp
            };
            AadharVerifyOtpResponse response = new AadharVerifyOtpResponse();
            string requestjson = JsonConvert.SerializeObject(request);

            HTTPProcessing http = new HTTPProcessing();
            NetworkResponse netwotk = http.POST(eKycUrl.QuickVerifyOtpUrl, requestjson);
            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = netwotk.Response;
                return response;
            }
            AadharVerifyOtpAPIResponse apiResponse = JsonConvert.DeserializeObject<AadharVerifyOtpAPIResponse>(netwotk.Response);
            if (!apiResponse.status.EqualsIgnoreCase(StatusType.Success))
            {
                response.Status = StatusType.Failure;
                response.Message = apiResponse.message;
                return response;
            }
            response.Status = StatusType.Success;
            response.Message = apiResponse.message;
            response.FullName = apiResponse.data.full_name;
            response.Address = AdressDetails(apiResponse);
            response.City = apiResponse.data.address.subdist;
            response.Dob = apiResponse.data.dob;
            response.AadhaarNumber = apiResponse.data.aadhaar_number;
            response.Pin = apiResponse.data.zip;
            response.ProfileImage = apiResponse.data.profile_image;
            response.State = apiResponse.data.address.state;
            response.Gender = apiResponse.data.gender;
            string reqStr = JsonConvert.SerializeObject(request);
            string resStr = JsonConvert.SerializeObject(response);
            new eKYCMasterBal().Update(RequestID, reqStr, resStr);

            return response;
        }

        private string AdressDetails(AadharVerifyOtpAPIResponse apiResponse)
        {
            string address = $"{apiResponse?.data?.address?.house ?? ""}, {apiResponse?.data?.address?.street ?? ""}, {apiResponse?.data?.address?.landmark ?? ""}" +
                  $", {apiResponse?.data?.address?.vtc ?? ""}, {apiResponse?.data?.address?.subdist ?? ""}, {apiResponse?.data?.address?.loc ?? ""}, {apiResponse?.data?.address?.dist ?? ""}, {apiResponse?.data?.address?.state ?? ""}, {apiResponse?.data?.address?.country ?? ""}";

            return address;
        }

        internal UPIVerificationResponse Quick_UPIVerification(string Key, string UPINumber)
        {
            UPIVerificationRequest request = new UPIVerificationRequest
            {
                key = Key,
                upi_id = UPINumber
            };

            UPIVerificationResponse response = new UPIVerificationResponse();
            string requestjson = JsonConvert.SerializeObject(request);

            HTTPProcessing http = new HTTPProcessing();
            NetworkResponse netwotk = http.POST(eKycUrl.QuickUPIUrl, requestjson);
            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = netwotk.Response;
                return response;
            }
            UPIVerificationAPIResponse apiResponse = JsonConvert.DeserializeObject<UPIVerificationAPIResponse>(netwotk.Response);
            if (!apiResponse.status.EqualsIgnoreCase(StatusType.Success))
            {
                response.Status = StatusType.Failure;
                response.Message = apiResponse.message;
                return response;
            }

            response.Status = StatusType.Success;
            response.Message = apiResponse.message;
            response.request_id = apiResponse.request_id;
            response.full_name = apiResponse.data.name_at_bank;    
            response.account_exists = apiResponse.data.account_exists;    

            return response;
        }

    }
}