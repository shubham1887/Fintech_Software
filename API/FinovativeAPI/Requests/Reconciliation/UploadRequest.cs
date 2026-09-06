using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Reconciliation
{
    public class UploadRequest
    {
        private string _FromDate;
        public string FromDate { get => _FromDate; set => _FromDate = value; }

        private string _ToDate;
        public string ToDate { get => _ToDate; set => _ToDate = value; }
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        public CustomFile ExcelDocument = new CustomFile();

        internal string IsValidModalState()
        {
            var validator = new UploadRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UploadRequestValidator : AbstractValidator<UploadRequest>
    {
        public UploadRequestValidator()
        {
            RuleFor(x => x.ProviderID)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(ReconciliationSettingMesages.InvalidProvider);
        }
    }
}