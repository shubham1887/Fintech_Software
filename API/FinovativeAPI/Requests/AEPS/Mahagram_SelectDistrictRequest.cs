using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class Mahagram_SelectDistrictRequest
    {
        private string _StateID;

        public string StateID
        {
            get => _StateID.TrimIfNotNull();
            set => _StateID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Mahagram_SelectDistrictRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Mahagram_SelectDistrictRequestValidator : AbstractValidator<Mahagram_SelectDistrictRequest>
    {
        public Mahagram_SelectDistrictRequestValidator()
        {
            RuleFor(x => x.StateID)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidID);
        }
    }
}