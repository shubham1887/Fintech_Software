using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.Service
{
    public class GetServiceRequest
    {
        public int ServiceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        // Null = no active/inactive filter (admin lists show OFF services too).
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetServiceRequestValidator : AbstractValidator<GetServiceRequest>
    {
        public GetServiceRequestValidator()
        {
            // PageNumber
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage(ServiceMessages.InvalidValue);

            // PageSize
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage(ServiceMessages.InvalidValue);

            // Name
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage(ServiceMessages.InvalidLength);

            // ServiceCategory
            RuleFor(x => x.ServiceCategory)
                .MaximumLength(50).WithMessage(ServiceMessages.InvalidLength);
        }
    }
}