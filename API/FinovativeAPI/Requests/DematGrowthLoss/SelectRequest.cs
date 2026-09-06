using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
namespace Project.Requests.DematGrowthLoss

{
    public class SelectRequest
    {
        private int _DematGrowthLossMasterID;

        public int DematGrowthLossMasterID
        {
            get => _DematGrowthLossMasterID;
            set => _DematGrowthLossMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.DematGrowthLossMasterID)
         .Must(new Validation().IsValidInt)
         .WithMessage(DematGrowthLossMasterMessage.InValidDematGrowthLossMasterID);
        }
    }

}
