using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class BulkCopyRequest
    {
        private int _SlabID;

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }

        private int _CopySlabID;

        public int CopySlabID
        {
            get => _CopySlabID;
            set => _CopySlabID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new BulkCopyRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (!success) ? failures[0].ToString() : StatusType.Ok;
        }
    }

    public class BulkCopyRequestValidator : AbstractValidator<BulkCopyRequest>
    {
        public BulkCopyRequestValidator()
        {
            RuleFor(x => x.SlabID)
                .Must(new Validation().IsValidInt)
                .WithMessage(SlabMessage.InvalidName);
        }
    }
}