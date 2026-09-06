using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CircleWiseCommission
{
    public class SelectRequest
    {
        private int _SlabMasterID;

        public int SlabMasterID
        {
            get => _SlabMasterID;
            set => _SlabMasterID = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
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
            RuleFor(x => x.SlabMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}