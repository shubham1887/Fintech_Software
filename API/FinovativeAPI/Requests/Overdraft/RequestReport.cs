using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Overdraft
{
    public class RequestReport
    {
        private string _FromDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        //private bool _IsExcelDownload;

        //public int TakeCount
        //{
        //    get => _TakeCount;
        //    set => _TakeCount = value;
        //}

        //public int SkipCount
        //{
        //    get => _SkipCount;
        //    set => _SkipCount = value;
        //}

        //public bool IsExcelDownload
        //{
        //    get => _IsExcelDownload;
        //    set => _IsExcelDownload = value;
        //}

        //private int _TakeCount;
        //private int _SkipCount;

        internal string IsValidModalState()
        {
            var validator = new RequestReportValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success)
                return StatusType.Ok;

            return failures[0].ToString();
        }
    }

    internal class RequestReportValidator : AbstractValidator<RequestReport>
    {
        public RequestReportValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(FundMessage.Invalidfrom);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(FundMessage.InvalidDate);

            RuleFor(x => x.Status).NotEmpty().WithMessage(FundMessage.InvalidStatus);
        }
    }
}