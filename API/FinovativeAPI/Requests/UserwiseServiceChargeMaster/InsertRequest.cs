using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserwiseServiceChargeMaster
{
    public class InsertRequest
    {
        private int _UserwiseServiceChargeMasterID;

        public int UserwiseServiceChargeMasterID
        {
            get => _UserwiseServiceChargeMasterID;
            set => _UserwiseServiceChargeMasterID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private decimal _OneTime;
        public decimal OneTime { get => _OneTime; set => _OneTime = value; }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.InvalidUserID);

            RuleFor(x => x.ServiceID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ServiceMessage.InvalidServideID);

            RuleFor(x => x.OneTime)
              .Must(new Validation().IsValidDecimal)
              .WithMessage(ServiceMessage.InvalidOnetime);
        }
    }
}