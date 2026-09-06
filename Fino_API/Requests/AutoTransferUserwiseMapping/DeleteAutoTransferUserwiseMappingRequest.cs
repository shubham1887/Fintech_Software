using FluentValidation;
namespace FintechApp.Requests.AutoTransferUserwiseMapping
{
    public class DeleteAutoTransferUserwiseMappingRequest { public int AutoTransferUserwiseMappingID { get; set; } }
    public class DeleteAutoTransferUserwiseMappingRequestValidator : AbstractValidator<DeleteAutoTransferUserwiseMappingRequest> { public DeleteAutoTransferUserwiseMappingRequestValidator() { RuleFor(x => x.AutoTransferUserwiseMappingID).GreaterThan(0); } }
}
