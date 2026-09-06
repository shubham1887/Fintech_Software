using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineOperatorMaster
{
    public class UpdateRequest
    {
        private int _OfflineOperatorMasterID;

        public int OfflineOperatorMasterID
        {
            get => _OfflineOperatorMasterID;
            set => _OfflineOperatorMasterID = value;
        }

        private string _OfflineOperatorName;

        public string OfflineOperatorName
        {
            get => _OfflineOperatorName.TrimIfNotNull();
            set => _OfflineOperatorName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.OfflineOperatorMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OfflineOperatorMessages.InvalidOfflineOperatorID);

            RuleFor(x => x.OfflineOperatorName)
                .Must(new Validation().IsValidString)
                .WithMessage(OfflineServiceMessages.InvalidOfflineOperatorName);
        }
    }
}