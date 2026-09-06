using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class InsertMasterRequest
    {
        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.ToTitleCase();
            set => _SlabName = value.ToTitleCase();
        }

        private int _ChannelSlabMasterID;

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertMasterRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertMasterRequestValidator : AbstractValidator<InsertMasterRequest>
    {
        public InsertMasterRequestValidator()
        {
            RuleFor(x => x.SlabName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}