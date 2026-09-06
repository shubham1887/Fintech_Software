using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineOperatorMaster
{
    public class InsertRequest
    {
        private int _OfflineServiceMasterID;

        public int OfflineServiceMasterID
        {
            get => _OfflineServiceMasterID;
            set => _OfflineServiceMasterID = value;
        }

        private string _OfflineOperatorName;

        public string OfflineOperatorName
        {
            get => _OfflineOperatorName.TrimIfNotNull();
            set => _OfflineOperatorName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.OfflineServiceMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OfflineServiceMessages.InvalidOfflineServiceID);

            RuleFor(x => x.OfflineOperatorName)
                .Must(new Validation().IsValidString)
                .WithMessage(OfflineServiceMessages.InvalidOfflineOperatorName);
        }
    }
}