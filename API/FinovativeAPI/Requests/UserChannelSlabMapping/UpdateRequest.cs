using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserChannelSlabMapping
{
    public class UpdateRequest
    {
        private int _UserChannelSlabMappingID;
        public int UserChannelSlabMappingID { get => _UserChannelSlabMappingID; set => _UserChannelSlabMappingID = value; }
        private int _ChannelSlabMasterID;
        public int ChannelSlabMasterID { get => _ChannelSlabMasterID; set => _ChannelSlabMasterID = value; }

        internal string IsValidModalState()
        {
            var validator = new UserSlabUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UserSlabUpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UserSlabUpdateRequestValidator()
        {
            RuleFor(x => x.ChannelSlabMasterID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.SlabMessage.InvalidName);
            RuleFor(x => x.UserChannelSlabMappingID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.SlabMessage.InvalidID);
        }
    }
}