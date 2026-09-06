using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.DenominationMaster
{
    public class InsertRequest
    {
        private int _DenominationMasterID;

        public int DenominationMasterID
        {
            get => _DenominationMasterID;
            set => _DenominationMasterID = value;
        }

        private int _MinAmount;

        public int MinAmount
        {
            get => _MinAmount;
            set => _MinAmount = value;
        }

        private int _MaxAmount;

        public int MaxAmount
        {
            get => _MaxAmount;
            set => _MaxAmount = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private bool _IsStop;
        public bool IsStop { get => _IsStop; set => _IsStop = value; }
        private string _DenominationRange;

        public string DenominationRange
        {
            get => _DenominationRange;
            set => _DenominationRange = value;
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
                RuleFor(x => x.Description)
                             .Must(new Validation().IsValidString)
                             .WithMessage(Message.DenominationMasterMessage.InvalidDescription);
            }
        }
    }
}