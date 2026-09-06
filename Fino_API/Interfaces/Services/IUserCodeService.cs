using FintechApp.Requests.UserCode;
using FintechApp.Responses.UserCode;

namespace FintechApp.Interfaces.Services
{
    public interface IUserCodeService
    {
        /// <summary>Every user type with its configured code format and live sequence state.</summary>
        Task<List<UserCodeFormatResponse>> GetAll();

        Task<string> Save(SaveUserCodeFormatRequest request);

        Task<string> Delete(int userCodeFormatId);

        /// <summary>The code the next user of this type would get. Consumes nothing.</summary>
        Task<UserCodePreviewResponse> Preview(int userTypeId);

        /// <summary>
        /// Allocates and returns the next User Code for a type. Consumes a number — call it once,
        /// at the moment the user is actually created.
        /// </summary>
        Task<string> GenerateUserCode(int userTypeId);
    }
}
