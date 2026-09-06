using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ReconciliationSettings
{
    public class InsertRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _WorksheetName;

        public string WorksheetName
        {
            get => _WorksheetName.TrimIfNotNull();
            set => _WorksheetName = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _SuccessStatusAs;

        public string SuccessStatusAs
        {
            get => _SuccessStatusAs.TrimIfNotNull();
            set => _SuccessStatusAs = value.TrimIfNotNull();
        }

        private string _FailureStatusAs;

        public string FailureStatusAs
        {
            get => _FailureStatusAs.TrimIfNotNull();
            set => _FailureStatusAs = value.TrimIfNotNull();
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
            RuleFor(x => x.ProviderID)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(ReconciliationSettingMesages.InvalidProvider);

            RuleFor(x => x.ConsumerNumber)
                      .Must(new Validation().IsValidString)
                      .WithMessage(ReconciliationSettingMesages.InvalidConsumerNumber);

            RuleFor(x => x.Amount)
                   .Must(new Validation().IsValidString)
                   .WithMessage(ReconciliationSettingMesages.InvalidAmount);

            RuleFor(x => x.Status)
                  .Must(new Validation().IsValidString)
                  .WithMessage(ReconciliationSettingMesages.InvalidStatus);

            RuleFor(x => x.WorksheetName)
           .Must(new Validation().IsValidString)
           .WithMessage(ReconciliationSettingMesages.InvalidWorksheetName);

            RuleFor(x => x.APIUserRequestID)
           .Must(new Validation().IsValidString)
           .WithMessage(ReconciliationSettingMesages.InvalidAPIUserRequestID);

            RuleFor(x => x.SuccessStatusAs)
      .Must(new Validation().IsValidString)
      .WithMessage(ReconciliationSettingMesages.InvalidStatusSuccessAS);

            RuleFor(x => x.FailureStatusAs)
           .Must(new Validation().IsValidString)
           .WithMessage(ReconciliationSettingMesages.InvalidFailureAS);
        }
    }
}