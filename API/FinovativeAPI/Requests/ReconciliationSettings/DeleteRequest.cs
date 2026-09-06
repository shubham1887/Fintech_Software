using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ReconciliationSettings
{
    public class DeleteRequest
    {
        private int _ReconciliationSettingID;

        public int ReconciliationSettingID
        {
            get => _ReconciliationSettingID;
            set => _ReconciliationSettingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.ReconciliationSettingID)
                          .Must(new Validation().IsValidInt)
                          .WithMessage(Message.ReconciliationSettingMesages.InvalidReconcilation);
        }
    }
}