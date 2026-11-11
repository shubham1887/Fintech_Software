using FintechApp.Requests.ResponseMessageRule;
using FintechApp.Responses.ResponseMessageRule;

namespace FintechApp.Interfaces.Services
{
    public interface IResponseMessageRuleService
    {
        Task<List<ResponseMessageRuleResponse>> GetAll();
        Task<ResponseMessageRuleResponse?> GetByID(int id);
        Task<string> Create(CreateResponseMessageRuleRequest request);
        Task<string> Update(UpdateResponseMessageRuleRequest request);
        Task<string> Delete(int id);

        /// <summary>
        /// The one call every user-facing message goes through. Returns the message to send: the
        /// first matching rule's replacement, or the original text when no rule matches. Never
        /// throws — a rewrite problem must not turn a successful transaction into an error.
        /// </summary>
        Task<string> Apply(string message, int providerID, int serviceID, string channel);

        /// <summary>Dry-run for the admin screen's Test box.</summary>
        Task<TestResponseMessageResponse> Test(TestResponseMessageRequest request);
    }
}
