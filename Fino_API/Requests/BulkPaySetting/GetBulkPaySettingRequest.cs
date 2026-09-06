using FluentValidation;
namespace FintechApp.Requests.BulkPaySetting { public class GetBulkPaySettingRequest { public int BulkPaySettingID { get; set; } } public class GetBulkPaySettingRequestValidator : AbstractValidator<GetBulkPaySettingRequest> { public GetBulkPaySettingRequestValidator() { RuleFor(x => x.BulkPaySettingID).GreaterThan(0); } } }
