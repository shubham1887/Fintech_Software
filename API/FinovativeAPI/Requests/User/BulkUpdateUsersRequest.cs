using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class BulkUpdateUsersRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

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

        private string _ResponseType;

        public string ResponseType
        {
            get => _ResponseType.TrimIfNotNull();
            set => _ResponseType = value.TrimIfNotNull();
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

        private string _GSTHoldCutting;

        public string GSTHoldCutting
        {
            get => _GSTHoldCutting.TrimIfNotNull();
            set => _GSTHoldCutting = value.TrimIfNotNull();
        }

        private string _GSTType;

        public string GSTType
        {
            get => _GSTType.TrimIfNotNull();
            set => _GSTType = value.TrimIfNotNull();
        }

        private decimal _ReferralAmount;

        public decimal ReferralAmount
        {
            get => _ReferralAmount;
            set => _ReferralAmount = value;
        }

        private decimal _ReferralBenefitMinAmount;

        public decimal ReferralBenefitMinAmount
        {
            get => _ReferralBenefitMinAmount;
            set => _ReferralBenefitMinAmount = value;
        }

        private List<UserServiceMappingDetailList> _list;

        public List<UserServiceMappingDetailList> UserServiceList
        {
            get => _list;
            set => _list = value;
        }

        internal string IsValidModalState()
        {
            var validator = new BulkUpdateUsersRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class BulkUpdateUsersRequestValidator : AbstractValidator<BulkUpdateUsersRequest>
    {
        public BulkUpdateUsersRequestValidator()
        {
            RuleFor(x => x.UserID)
               .Must(new Validation().IsValidInt)
               .WithMessage(UserMessage.InvalidUser);

            RuleFor(x => x.Criteria)
              .Must(new Validation().IsValidString)
              .WithMessage(Message.UserMessage.InvalidCriteria);
        }
    }
}