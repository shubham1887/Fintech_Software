using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.User
{
    public class UpdateUserCommissionRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private decimal _Comm;

        public decimal Comm
        {
            get => _Comm;
            set => _Comm = value;
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(StaticModel.CommissionCategory.Commission);

            set => _CommissionCategory = value.TrimIfNotNullWithDefault(StaticModel.CommissionCategory.Commission);
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateUserCommissionRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class UpdateUserCommissionRequestValidator : AbstractValidator<UpdateUserCommissionRequest>
    {
        public UpdateUserCommissionRequestValidator()
        {
            RuleFor(x => x.UserID).Must(new Validation().IsValidInt).WithMessage(UserMessage.InvalidUser);

            RuleFor(x => x.Comm).Must(IsValidCommissionAmount).WithMessage(SlabDetailMessage.InvalidAmout);

            // RuleFor(x => x.Comm).Must(new Validation().IsValidDecimal).WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.CommissionCategory).Must(new Validation().IsValidString).WithMessage(SlabDetailMessage.InvalidCommissionCategory);
        }

        private bool IsValidCommissionAmount(decimal amt)
        {
            if (amt < -5 || amt > 5) return false;
            return true;
        }
    }
}