using FintechApp.Requests.Verification;
using FintechApp.Responses.Verification;

namespace FintechApp.Interfaces.Services
{
    public interface IVerificationDocumentService
    {
        // Admin configuration.
        Task<List<VerificationDocumentResponse>> GetAll();
        Task<string> Create(CreateVerificationDocumentRequest request, int currentUserID);
        Task<string> Update(UpdateVerificationDocumentRequest request, int currentUserID);
        Task<string> Delete(int id);

        // User manual verification.
        Task<List<VerificationDocumentResponse>> GetRequiredDocuments(int currentUserID);
        Task<string> SubmitDocument(SubmitVerificationDocumentRequest request, int currentUserID);
    }
}
