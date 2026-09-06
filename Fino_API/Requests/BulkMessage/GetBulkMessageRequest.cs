using FluentValidation;
namespace FintechApp.Requests.BulkMessage
{
    public class GetBulkMessageRequest { public int BulkMessageID { get; set; }
    public int UserID { get; set; } }
    public class GetBulkMessageRequestValidator : AbstractValidator<GetBulkMessageRequest> { public GetBulkMessageRequestValidator() { RuleFor(x => x.BulkMessageID).GreaterThan(0); } }
}
