using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.DenominationMaster
{
    public class SelectRequest
    {
        private int _DenominationMasterID;

        public int DenominationMasterID
        {
            get => _DenominationMasterID;
            set => _DenominationMasterID = value;
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
            RuleFor(x => x.DenominationMasterID)
                          .Must(new Validation().IsValidInt)
                          .WithMessage(Project.Message.DenominationMasterMessage.Invaliddenomination);
        }
    }
}