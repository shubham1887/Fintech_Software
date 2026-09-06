using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserChannelSlabMapping
{
    public class SelectRequest
    {
        private int _UserChannelSlabMappingID;

        public int UserChannelSlabMappingID
        {
            get => _UserChannelSlabMappingID;
            set => _UserChannelSlabMappingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UserSlabMappingDeleteRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UserSlabMappingDeleteRequestValidator : AbstractValidator<SelectRequest>
    {
        public UserSlabMappingDeleteRequestValidator()
        {
            RuleFor(x => x.UserChannelSlabMappingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidID);
        }
    }
}