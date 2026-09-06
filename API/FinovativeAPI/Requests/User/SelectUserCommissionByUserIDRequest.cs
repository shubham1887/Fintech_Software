using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.User
{
    public class SelectUserCommissionByUserIDRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID; set => _UserID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectUserCommissionByUserIDRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class SelectUserCommissionByUserIDRequestValidator : AbstractValidator<SelectUserCommissionByUserIDRequest>
    {
        public SelectUserCommissionByUserIDRequestValidator()
        {
            RuleFor(x => x.UserID).Must(new Validation().IsValidInt).WithMessage(UserMessage.InvalidUser);
        }
    }
}