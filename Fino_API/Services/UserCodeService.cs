using System.Text;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserCode;
using FintechApp.Responses.UserCode;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// Generates the User Code stored on a user as AliasName, from the admin-configured format of
    /// the user type. Each type keeps its OWN running sequence, so Retailers count RD000001,
    /// RD000002 … while Admins count AD000001, AD000002 … independently.
    ///
    /// <para>Two things guarantee uniqueness. The number is handed out by a single atomic UPDATE
    /// (see <see cref="IUserCodeFormatRepository.AllocateNextSequence"/>), so two simultaneous
    /// creations can never share one. And every candidate is checked against the users table
    /// before it is returned — if RD000004 is already taken (data migration, a manual edit, a
    /// counter that fell behind) the generator walks on to RD000005 rather than ever returning a
    /// duplicate.</para>
    /// </summary>
    public class UserCodeService : IUserCodeService
    {
        private readonly IUserCodeFormatRepository _repository;
        private readonly IUserTypeMasterRepository _userTypeRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>Default shape: code + zero-padded running number (RD000001).</summary>
        public const string DefaultPattern = "{CODE}{SEQ}";
        private const string SequenceToken = "{SEQ}";
        private const int DefaultSequenceLength = 6;
        private const int MaxSequenceLength = 12;
        private const int MaxCodeLength = 10;
        /// <summary>How many taken codes to step over before falling back — generous, and bounded.</summary>
        private const int MaxAttempts = 50;

        public UserCodeService(
            IUserCodeFormatRepository repository,
            IUserTypeMasterRepository userTypeRepository,
            IUserRepository userRepository)
        {
            _repository = repository;
            _userTypeRepository = userTypeRepository;
            _userRepository = userRepository;
        }

        // ── Admin configuration ──────────────────────────────────────────────────────────────

        public async Task<List<UserCodeFormatResponse>> GetAll()
        {
            var types = await _userTypeRepository.GetAll();
            var formats = await _repository.GetAll();
            var users = await _userRepository.GetAllUsers();
            var now = DateTime.Now;

            var list = new List<UserCodeFormatResponse>();
            foreach (var type in types.OrderBy(t => t.UserTypeID))
            {
                var cfg = formats.FirstOrDefault(f => f.UserTypeID == type.UserTypeID);
                var effective = cfg ?? await BuildDefault(type);

                list.Add(new UserCodeFormatResponse
                {
                    UserCodeFormatID = cfg?.UserCodeFormatID ?? 0,
                    UserTypeID = type.UserTypeID,
                    UserType = type.UserType ?? string.Empty,
                    Code = effective.Code,
                    Pattern = effective.Pattern,
                    SequenceLength = effective.SequenceLength,
                    StartNumber = effective.StartNumber,
                    LastSequence = effective.LastSequence,
                    ResetPolicy = effective.ResetPolicy,
                    SequencePeriod = effective.SequencePeriod,
                    IsActive = effective.IsActive,
                    // Only a number at or past the start has actually been handed out — a counter
                    // parked just below StartNumber means the type has issued nothing yet.
                    LastCode = effective.LastSequence >= effective.StartNumber
                        ? Render(effective, effective.LastSequence, now)
                        : string.Empty,
                    NextCode = await NextAvailableCode(effective, now),
                    UsersWithCode = users.Count(u => u.UserTypeID == type.UserTypeID && !string.IsNullOrWhiteSpace(u.AliasName)),
                    IsConfigured = cfg != null,
                });
            }
            return list;
        }

        public async Task<string> Save(SaveUserCodeFormatRequest request)
        {
            var type = await _userTypeRepository.GetByID(request.UserTypeID);
            if (type == null) return "Please select a valid user type.";

            string code = Sanitise(request.Code);
            if (string.IsNullOrEmpty(code))
                return "Please enter the code for this user type (letters and digits only, e.g. RD).";
            if (code.Length > MaxCodeLength)
                return $"The code cannot be longer than {MaxCodeLength} characters.";

            string pattern = string.IsNullOrWhiteSpace(request.Pattern) ? DefaultPattern : request.Pattern.Trim();
            if (pattern.Length > 100)
                return "The format is too long (100 characters maximum).";
            if (pattern.IndexOf(SequenceToken, StringComparison.OrdinalIgnoreCase) < 0)
                return "The format must contain {SEQ} — that is the running number that makes each code unique.";

            int sequenceLength = request.SequenceLength <= 0 ? DefaultSequenceLength : request.SequenceLength;
            if (sequenceLength > MaxSequenceLength)
                return $"The running number cannot be padded beyond {MaxSequenceLength} digits.";

            int startNumber = request.StartNumber < 1 ? 1 : request.StartNumber;

            string resetPolicy = string.IsNullOrWhiteSpace(request.ResetPolicy)
                ? UserCodeResetPolicy.None
                : request.ResetPolicy.Trim();
            if (!UserCodeResetPolicy.IsValid(resetPolicy))
                return "Reset must be None, Yearly or Monthly.";
            resetPolicy = Normalise(resetPolicy);

            var existing = await _repository.GetByUserTypeID(request.UserTypeID);
            var now = DateTime.Now;
            string period = UserCodeResetPolicy.PeriodKey(resetPolicy, now);

            if (existing == null)
            {
                var entity = new UserCodeFormat
                {
                    UserTypeID = request.UserTypeID,
                    Code = code,
                    Pattern = pattern,
                    SequenceLength = sequenceLength,
                    StartNumber = startNumber,
                    ResetPolicy = resetPolicy,
                    SequencePeriod = period,
                    IsActive = request.IsActive,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                };
                // Continue whatever series the existing users are already on, so a freshly
                // configured type never re-issues a code that data migration handed out.
                entity.LastSequence = await SeedSequence(entity, now);
                return await _repository.Insert(entity) > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
            }

            bool shapeChanged = !string.Equals(existing.Code, code, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.Pattern, pattern, StringComparison.Ordinal)
                || existing.SequenceLength != sequenceLength;

            existing.Code = code;
            existing.Pattern = pattern;
            existing.SequenceLength = sequenceLength;
            existing.StartNumber = startNumber;
            existing.ResetPolicy = resetPolicy;
            existing.IsActive = request.IsActive;
            existing.UpdatedateTime = DateTime.UtcNow;

            // A new shape means the old counter counts something else — restart it from whatever
            // the new shape has already reached among existing users.
            if (shapeChanged || existing.SequencePeriod != period)
            {
                existing.SequencePeriod = period;
                existing.LastSequence = await SeedSequence(existing, now);
            }

            return await _repository.Update(existing) > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int userCodeFormatId)
        {
            if (userCodeFormatId <= 0) return Messages.Common.Message.InvalidRequest;
            return await _repository.Delete(userCodeFormatId) > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        // ── Generation ───────────────────────────────────────────────────────────────────────

        public async Task<UserCodePreviewResponse> Preview(int userTypeId)
        {
            var type = await _userTypeRepository.GetByID(userTypeId);
            var cfg = await _repository.GetByUserTypeID(userTypeId);
            var now = DateTime.Now;

            if (cfg == null)
            {
                if (type == null)
                    return new UserCodePreviewResponse { UserTypeID = userTypeId };
                cfg = await BuildDefault(type);
            }

            return new UserCodePreviewResponse
            {
                UserTypeID = userTypeId,
                UserType = type?.UserType ?? string.Empty,
                UserCode = await NextAvailableCode(cfg, now),
                Pattern = cfg.Pattern,
                IsConfigured = cfg.UserCodeFormatID > 0,
            };
        }

        public async Task<string> GenerateUserCode(int userTypeId)
        {
            var now = DateTime.Now;
            var cfg = await _repository.GetByUserTypeID(userTypeId);

            // First user of a type nobody has configured yet: provision the default format so the
            // type has a real counter from here on, seeded past whatever codes already exist.
            if (cfg == null)
            {
                var type = await _userTypeRepository.GetByID(userTypeId);
                if (type == null) return string.Empty;

                cfg = await BuildDefault(type);
                cfg.DateTime = DateTime.UtcNow;
                cfg.UpdatedateTime = DateTime.UtcNow;
                if (await _repository.Insert(cfg) <= 0) return await UniqueFallback(cfg, now);
                cfg = await _repository.GetByUserTypeID(userTypeId) ?? cfg;
            }

            if (!cfg.IsActive) return string.Empty; // auto-coding switched off for this type

            string period = UserCodeResetPolicy.PeriodKey(cfg.ResetPolicy, now);
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                int? sequence = await _repository.AllocateNextSequence(cfg.UserTypeID, period);
                if (sequence == null) break;

                string candidate = Render(cfg, sequence.Value, now);
                // Already taken (migrated data, a manual edit, a counter that fell behind) — burn
                // this number and take the next one rather than issue a duplicate.
                if (!await _repository.UserCodeExists(candidate)) return candidate;
            }

            return await UniqueFallback(cfg, now);
        }

        // ── Internals ────────────────────────────────────────────────────────────────────────

        /// <summary>The format a type gets before an admin configures one (not persisted here).</summary>
        private async Task<UserCodeFormat> BuildDefault(UserTypeMaster type)
        {
            var cfg = new UserCodeFormat
            {
                UserTypeID = type.UserTypeID,
                Code = DeriveCode(type),
                Pattern = DefaultPattern,
                SequenceLength = DefaultSequenceLength,
                StartNumber = 1,
                ResetPolicy = UserCodeResetPolicy.None,
                SequencePeriod = string.Empty,
                IsActive = true,
            };
            cfg.LastSequence = await SeedSequence(cfg, DateTime.Now);
            return cfg;
        }

        /// <summary>
        /// Starting code for a type nobody has configured yet: the known short code for the
        /// standard tiers, else the code already on the type master when it is short enough to be
        /// one, else its initials. An admin can change any of it on the User Alias screen.
        /// </summary>
        private static readonly Dictionary<int, string> DefaultCodes = new()
        {
            [UserTypeCode.Admin] = "AD",
            [UserTypeCode.SuperDistributor] = "SD",
            [UserTypeCode.Distributor] = "DS",
            [UserTypeCode.Retailer] = "RD",
            [UserTypeCode.ApiUser] = "AP",
        };

        private static string DeriveCode(UserTypeMaster type)
        {
            if (DefaultCodes.TryGetValue(type.UserTypeID, out var known)) return known;

            // The type master carries a display alias ("Retailer"), which is only a code when it
            // is short — a full word would make for a clumsy user code.
            string alias = Sanitise(type.AliasName ?? string.Empty).ToUpperInvariant();
            if (alias.Length is > 0 and <= 4) return alias;

            string name = Sanitise(type.UserType ?? string.Empty).ToUpperInvariant();
            if (name.Length >= 2) return name[..2];
            return string.IsNullOrEmpty(name) ? "USR" : name;
        }

        /// <summary>Highest number this shape has already reached among existing users.</summary>
        private async Task<int> SeedSequence(UserCodeFormat cfg, DateTime now)
        {
            var (prefix, suffix) = SplitAroundSequence(cfg, now);
            int inUse = await _repository.MaxSequenceInUse(cfg.UserTypeID, prefix, suffix, cfg.SequenceLength);
            return Math.Max(inUse, cfg.StartNumber - 1);
        }

        /// <summary>The next free code for a format, without consuming a number.</summary>
        private async Task<string> NextAvailableCode(UserCodeFormat cfg, DateTime now)
        {
            if (!cfg.IsActive) return string.Empty;

            string period = UserCodeResetPolicy.PeriodKey(cfg.ResetPolicy, now);
            int next = cfg.SequencePeriod == period
                ? Math.Max(cfg.LastSequence, cfg.StartNumber - 1) + 1
                : cfg.StartNumber;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                string candidate = Render(cfg, next + attempt, now);
                if (!await _repository.UserCodeExists(candidate)) return candidate;
            }
            return Render(cfg, next + MaxAttempts, now);
        }

        /// <summary>
        /// Last resort when every attempt collided or the counter is unreachable: a code that is
        /// still recognisably the type, made unique by the clock. User creation must never fail —
        /// and must never hand out a duplicate — because of the code generator.
        /// </summary>
        private async Task<string> UniqueFallback(UserCodeFormat cfg, DateTime now)
        {
            string prefix = string.IsNullOrEmpty(cfg.Code) ? "USR" : cfg.Code;
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                string candidate = $"{prefix}{now:yyMMddHHmmss}{attempt:00}";
                if (!await _repository.UserCodeExists(candidate)) return candidate;
            }
            return $"{prefix}{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
        }

        /// <summary>Renders the configured pattern for one running number.</summary>
        public static string Render(UserCodeFormat cfg, int sequence, DateTime now)
        {
            string pattern = string.IsNullOrWhiteSpace(cfg.Pattern) ? DefaultPattern : cfg.Pattern;
            int pad = cfg.SequenceLength <= 0 ? 1 : cfg.SequenceLength;

            var sb = new StringBuilder(pattern);
            ReplaceToken(sb, "{CODE}", cfg.Code ?? string.Empty);
            ReplaceToken(sb, "{YYYY}", now.ToString("yyyy"));
            ReplaceToken(sb, "{YY}", now.ToString("yy"));
            ReplaceToken(sb, "{MM}", now.ToString("MM"));
            ReplaceToken(sb, SequenceToken, sequence.ToString(new string('0', pad)));
            return sb.ToString();
        }

        /// <summary>The literal text on either side of the running number, for the current period.</summary>
        private static (string prefix, string suffix) SplitAroundSequence(UserCodeFormat cfg, DateTime now)
        {
            string pattern = string.IsNullOrWhiteSpace(cfg.Pattern) ? DefaultPattern : cfg.Pattern;
            int at = pattern.IndexOf(SequenceToken, StringComparison.OrdinalIgnoreCase);
            if (at < 0) return (RenderLiterals(pattern, cfg, now), string.Empty);

            return (RenderLiterals(pattern[..at], cfg, now),
                    RenderLiterals(pattern[(at + SequenceToken.Length)..], cfg, now));
        }

        /// <summary>Resolves every token except {SEQ} — the fixed part of a code.</summary>
        private static string RenderLiterals(string text, UserCodeFormat cfg, DateTime now)
        {
            var sb = new StringBuilder(text);
            ReplaceToken(sb, "{CODE}", cfg.Code ?? string.Empty);
            ReplaceToken(sb, "{YYYY}", now.ToString("yyyy"));
            ReplaceToken(sb, "{YY}", now.ToString("yy"));
            ReplaceToken(sb, "{MM}", now.ToString("MM"));
            return sb.ToString();
        }

        private static void ReplaceToken(StringBuilder sb, string token, string value)
        {
            string text = sb.ToString();
            int at = text.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            while (at >= 0)
            {
                sb.Remove(at, token.Length);
                sb.Insert(at, value);
                text = sb.ToString();
                at = text.IndexOf(token, at + value.Length, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>Codes are letters and digits only — separators belong in the pattern.</summary>
        private static string Sanitise(string value) =>
            new((value ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray());

        private static string Normalise(string resetPolicy) =>
            resetPolicy.Equals(UserCodeResetPolicy.Yearly, StringComparison.OrdinalIgnoreCase) ? UserCodeResetPolicy.Yearly
            : resetPolicy.Equals(UserCodeResetPolicy.Monthly, StringComparison.OrdinalIgnoreCase) ? UserCodeResetPolicy.Monthly
            : UserCodeResetPolicy.None;
    }
}
