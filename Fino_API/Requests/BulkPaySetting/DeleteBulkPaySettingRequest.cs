using FluentValidation;
namespace FintechApp.Requests.BulkPaySetting { public class DeleteBulkPaySettingRequest { public int BulkPaySettingID { get; set; } } public class DeleteBulkPaySettingRequestValidator : AbstractValidator<DeleteBulkPaySettingRequest> { public DeleteBulkPaySettingRequestValidator() { RuleFor(x => x.BulkPaySettingID).GreaterThan(0); } } }
