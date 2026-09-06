using FluentValidation;

namespace FintechApp.Requests.UserType
{
    public class CreateUserTypeRequest
    {
        public string UserType { get; set; } = string.Empty;
        public string AliasName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateUserTypeRequestValidator : AbstractValidator<CreateUserTypeRequest>
    {
        public CreateUserTypeRequestValidator()
        {
            RuleFor(x => x.UserType).NotEmpty().MaximumLength(100);
            RuleFor(x => x.AliasName).MaximumLength(10);
        }
    }
}
