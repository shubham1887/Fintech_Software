using FluentValidation;
namespace FintechApp.Requests.EnquiryDetails
{
    public class GetEnquiryDetailsRequest { public int EnquiryDetailsID { get; set; } }
    public class GetEnquiryDetailsRequestValidator : AbstractValidator<GetEnquiryDetailsRequest> { public GetEnquiryDetailsRequestValidator() { RuleFor(x => x.EnquiryDetailsID).GreaterThan(0); } }
}
