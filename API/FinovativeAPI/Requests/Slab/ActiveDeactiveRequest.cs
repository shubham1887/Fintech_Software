using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Slab
{
    public class ActiveDeactiveRequest
    {
        private int _SlabID;

        public int SlabID { get => _SlabID; set => _SlabID = value; }

        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        internal string IsValidModalState()
        {
            var validator = new ActiveDeactiveRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Slab Activation {IsActive}";
        }
    }

    internal class ActiveDeactiveRequestValidator : AbstractValidator<ActiveDeactiveRequest>
    {
        public ActiveDeactiveRequestValidator()
        {
            RuleFor(x => x.SlabID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}