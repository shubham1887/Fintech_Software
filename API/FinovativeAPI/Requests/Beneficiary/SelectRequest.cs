using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Beneficiary
{
    public class SelectRequest
    {
        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }


        private string _DMTName;

        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.SenderID)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidSender);
        }
    }
}