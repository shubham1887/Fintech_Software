using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Onboarding
{
    public class SelectRequest
    {
        private int _OnboardingUserID;
        public int OnboardingUserID
        {
            get => _OnboardingUserID;
            set => _OnboardingUserID = value;
        }
        private string _CriteriaText;
        public string CriteriaText
        {
            get => _CriteriaText.TrimIfNotNull();
            set => _CriteriaText = value.TrimIfNotNull();
        }
        private string _Criteria;
        public string Criteria
        {
            get => _Criteria.TrimIfNotNull();
            set => _Criteria = value.TrimIfNotNull();
        }

        
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

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

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }
        private bool _IsPDFDownload;
        public bool IsPDFDownload
        {
            get => _IsPDFDownload;
            set => _IsPDFDownload = value;
        }

        private string _ExportType;

        public string ExportType
        {
            get => _ExportType.TrimIfNotNull();
            set => _ExportType = value.TrimIfNotNull();
        }

        private string _CheckerStatus;

        public string CheckerStatus
        {
            get => _CheckerStatus.TrimIfNotNull();
            set => _CheckerStatus = value.TrimIfNotNull();
        }
        private string _MakerStatus;

        public string MakerStatus
        {
            get => _MakerStatus.TrimIfNotNull();
            set => _MakerStatus = value.TrimIfNotNull();
        }
        private string _Search;

        public string Search
        {
            get => _Search.TrimIfNotNull();
            set => _Search = value.TrimIfNotNull();
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
            var validator = new ReportRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success)
            {
                return StatusType.Ok;
            }

            return failures[0].ToString();
        }
    }

    internal class ReportRequestValidator : AbstractValidator<SelectRequest>
    {
        public ReportRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(TransactionDetailMessage.Invalidfrom);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(TransactionDetailMessage.InvalidDate);
        }
    }
}