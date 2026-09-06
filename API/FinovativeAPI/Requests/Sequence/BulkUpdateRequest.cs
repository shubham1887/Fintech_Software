using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Sequence
{
    public class BulkUpdateRequest
    {
        private string _Criteria;

        public string Criteria
        {
            get => _Criteria.TrimIfNotNull();
            set => _Criteria = value.TrimIfNotNull();
        }

        private string _CriteriaText;

        public string CriteriaText
        {
            get => _CriteriaText.TrimIfNotNull();
            set => _CriteriaText = value.TrimIfNotNull();
        }

        private bool _IsTextValue;

        public bool IsTextValue
        {
            get => _IsTextValue;
            set => _IsTextValue = value;
        }

        private bool _CriteriaValue;

        public bool CriteriaValue
        {
            get => _CriteriaValue;
            set => _CriteriaValue = value;
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

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private List<ProviderList> _List;

        public List<ProviderList> providerList
        {
            get => _List;
            set => _List = value;
        }

        private int _RealProviderID;

        public int RealProviderID
        {
            get => _RealProviderID;
            set => _RealProviderID = value;
        }

        private int _ViewProviderID;

        public int ViewProviderID
        {
            get => _ViewProviderID;
            set => _ViewProviderID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new BulkUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class BulkUpdateRequestValidator : AbstractValidator<BulkUpdateRequest>
    {
        public BulkUpdateRequestValidator()
        {
            RuleFor(x => x.OperatorID)
               .Must(new Validation().IsValidInt)
               .WithMessage(SequenceMessage.InvalidOperator);

            RuleFor(x => x.Criteria)
              .Must(new Validation().IsValidString)
              .WithMessage(SequenceMessage.InvalidCriteria);
        }
    }
}