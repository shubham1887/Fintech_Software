using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.Service
{
    public class DeleteServiceRequest
    {
        public int ServiceID { get; set; }
    }

    public class DeleteServiceRequestValidator : AbstractValidator<DeleteServiceRequest>
    {
        public DeleteServiceRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                .GreaterThan(0).WithMessage(ServiceMessages.InvalidValue);
        }
    }
}
