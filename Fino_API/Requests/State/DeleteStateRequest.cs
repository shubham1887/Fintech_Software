using FluentValidation;

namespace FintechApp.Requests.State
{
    public class DeleteStateRequest
    {
        public int StateID { get; set; }
    }

    public class DeleteStateRequestValidator : AbstractValidator<DeleteStateRequest>
    {
        public DeleteStateRequestValidator()
        {
            RuleFor(x => x.StateID).GreaterThan(0);
        }
    }
}
