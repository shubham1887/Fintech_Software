using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SliderMaster
{
    public class DeleteRequest
    {
        private int _SliderMasterID;

        public int SliderMasterID
        {
            get => _SliderMasterID;
            set => _SliderMasterID = value;
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
            RuleFor(x => x.SliderMasterID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.SliderMasterMessages.InvalidSliderMasterID);
        }
    }
}