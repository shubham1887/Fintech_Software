using FluentValidation;
namespace FintechApp.Requests.AutoTransferUserwiseMapping
{
    public class GetAutoTransferUserwiseMappingRequest { public int AutoTransferUserwiseMappingID { get; set; } public int UserID { get; set; } }
    public class GetAutoTransferUserwiseMappingRequestValidator : AbstractValidator<GetAutoTransferUserwiseMappingRequest> { public GetAutoTransferUserwiseMappingRequestValidator() { RuleFor(x => x.AutoTransferUserwiseMappingID).GreaterThan(0); } }
}
