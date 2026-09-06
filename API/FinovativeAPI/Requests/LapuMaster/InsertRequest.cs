using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.LapuMaster
{
    public class InsertRequest
    {
        private string _LapuNo;

        public string LapuNo
        {
            get => _LapuNo.TrimIfNotNull();
            set => _LapuNo = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _LapuPartyMasterID;

        public int LapuPartyMasterID
        {
            get => _LapuPartyMasterID;
            set => _LapuPartyMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.LapuNo)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.LapuMasterMessage.InvalidLapuNo);

            RuleFor(x => x.OperatorID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.LapuMasterMessage.InvalidOperatorID);

            RuleFor(x => x.LapuPartyMasterID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.LapuMasterMessage.InvalidLapuParty);
        }
    }
}