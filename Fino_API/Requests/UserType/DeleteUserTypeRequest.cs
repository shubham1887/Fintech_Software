using FluentValidation;

namespace FintechApp.Requests.UserType
{
    public class DeleteUserTypeRequest
    {
        public int UserTypeID { get; set; }
    }

    public class DeleteUserTypeRequestValidator : AbstractValidator<DeleteUserTypeRequest>
    {
        public DeleteUserTypeRequestValidator()
        {
            RuleFor(x => x.UserTypeID).GreaterThan(0);
        }
    }
}
