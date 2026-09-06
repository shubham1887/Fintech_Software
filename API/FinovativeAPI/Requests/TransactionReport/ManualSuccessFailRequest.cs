using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.TransactionReport
{
    public class ManualSuccessFailRequest
    {
        private string _FromDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ManualSuccessFailRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success) return StatusType.Ok;

            return failures[0].ToString();
        }
    }

    internal class ManualSuccessFailRequestValidator : AbstractValidator<ManualSuccessFailRequest>
    {
        public ManualSuccessFailRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(TransactionMessage.InvalidDate);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(TransactionMessage.InvalidDate);
        }
    }
}