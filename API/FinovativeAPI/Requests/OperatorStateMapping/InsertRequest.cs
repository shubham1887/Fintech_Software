using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OperatorStateMapping
{
    public class InsertRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private string _HLR;

        public string HLR
        {
            get => _HLR.TrimIfNotNull();
            set => _HLR = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal class InsertRequestValidator : AbstractValidator<InsertRequest>
        {
            public InsertRequestValidator()
            {
                RuleFor(x => x.OperatorID)
                   .Must(new Validation().IsValidInt)
                   .WithMessage(OperatorStateMappingMessage.InvalidOperator);
                RuleFor(x => x.StateID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(OperatorStateMappingMessage.InvalidState);

                RuleFor(x => x.HLR)
                    .Must(new Validation().IsValidString)
                    .WithMessage(OperatorStateMappingMessage.InvalidHLR);
            }
        }
    }
}