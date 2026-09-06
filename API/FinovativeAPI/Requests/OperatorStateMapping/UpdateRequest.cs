using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OperatorStateMapping
{
    public class UpdateRequest
    {
        private int _OperatorStateMappingID;

        public int OperatorStateMappingID { get => _OperatorStateMappingID; set => _OperatorStateMappingID = value; }

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

        internal class InsertRequestValidator : AbstractValidator<UpdateRequest>
        {
            public InsertRequestValidator()
            {
                RuleFor(x => x.OperatorStateMappingID)
                   .Must(new Validation().IsValidInt)
                   .WithMessage(OperatorStateMappingMessage.InvalidOperator);

                RuleFor(x => x.HLR)
                    .Must(new Validation().IsValidString)
                    .WithMessage(OperatorStateMappingMessage.InvalidHLR);
            }
        }
    }
}