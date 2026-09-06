using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using Project.Message;
namespace Project.Requests.DematGrowthLoss
{
    public class DeleteRequest
    {
        private int _DematGrowthLossMasterID;

        public int DematGrowthLossMasterID
        {
            get => _DematGrowthLossMasterID;
            set => _DematGrowthLossMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
           RuleFor(x => x.DematGrowthLossMasterID)
         .Must(new Validation().IsValidInt)
         .WithMessage(DematGrowthLossMasterMessage.InValidDematGrowthLossMasterID);
        }
    }
}

