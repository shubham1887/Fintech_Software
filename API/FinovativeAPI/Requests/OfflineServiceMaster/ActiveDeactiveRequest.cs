using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineServiceMaster
{
    public class ActiveDeactiveRequest
    {
        private int _OfflineServiceMasterID;

        public int OfflineServiceMasterID
        {
            get => _OfflineServiceMasterID;
            set => _OfflineServiceMasterID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ActiveDeactiveRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ActiveDeactiveRequestValidator : AbstractValidator<ActiveDeactiveRequest>
    {
        public ActiveDeactiveRequestValidator()
        {
            RuleFor(x => x.OfflineServiceMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OfflineServiceMessages.InvalidOfflineServiceID);
        }
    }
}