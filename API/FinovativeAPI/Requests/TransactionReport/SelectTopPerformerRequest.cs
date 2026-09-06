using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.TransactionReport
{
    public class SelectTopPerformerRequest
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

        private int _TopPerformance;

        public int TopPerformance
        {
            get => _TopPerformance;
            set => _TopPerformance = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectTopPerformerRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success) return StatusType.Ok;

            return failures[0].ToString();
        }
    }

    internal class SelectTopPerformerRequestValidator : AbstractValidator<SelectTopPerformerRequest>
    {
        public SelectTopPerformerRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(TransactionDetailMessage.Invalidfrom);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(TransactionDetailMessage.InvalidDate);
        }
    }
}