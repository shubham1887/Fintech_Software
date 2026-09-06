using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.LapuMaster
{
    public class DeleteRequest
    {
        private int _LapuMasterID;

        public int LapuMasterID
        {
            get => _LapuMasterID;
            set => _LapuMasterID = value;
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
            RuleFor(x => x.LapuMasterID)
                          .Must(new Validation().IsValidInt)
                          .WithMessage(Message.LapuMasterMessage.InvalidLapu);
        }
    }
}