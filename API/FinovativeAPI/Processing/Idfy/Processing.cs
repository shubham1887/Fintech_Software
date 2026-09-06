using Newtonsoft.Json;
using Project.Requests.SignUp;
using Project.Response.SignUp;
using Project.StaticModel;

namespace Project.Processing.Idfy
{
    public class Processing
    {
        internal CreateProfileResponse CreateProfile(CreateProfileAPIRequest request, string AuthKey, string Token, string MerchantID)
        {

            CreateProfileResponse response = new CreateProfileResponse();
            string requestjson = JsonConvert.SerializeObject(request);

            HTTPProcessing http = new HTTPProcessing();
            NetworkResponse netwotk = http.POST(eKycUrl.IdfyCreateProfileUrl, requestjson, AuthKey, Token);
            if (!netwotk.Status.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Status = StatusType.Failure;
                response.Message = netwotk.Response;
                return response;
            }
            CreateProfileAPIResponse apiResponse = JsonConvert.DeserializeObject<CreateProfileAPIResponse>(netwotk.Response);
            if (apiResponse == null)
            {
                response.Status = StatusType.Failure;
                response.Message = StatusType.Failure;
                return response;
            }

            if (apiResponse.message.EqualsIgnoreCase("Invalid authentication credentials"))
            {
                response.Status = StatusType.Failure;
                response.Message = apiResponse.message;
                return response;
            }
            if (apiResponse.error.Length > 2)
            {
                response.Status = StatusType.Failure;
                response.Message = apiResponse.error;
                return response;
            }
            if (apiResponse.capture_link.Length > 5)
            {
                response.Status = StatusType.Success;
                response.RedirectUrl = apiResponse.capture_link;
                response.Message = StatusType.Success;
                response.ProfileID = apiResponse.profile_id;
                return response;
            }
            response.Status = StatusType.Process;
            var message = apiResponse?.message ?? apiResponse?.error;
            response.Message = message ?? StatusType.Process;
            return response;
        }
    }
}
