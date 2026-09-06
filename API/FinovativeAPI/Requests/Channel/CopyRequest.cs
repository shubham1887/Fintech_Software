using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class CopyRequest
    {
        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

        private int _CopySlabID;

        public int CopySlabID
        {
            get => _CopySlabID;
            set => _CopySlabID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new CopyRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CopyRequestValidator : AbstractValidator<CopyRequest>
    {
        public CopyRequestValidator()
        {
            RuleFor(x => x.SlabName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}