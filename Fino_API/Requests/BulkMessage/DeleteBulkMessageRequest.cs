using FluentValidation;
namespace FintechApp.Requests.BulkMessage
{
    public class DeleteBulkMessageRequest { public int BulkMessageID { get; set; } }
    public class DeleteBulkMessageRequestValidator : AbstractValidator<DeleteBulkMessageRequest> { public DeleteBulkMessageRequestValidator() { RuleFor(x => x.BulkMessageID).GreaterThan(0); } }
}
