using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Slab
{
    public class UpdateRequest
    {
        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

        public int SlabID { get => _SlabID; set => _SlabID = value; }

        private int _SlabID;

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Update slab name {SlabName} with SlabID {SlabID}";
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.SlabName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.SlabMessage.InvalidName);

            RuleFor(x => x.SlabID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}