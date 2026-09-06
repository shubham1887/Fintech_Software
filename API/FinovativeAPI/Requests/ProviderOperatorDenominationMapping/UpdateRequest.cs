using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderOperatorDenominationMapping
{
    public class UpdateRequest
    {
        private int _ProviderOperatorDenominationMappingID;
        public int ProviderOperatorDenominationMappingID { get => _ProviderOperatorDenominationMappingID; set => _ProviderOperatorDenominationMappingID = value; }

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

        private int _DenominationMasterID;

        public int DenominationMasterID
        {
            get => _DenominationMasterID;
            set => _DenominationMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.ProviderOperatorDenominationMappingID)
                          .Must(new Validation().IsValidInt)
                          .WithMessage(Message.ProviderOperatorDenominationMappingMessage.NotFound);
            RuleFor(x => x.ProviderID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.ProviderMessage.InValidProvider);

            RuleFor(x => x.OperatorID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.OperatorMessage.InValidOperator);

            RuleFor(x => x.DenominationMasterID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.DenominationMasterMessage.NotFound);
        }
    }
}