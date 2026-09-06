using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserSlabMapping
{
    public class UpdateRequest
    {
        private int _UserSlabMappingID;
        public int UserSlabMappingID { get => _UserSlabMappingID; set => _UserSlabMappingID = value; }
        private int _SlabID;
        public int SlabID { get => _SlabID; set => _SlabID = value; }

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
            RuleFor(x => x.SlabID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.SlabMessage.InvalidName);
            RuleFor(x => x.UserSlabMappingID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}