using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineSlabDetail
{
    public class SelectByIDRequest
    {
        private int _OfflineSlabDetailID;

        public int OfflineSlabDetailID
        {
            get => _OfflineSlabDetailID;
            set => _OfflineSlabDetailID = value;
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
            return $"Commission deleted with SlabDetailID:{OfflineSlabDetailID}";
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.OfflineSlabDetailID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabDetailMessage.InValidID);
        }
    }
}