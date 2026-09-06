using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.QRCode
{
    public class GenerateQRCodeRequest
    {
        private int _Amount;

        public int Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new GenerateQRCodeRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class GenerateQRCodeRequestValidator : AbstractValidator<GenerateQRCodeRequest>
    {
        public GenerateQRCodeRequestValidator()
        {
            RuleFor(x => x.Amount)
              .Must(new Validation().IsValidInt)
              .WithMessage(UserOnbordingMessage.InvalidMobile);

            RuleFor(x => x.WalletID)
              .Must(new Validation().IsValidInt)
              .WithMessage(UserOnbordingMessage.InvalidMobile);
        }
    }
}