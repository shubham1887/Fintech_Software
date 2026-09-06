using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.DMTBank
{
    public class ActiveDeactiveRequest
    {
        private int _DMTBankID;

        public int DMTBankID
        {
            get => _DMTBankID;
            set => _DMTBankID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsIMPS;

        public bool IsIMPS
        {
            get => _IsIMPS;
            set => _IsIMPS = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ActiveDeactiveRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"DMTBank Activation {IsActive}";
        }
    }

    internal class ActiveDeactiveRequestValidator : AbstractValidator<ActiveDeactiveRequest>
    {
        public ActiveDeactiveRequestValidator()
        {
            RuleFor(x => x.DMTBankID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}