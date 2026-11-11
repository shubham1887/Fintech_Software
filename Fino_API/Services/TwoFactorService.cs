using System.Security.Cryptography;
using System.Text;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.TwoFactor;
using FintechApp.Responses.TwoFactor;
using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    /// <summary>
    /// Authenticator-app two-step verification: enrolment, the login check, and switching it off.
    ///
    /// Codes are generated and verified locally by <see cref="Totp"/> — there is no provider, no
    /// per-message cost and nothing to configure per install, which is the whole reason to prefer
    /// this over the SMS OTP path for staff and distributor accounts.
    /// </summary>
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IUserTwoFactorRepository _twoFactorRepo;
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _configuration;
        private readonly EncryptDecrypt _ed;
        private readonly Logger _logger;

        /// <summary>
        /// How many single-use recovery codes are issued. Ten is the figure every consumer service
        /// settled on: enough that losing a few does not matter, few enough to write on one line
        /// of paper.
        /// </summary>
        private const int RecoveryCodeCount = 10;

        /// <summary>
        /// Characters recovery codes are drawn from. No 0/O, 1/I/L — these get read off a screen
        /// and typed back in months later, often by someone on the phone to support.
        /// </summary>
        private const string RecoveryAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        public TwoFactorService(
            IUserTwoFactorRepository twoFactorRepo,
            IUserRepository userRepo,
            IConfiguration configuration)
        {
            _twoFactorRepo = twoFactorRepo;
            _userRepo = userRepo;
            _configuration = configuration;
            _logger = new Logger();
            _ed = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        /// <summary>
        /// The name the authenticator app lists the account under. Taken from the brand the
        /// installer already collected, so a reseller install shows the client's own name without
        /// anyone typing it a second time.
        /// </summary>
        private string Issuer =>
            new[] { _configuration["Company:ClientName"], _configuration["Bootstrap:BrandName"] }
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? "Fino";

        public async Task<TwoFactorStatusResponse> Status(int userId)
        {
            var row = await _twoFactorRepo.GetByUserID(userId);
            if (row == null)
                return new TwoFactorStatusResponse();

            return new TwoFactorStatusResponse
            {
                IsEnabled = row.IsConfirmed,
                IsPending = !row.IsConfirmed,
                EnabledOn = row.ConfirmedOn,
                RecoveryCodesRemaining = ReadRecoveryHashes(row.RecoveryCodeHashes).Count,
            };
        }

        public async Task<(string failureMessage, TwoFactorSetupResponse? data)> BeginSetup(int userId)
        {
            var user = await _userRepo.GetUserById(userId);
            if (user == null)
                return (TwoFactorMessages.UserNotFound, null);

            var existing = await _twoFactorRepo.GetByUserID(userId);
            if (existing != null && existing.IsConfirmed)
                return (TwoFactorMessages.AlreadyEnabled, null);

            // A fresh secret every time setup is opened. Re-using a pending secret would mean a QR
            // shown on a shared screen last week still enrols a phone today.
            string secret = Totp.GenerateSecret();
            await _twoFactorRepo.StartEnrolment(userId, secret);

            // Mobile number rather than the person name: it is what they sign in with and it is
            // unique, so someone holding several accounts for one firm can tell the entries apart.
            string account = string.IsNullOrWhiteSpace(user.MobileNumber) ? $"user{userId}" : user.MobileNumber;

            return (StatusType.Ok, new TwoFactorSetupResponse
            {
                ProvisioningUri = Totp.BuildProvisioningUri(Issuer, account, secret),
                ManualEntryKey = Totp.FormatSecretForDisplay(secret),
                Account = account,
                Issuer = Issuer,
            });
        }

        public async Task<(string failureMessage, TwoFactorConfirmedResponse? data)> Confirm(int userId, ConfirmTwoFactorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return (TwoFactorMessages.CodeRequired, null);

            var row = await _twoFactorRepo.GetByUserID(userId);
            if (row == null)
                return (TwoFactorMessages.SetupNotStarted, null);
            if (row.IsConfirmed)
                return (TwoFactorMessages.AlreadyEnabled, null);

            long? step = Totp.Verify(row.SecretBase32, request.Code, DateTime.UtcNow);
            if (step == null)
                return (TwoFactorMessages.InvalidCode, null);

            // Plain text here and nowhere else: the response below is the only time these exist in
            // readable form. What gets stored is hashes.
            var codes = GenerateRecoveryCodes();

            row.IsConfirmed = true;
            row.ConfirmedOn = DateTime.UtcNow;
            row.LastUsedStep = step.Value;
            row.RecoveryCodeHashes = JsonConvert.SerializeObject(codes.Select(HashRecoveryCode).ToList());
            await _twoFactorRepo.Update(row);

            // Only now does the account start demanding a second step. Setting the flag any earlier
            // — when the QR is shown — would lock out anyone who closed the page half way through.
            await _userRepo.SetTwoFactorEnabled(userId, true);

            return (StatusType.Ok, new TwoFactorConfirmedResponse { RecoveryCodes = codes });
        }

        public async Task<string> Disable(int userId, DisableTwoFactorRequest request)
        {
            if (request == null)
                return Common.Message.InvalidRequest;
            if (string.IsNullOrWhiteSpace(request.Password))
                return TwoFactorMessages.PasswordRequired;
            if (string.IsNullOrWhiteSpace(request.Code))
                return TwoFactorMessages.CodeRequired;

            // Password re-checked against the database, not the cached user list: this is a
            // credential check, and it is the one thing standing between a borrowed session and a
            // permanently unprotected account.
            var user = await _userRepo.GetUserByIdWithPassword(userId, _ed.HashPassword(request.Password));
            if (user == null)
                return TwoFactorMessages.WrongPassword;

            var row = await _twoFactorRepo.GetByUserID(userId);
            if (row == null || !row.IsConfirmed)
                return TwoFactorMessages.NotEnrolled;

            // Same check as at sign-in, including replay refusal and recovery-code consumption.
            if (!await VerifyForLogin(userId, request.Code))
                return TwoFactorMessages.InvalidCode;

            await _twoFactorRepo.DeleteForUser(userId);
            await _userRepo.SetTwoFactorEnabled(userId, false);

            return StatusType.Ok;
        }

        public async Task<bool> VerifyForLogin(int userId, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var row = await _twoFactorRepo.GetByUserID(userId);

            // An unconfirmed enrolment must not satisfy the check. If it did, starting setup and
            // walking away would be enough to pass the second step with a secret the user never
            // finished scanning.
            if (row == null || !row.IsConfirmed)
                return false;

            long? step = Totp.Verify(row.SecretBase32, code, DateTime.UtcNow);
            if (step != null)
            {
                // Refuse a code that has already been spent. Without this the same six digits stay
                // usable for the rest of their ninety-second window.
                if (step.Value <= row.LastUsedStep)
                    return false;

                row.LastUsedStep = step.Value;
                await _twoFactorRepo.Update(row);
                return true;
            }

            return await TrySpendRecoveryCode(row, code);
        }

        public async Task<string> ClearForUser(int userId)
        {
            var user = await _userRepo.GetUserById(userId);
            if (user == null)
                return TwoFactorMessages.UserNotFound;

            // Delete the secret, not just the flag: if two-step verification is switched on again
            // later it must not accept codes from the phone this was cleared because of.
            await _twoFactorRepo.DeleteForUser(userId);
            await _userRepo.SetTwoFactorEnabled(userId, false);

            return StatusType.Ok;
        }

        // ─── Recovery codes ──────────────────────────────────────────────────

        /// <summary>
        /// Spend a recovery code. Each is single-use: the matching hash is removed before this
        /// returns, so the same slip of paper cannot get someone in twice.
        /// </summary>
        private async Task<bool> TrySpendRecoveryCode(UserTwoFactor row, string code)
        {
            var hashes = ReadRecoveryHashes(row.RecoveryCodeHashes);
            if (hashes.Count == 0)
                return false;

            string candidate = HashRecoveryCode(code);

            // Every entry is compared, and with a fixed-time comparison, so neither the number of
            // codes left nor which one matched can be read off the response time.
            int matchIndex = -1;
            for (int i = 0; i < hashes.Count; i++)
            {
                bool same = CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(hashes[i]),
                    Encoding.ASCII.GetBytes(candidate));

                if (same && matchIndex < 0)
                    matchIndex = i;
            }

            if (matchIndex < 0)
                return false;

            hashes.RemoveAt(matchIndex);
            row.RecoveryCodeHashes = JsonConvert.SerializeObject(hashes);
            await _twoFactorRepo.Update(row);
            return true;
        }

        private static List<string> GenerateRecoveryCodes()
        {
            var codes = new List<string>(RecoveryCodeCount);
            for (int i = 0; i < RecoveryCodeCount; i++)
            {
                // Ten characters from a 31-character alphabet is about 49 bits — far beyond
                // guessing through a login form, and short enough to read aloud. Grouped in the
                // middle because these get transcribed by hand.
                string raw = new string(Enumerable.Range(0, 10)
                    .Select(_ => RecoveryAlphabet[RandomNumberGenerator.GetInt32(RecoveryAlphabet.Length)])
                    .ToArray());
                codes.Add($"{raw.Substring(0, 5)}-{raw.Substring(5)}");
            }
            return codes;
        }

        /// <summary>
        /// Hash of a recovery code, after normalising away everything a person might type
        /// differently from what was displayed — case, spaces and the grouping dash.
        ///
        /// An unsalted SHA-256 is adequate here in a way it would not be for a password: these are
        /// 49-bit strings the server generated at random, so there is no dictionary to precompute
        /// and nothing for a rainbow table to shorten.
        /// </summary>
        private string HashRecoveryCode(string code) =>
            _ed.HashPassword(NormaliseRecoveryCode(code));

        private static string NormaliseRecoveryCode(string code) =>
            new string((code ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        private List<string> ReadRecoveryHashes(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();
            try
            {
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            catch (Exception ex)
            {
                // A malformed column must not become a way past the second step, so this reports
                // "no recovery codes left" rather than failing open.
                _logger.Write(ex);
                return new List<string>();
            }
        }
    }
}
