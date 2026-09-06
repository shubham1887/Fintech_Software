using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class BulkAddRequest
    {
        private int _ChannelSlabMasterID;

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new BulkAddRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (!success) ? failures[0].ToString() : StatusType.Ok;
        }
    }

    public class BulkAddRequestValidator : AbstractValidator<BulkAddRequest>
    {
        public BulkAddRequestValidator()
        {
            RuleFor(x => x.ChannelSlabMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(SlabMessage.InvalidName);

            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);
        }
    }
}