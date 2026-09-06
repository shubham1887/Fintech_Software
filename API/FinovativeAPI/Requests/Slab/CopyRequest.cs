using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Slab
{
    public class CopyRequest
    {
        private int _CopySlabID;

        public int CopySlabID
        {
            get => _CopySlabID;
            set => _CopySlabID = value;
        }

        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new CopyRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            if (!success) return failures[0].ToString();

            return StatusType.Ok;
        }
    }

    internal class CopyRequestValidator : AbstractValidator<CopyRequest>
    {
        public CopyRequestValidator()
        {
            RuleFor(x => x.CopySlabID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.NotFound);

            RuleFor(x => x.SlabName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}