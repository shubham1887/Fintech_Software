using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.DenominationMaster
{
    public class UpdateRequest
    {
        private int _DenominationMasterID;

        public int DenominationMasterID
        {
            get => _DenominationMasterID;
            set => _DenominationMasterID = value;
        }

        private string _DenominationRange;

        public string DenominationRange
        {
            get => _DenominationRange.TrimIfNotNull();
            set => _DenominationRange = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private bool _IsStop;
        public bool IsStop { get => _IsStop; set => _IsStop = value; }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
        {
            public UpdateRequestValidator()
            {
                RuleFor(x => x.DenominationRange)
                      .Must(new Validation().IsValidString)
                      .WithMessage(Message.DenominationMasterMessage.Invaliddenomination);
                RuleFor(x => x.Description)
                             .Must(new Validation().IsValidString)
                             .WithMessage(Message.DenominationMasterMessage.InvalidDescription);
            }
        }
    }
}