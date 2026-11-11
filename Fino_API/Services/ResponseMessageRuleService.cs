using System.Text.RegularExpressions;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ResponseMessageRule;
using FintechApp.Responses.ResponseMessageRule;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// Rewrites user-facing messages from a table the admin edits (Settings → Response Messages).
    ///
    /// The rules are ordered by Sequence and the FIRST match wins, so "order is priority" is the
    /// whole mental model an operator needs: put the specific rule above the general one. A rule
    /// can be narrowed to one provider, one service and one channel; 0 / ALL mean "anything".
    ///
    /// Two deliberate limits:
    ///   • Nothing here edits what we STORE. The provider log keeps the upstream reply verbatim
    ///     and the transaction keeps its own description, so support can always see what really
    ///     came back — only the words the user reads change.
    ///   • A broken rule is never fatal. A regex that does not compile, a rule list that cannot be
    ///     read: the original message goes out unchanged rather than an error taking the place of
    ///     a successful transaction's reply.
    /// </summary>
    public class ResponseMessageRuleService : IResponseMessageRuleService
    {
        private readonly IResponseMessageRuleRepository _repository;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly Logger _logger = new();

        /// <summary>A pathological pattern must not hold a response open.</summary>
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(50);

        public ResponseMessageRuleService(
            IResponseMessageRuleRepository repository,
            IProviderMasterRepository providerRepo,
            IServiceRepository serviceRepo)
        {
            _repository = repository;
            _providerRepo = providerRepo;
            _serviceRepo = serviceRepo;
        }

        // ── Reading ──────────────────────────────────────────────────────────

        public async Task<List<ResponseMessageRuleResponse>> GetAll()
        {
            var rules = await _repository.GetAll();
            var rows = rules.Select(Map).ToList();
            await Enrich(rows);
            return rows;
        }

        public async Task<ResponseMessageRuleResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            if (entity == null) return null;
            var rows = new List<ResponseMessageRuleResponse> { Map(entity) };
            await Enrich(rows);
            return rows[0];
        }

        /// <summary>Names for the ids, so the list reads "JaiJui / DMT" instead of "7 / 3".</summary>
        private async Task Enrich(List<ResponseMessageRuleResponse> rows)
        {
            if (rows.Count == 0) return;
            try
            {
                var providers = (await _providerRepo.GetAll())
                    .GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);
                var services = (await _serviceRepo.GetAllServices())
                    .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);

                foreach (var r in rows)
                {
                    r.ProviderName = r.ProviderID > 0 && providers.TryGetValue(r.ProviderID, out var pn) ? pn : "All providers";
                    r.ServiceName = r.ServiceID > 0 && services.TryGetValue(r.ServiceID, out var sn) ? sn : "All services";
                }
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        private static ResponseMessageRuleResponse Map(ResponseMessageRule e) => new()
        {
            ResponseMessageRuleID = e.ResponseMessageRuleID,
            ProviderID = e.ProviderID,
            ServiceID = e.ServiceID,
            MatchType = MessageMatchType.Normalize(e.MatchType),
            Pattern = e.Pattern,
            Replacement = e.Replacement,
            Channel = MessageChannel.Normalize(e.Channel),
            Sequence = e.Sequence,
            Note = e.Note,
            IsActive = e.IsActive,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime,
        };

        // ── Writing ──────────────────────────────────────────────────────────

        public async Task<string> Create(CreateResponseMessageRuleRequest request)
        {
            string invalid = Validate(request.MatchType, request.Pattern);
            if (!invalid.EqualsIgnoreCase(StatusType.Ok)) return invalid;

            DateTime now = DateTime.Now;
            await _repository.Insert(new ResponseMessageRule
            {
                ProviderID = Math.Max(request.ProviderID, 0),
                ServiceID = Math.Max(request.ServiceID, 0),
                MatchType = MessageMatchType.Normalize(request.MatchType),
                Pattern = (request.Pattern ?? string.Empty).Trim(),
                Replacement = (request.Replacement ?? string.Empty).Trim(),
                Channel = MessageChannel.Normalize(request.Channel),
                Sequence = request.Sequence,
                Note = (request.Note ?? string.Empty).Trim(),
                IsActive = request.IsActive,
                DateTime = now,
                UpdatedateTime = now,
            });
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateResponseMessageRuleRequest request)
        {
            string invalid = Validate(request.MatchType, request.Pattern);
            if (!invalid.EqualsIgnoreCase(StatusType.Ok)) return invalid;

            var entity = await _repository.GetByID(request.ResponseMessageRuleID);
            if (entity == null) return Messages.ResponseMessageRuleMessages.NotFound;

            entity.ProviderID = Math.Max(request.ProviderID, 0);
            entity.ServiceID = Math.Max(request.ServiceID, 0);
            entity.MatchType = MessageMatchType.Normalize(request.MatchType);
            entity.Pattern = (request.Pattern ?? string.Empty).Trim();
            entity.Replacement = (request.Replacement ?? string.Empty).Trim();
            entity.Channel = MessageChannel.Normalize(request.Channel);
            entity.Sequence = request.Sequence;
            entity.Note = (request.Note ?? string.Empty).Trim();
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.Now;

            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ResponseMessageRuleMessages.NotFound;
        }

        /// <summary>A regex is saved only if it compiles — otherwise it would silently never match.</summary>
        private static string Validate(string matchType, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return Messages.ResponseMessageRuleMessages.PatternRequired;
            if (MessageMatchType.Normalize(matchType) != MessageMatchType.Regex) return StatusType.Ok;
            try { _ = new Regex(pattern, RegexOptions.IgnoreCase, RegexTimeout); return StatusType.Ok; }
            catch (ArgumentException) { return Messages.ResponseMessageRuleMessages.InvalidRegex; }
        }

        // ── Applying ─────────────────────────────────────────────────────────

        public async Task<string> Apply(string message, int providerID, int serviceID, string channel)
        {
            var (_, result) = await Resolve(message, providerID, serviceID, channel);
            return result;
        }

        public async Task<TestResponseMessageResponse> Test(TestResponseMessageRequest request)
        {
            var (rule, result) = await Resolve(request.Message, request.ProviderID, request.ServiceID, request.Channel);

            return new TestResponseMessageResponse
            {
                Matched = rule != null,
                MatchedRuleID = rule?.ResponseMessageRuleID ?? 0,
                MatchedPattern = rule?.Pattern ?? string.Empty,
                MatchType = rule == null ? string.Empty : MessageMatchType.Normalize(rule.MatchType),
                Original = request.Message ?? string.Empty,
                Result = result,
                Message = rule == null
                    ? Messages.ResponseMessageRuleMessages.NoRuleMatched
                    : Messages.ResponseMessageRuleMessages.RuleMatched,
            };
        }

        /// <summary>The matcher itself: first active, in-scope rule whose pattern hits.</summary>
        private async Task<(ResponseMessageRule? rule, string result)> Resolve(
            string message, int providerID, int serviceID, string channel)
        {
            string original = message ?? string.Empty;
            if (original.Trim().Length == 0) return (null, original);

            try
            {
                var rules = await _repository.GetAll();
                foreach (var rule in rules)
                {
                    if (!rule.IsActive) continue;
                    // 0 on the rule means "any"; a rule pinned to a provider or service only fires
                    // where that provider/service is actually known to be the source.
                    if (rule.ProviderID > 0 && rule.ProviderID != providerID) continue;
                    if (rule.ServiceID > 0 && rule.ServiceID != serviceID) continue;
                    if (!MessageChannel.Covers(rule.Channel, channel)) continue;
                    if (!IsMatch(rule, original)) continue;

                    return (rule, rule.Replacement ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                // Rewriting is cosmetic; the reply is not. Send the original rather than fail.
                _logger.Write(ex);
            }

            return (null, original);
        }

        private bool IsMatch(ResponseMessageRule rule, string message)
        {
            string pattern = (rule.Pattern ?? string.Empty).Trim();
            if (pattern.Length == 0) return false;

            try
            {
                return MessageMatchType.Normalize(rule.MatchType) switch
                {
                    MessageMatchType.Equals => string.Equals(message.Trim(), pattern, StringComparison.OrdinalIgnoreCase),
                    MessageMatchType.StartsWith => message.TrimStart().StartsWith(pattern, StringComparison.OrdinalIgnoreCase),
                    MessageMatchType.Regex => Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase, RegexTimeout),
                    _ => message.Contains(pattern, StringComparison.OrdinalIgnoreCase),
                };
            }
            catch (Exception ex)
            {
                // A bad or runaway pattern disables ITSELF, not the whole chain.
                _logger.Write(ex);
                return false;
            }
        }
    }
}
