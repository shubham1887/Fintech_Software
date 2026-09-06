using FluentValidation;
namespace FintechApp.Requests.EnquiryDetails
{
    public class DeleteEnquiryDetailsRequest { public int EnquiryDetailsID { get; set; } }
    public class DeleteEnquiryDetailsRequestValidator : AbstractValidator<DeleteEnquiryDetailsRequest> { public DeleteEnquiryDetailsRequestValidator() { RuleFor(x => x.EnquiryDetailsID).GreaterThan(0); } }
}
