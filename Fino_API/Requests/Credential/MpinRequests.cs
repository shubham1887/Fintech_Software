using FluentValidation;

namespace FintechApp.Requests.Credential
{
    /// <summary>Set an MPIN for the first time, or replace it. `OldMpin` is required only when one
    /// already exists (the service enforces that).</summary>
    public class SetMpinRequest
    {
        public string OldMpin { get; set; } = string.Empty;
        public string NewMpin { get; set; } = string.Empty;
        public string ConfirmMpin { get; set; } = string.Empty;
        /// <summary>User's own opt-in. The platform-wide requirement is the IsMpinEnabled Key Setting.</summary>
        public bool Enable { get; set; } = true;
    }

    public class SetMpinRequestValidator : AbstractValidator<SetMpinRequest>
    {
        public SetMpinRequestValidator()
        {
            RuleFor(x => x.NewMpin).NotEmpty().Matches("^[0-9]{4,6}$").WithMessage("MPIN must be 4 to 6 digits.");
            RuleFor(x => x.ConfirmMpin).Equal(x => x.NewMpin).WithMessage("MPIN and confirm MPIN do not match.");
        }
    }

    /// <summary>Verify the MPIN (on a transaction or on opening the app).</summary>
    public class VerifyMpinRequest
    {
        public string Mpin { get; set; } = string.Empty;
    }

    public class VerifyMpinRequestValidator : AbstractValidator<VerifyMpinRequest>
    {
        public VerifyMpinRequestValidator()
        {
            RuleFor(x => x.Mpin).NotEmpty().Matches("^[0-9]{4,6}$").WithMessage("MPIN must be 4 to 6 digits.");
        }
    }
}
