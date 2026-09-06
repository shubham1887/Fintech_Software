using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.DematGrowthLoss
{
    public class SelectByIDRequest
    {
        private int _DematGrowthLossMasterID;

        public int DematGrowthLossMasterID
        {
            get => _DematGrowthLossMasterID;
            set => _DematGrowthLossMasterID = value;
        }

     

        internal string IsValidModalState()
        {
            var validator = new EditRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class EditRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public EditRequestValidator()
        {
            RuleFor(x => x.DematGrowthLossMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(DematGrowthLossMasterMessage.InvalidID);
        }
    }
}

