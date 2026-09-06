using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.BBPSConfiguration
{
    public class GetBBPSConfigurationRequest
    {
        public int BBPSConfigurationID { get; set; }
        public int OperatorID { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetBBPSConfigurationRequestValidator : AbstractValidator<GetBBPSConfigurationRequest>
    {
        public GetBBPSConfigurationRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage(BBPSConfigurationMessages.InvalidValue);
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage(BBPSConfigurationMessages.InvalidValue);
        }
    }
}