namespace FintechApp.Messages
{
    public static class TwoFactorMessages
    {
        public const string AlreadyEnabled = "Two-step verification is already switched on for this account.";
        public const string NotEnrolled = "Two-step verification has not been set up for this account.";
        public const string SetupNotStarted = "Start the setup again — there is no pending authenticator to confirm.";
        public const string CodeRequired = "Enter the 6-digit code from your authenticator app.";
        public const string InvalidCode = "That code is not correct. Check the app and try again — each code lasts 30 seconds.";
        public const string CodeAlreadyUsed = "That code has already been used. Wait for the app to show the next one.";
        public const string EnabledSuccessfully = "Two-step verification is on. Keep your recovery codes somewhere safe.";
        public const string DisabledSuccessfully = "Two-step verification has been switched off.";
        public const string ClearedSuccessfully = "The authenticator enrolment has been cleared. The user can set it up again at next login.";
        public const string PasswordRequired = "Enter your account password to switch two-step verification off.";
        public const string WrongPassword = "That password is not correct.";

        /// <summary>
        /// Shown at login when the password was right but the second step is still outstanding.
        /// The client reads this alongside IsTwoFA and shows the code box.
        /// </summary>
        public const string Required = "Enter the code from your authenticator app to finish signing in.";
        public const string UserNotFound = "This user could not be found.";
    }
}
