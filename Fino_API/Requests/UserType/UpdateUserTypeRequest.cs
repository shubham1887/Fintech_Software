using FluentValidation;

namespace FintechApp.Requests.UserType
{
    public class UpdateUserTypeRequest
    {
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string AliasName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateUserTypeRequestValidator : AbstractValidator<UpdateUserTypeRequest>
    {
        public UpdateUserTypeRequestValidator()
        {
            RuleFor(x => x.UserTypeID).GreaterThan(0);
            RuleFor(x => x.UserType).NotEmpty().MaximumLength(100);
            RuleFor(x => x.AliasName).MaximumLength(10);
        }
    }
}
