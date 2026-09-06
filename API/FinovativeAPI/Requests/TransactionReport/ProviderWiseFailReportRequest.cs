using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.TransactionReport
{
    public class ProviderWiseFailReportRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
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

        internal string IsValidModalState()
        {
            var validator = new ProviderWiseFailReportRequestValidator();
            ValidationResult results = validator.Validate(this);

            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    internal class ProviderWiseFailReportRequestValidator : AbstractValidator<ProviderWiseFailReportRequest>
    {
        public ProviderWiseFailReportRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(TransactionDetailMessage.Invalidfrom);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(TransactionDetailMessage.InvalidDate);

            RuleFor(x => x.ProviderID).NotEmpty().WithMessage(TransactionDetailMessage.InvalidProvider);
        }
    }
}