using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineOperatorMaster
{
    public class SelectByServiceIDRequest
    {
        private int _OfflineServiceMasterID;

        public int OfflineServiceMasterID
        {
            get => _OfflineServiceMasterID;
            set => _OfflineServiceMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByServiceIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectByServiceIDRequestValidator : AbstractValidator<SelectByServiceIDRequest>
    {
        public SelectByServiceIDRequestValidator()
        {
            RuleFor(x => x.OfflineServiceMasterID)
              .Must(new Validation().IsValidpositive)
              .WithMessage(Message.OfflineServiceMessages.InvalidOfflineServiceID);
        }
    }
}