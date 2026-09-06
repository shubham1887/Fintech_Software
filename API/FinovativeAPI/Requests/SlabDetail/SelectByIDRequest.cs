using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class SelectByIDRequest
    {
        private int _SlabDetailID;

        public int SlabDetailID
        {
            get => _SlabDetailID;
            set => _SlabDetailID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Commission deleted with SlabDetailID:{SlabDetailID}";
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.SlabDetailID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabDetailMessage.InValidID);
        }
    }
}