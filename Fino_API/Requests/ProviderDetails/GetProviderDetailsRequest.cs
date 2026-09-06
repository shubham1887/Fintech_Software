using FluentValidation;

namespace FintechApp.Requests.ProviderDetails
{
    public class GetProviderDetailsRequest
    {
        public int ProviderDetailID { get; set; }
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
    }

    public class GetProviderDetailsRequestValidator : AbstractValidator<GetProviderDetailsRequest>
    {
        public GetProviderDetailsRequestValidator() { }
    }
}
