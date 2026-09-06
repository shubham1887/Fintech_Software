using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.UISetting
{
    public class GetUISettingRequest
    {
        public int UISettingMasterID { get; set; }
        public string Key { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetUISettingRequestValidator : AbstractValidator<GetUISettingRequest>
    {
        public GetUISettingRequestValidator()
        {
            // PageNumber
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage(UISettingMessages.InvalidValue);

            // PageSize
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage(UISettingMessages.InvalidValue);

            // Key
            RuleFor(x => x.Key)
                .MaximumLength(100).WithMessage(UISettingMessages.InvalidLength);
        }
    }
}