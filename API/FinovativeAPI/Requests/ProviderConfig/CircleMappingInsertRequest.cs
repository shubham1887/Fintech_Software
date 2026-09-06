using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfig
{
    public class CircleMappingInsertRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        public List<Project.Response.User.StateViewModel> CircleList = new List<Project.Response.User.StateViewModel>();

        internal string IsValidModalState()
        {
            var validator = new CircleMappingInsertRequestRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CircleMappingInsertRequestRequestValidator : AbstractValidator<CircleMappingInsertRequest>
    {
        public CircleMappingInsertRequestRequestValidator()
        {
            RuleFor(x => x.ProviderID)
                     .Must(new Validation().IsValidInt)
                     .WithMessage(ProviderMessage.InValidProvider);
            RuleFor(x => x.OperatorID)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(OperatorMessage.InValidOperator);

            //RuleFor(x => x.CircleList)
            //          .Must(list => list.Count >= 1)
            //          .WithMessage(ProviderConfigMessage.InvalidCircle);
        }
    }
}