using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using System.Collections.Generic;

namespace Project.Requests.DayBookReport
{
    public class ProviderDayBookRequest
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

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        internal string IsValidModalState()
        {
            var validator = new RequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success == false)
            {
                return failures[0].ToString(); ;
            }

            return new Validation().isValidFromAndToDate(FromDate, ToDate);
        }
    }

    internal class RequestValidator : AbstractValidator<ProviderDayBookRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty()
                .WithMessage(Message.DaybookReport.InvalidtoDate);
            RuleFor(x => x.ToDate).NotEmpty()
                .WithMessage(Message.DaybookReport.InvalidfromDate);
        }
    }
}