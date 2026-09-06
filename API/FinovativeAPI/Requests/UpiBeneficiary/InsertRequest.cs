using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.UpiBeneficiary
{
    public class InsertRequest
    {
        private string _ActiveUPITransferName;

        [DefaultValue("")]
        public string ActiveUPITransferName
        {
            get => _ActiveUPITransferName.TrimIfNotNull();
            set => _ActiveUPITransferName = value.TrimIfNotNull();
        }

        public int UserID { get; set; }

        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

     
        private string _UpiID;

        public string UpiID
        {
            get => _UpiID.TrimIfNotNull();
            set => _UpiID = value.TrimIfNotNull();
        }


        private string _UpiSenderID;

        public string UpiSenderID
        {
            get => _UpiSenderID.TrimIfNotNull();
            set => _UpiSenderID = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }
       
        private string _SenderMobile;

        public string SenderMobile
        {
            get => _SenderMobile.TrimIfNotNull();
            set => _SenderMobile = value.TrimIfNotNull();
        }

        private string _AccountType;

        public string AccountType
        {
            get => _AccountType.TrimIfNotNull();
            set => _AccountType = value.TrimIfNotNull();
        }

        private bool _IsNameVerified;

        public bool IsNameVerified
        {
            get => _IsNameVerified;
            set => _IsNameVerified = value;
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
            /* RuleFor(x => x.UserID)
             .Must(new Validation().IsValidInt)
             .WithMessage(BeneficiaryMessage.InvalidUserID);*/

            RuleFor(x => x.Name)
                .Must(new Validation().IsValidString)
                .WithMessage(BeneficiaryMessage.InvalidName);
        

        }
    }
}
