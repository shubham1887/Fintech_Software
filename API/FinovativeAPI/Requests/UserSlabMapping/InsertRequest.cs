using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserSlabMapping
{
    public class InsertRequest
    {
        private int _SlabID;
        public int SlabID { get => _SlabID; set => _SlabID = value; }
        public int UserID { get => _UserID; set => _UserID = value; }

        private int _UserID;

        internal string IsValidModalState()
        {
            var validator = new UserSlabMappingInsertValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UserSlabMappingInsertValidator : AbstractValidator<InsertRequest>
    {
        public UserSlabMappingInsertValidator()
        {
            RuleFor(x => x.SlabID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}