using FintechApp.Requests.SignUp;
using FintechApp.Responses.SignUp;
namespace FintechApp.Interfaces.Services
{
    public interface ISignUpService
    {
        Task<List<SignUpResponse>> GetAll();
        Task<SignUpResponse?> GetByID(int id);

        Task<string> Create(CreateSignUpRequest request);
        Task<string> Update(UpdateSignUpRequest request);
        Task<string> Delete(int id);
    }
}
