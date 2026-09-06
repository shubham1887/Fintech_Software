using FluentValidation;
namespace FintechApp.Requests.EnquiryDetails
{
    public class CreateEnquiryDetailsRequest { public string PersonName { get; set; } = string.Empty; public string MobileNumber { get; set; } = string.Empty; public string EmailID { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string EnquiryType { get; set; } = string.Empty; public string Remark { get; set; } = string.Empty; public string AdminStatus { get; set; } = string.Empty; public string AdminRemark { get; set; } = string.Empty; }
    public class CreateEnquiryDetailsRequestValidator : AbstractValidator<CreateEnquiryDetailsRequest> { public CreateEnquiryDetailsRequestValidator() {  } }
}
