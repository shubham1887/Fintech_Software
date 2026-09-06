using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using System.Text.RegularExpressions;

namespace FintechApp.Services
{
    /// <summary>
    /// Resolves an app OperatorMaster row to the plan provider's OpCode (see
    /// <see cref="PlanApiCatalog"/>). Resolution order, most authoritative first:
    ///
    ///   1. appsettings "PlanApi:OperatorCodes:&lt;OperatorID&gt;" — a per-deployment override, so a
    ///      reseller can correct any mapping without a code change.
    ///   2. OperatorMaster.Opcode, when the admin has already stored a numeric code the provider
    ///      publishes (a deployment that keys its master off the plan provider directly).
    ///   3. A curated alias for the operators whose names differ between the two catalogs
    ///      ("Tata Play" ⇄ "TATA SKY", "d2h" ⇄ "VIDEOCON D2H", "Airtel Digital TV" ⇄ "AIRTEL DTH").
    ///   4. A name match against catalog entries of the same type (prepaid / DTH / postpaid …).
    ///
    /// An unresolved operator returns an empty string; the caller then reports that plans are not
    /// available rather than sending the provider a code it will reject.
    /// </summary>
    public class PlanOperatorCodeService : IPlanOperatorCodeService
    {
        private const string OverrideSection = "PlanApi:OperatorCodes";

        private readonly IOperatorRepository _operators;
        private readonly IConfiguration _config;

        public PlanOperatorCodeService(IOperatorRepository operators, IConfiguration config)
        {
            _operators = operators;
            _config = config;
        }

        // Comparison key: case/spacing/punctuation-insensitive ("Vodafone Idea (Vi)" → "vodafoneideavi").
        private static string Key(string value) => Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9]", "").ToLowerInvariant();

        /// <summary>
        /// Operators whose app name does not match the provider's name closely enough for the
        /// generic match to be safe. Keyed by provider type, then by the normalised app name.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> Aliases = new(StringComparer.OrdinalIgnoreCase)
        {
            [PlanApiCatalog.OperatorType.Prepaid] = new()
            {
                ["airtel"] = "2",
                ["bsnl"] = "4",              // BSNL TOPUP (BSNL SPECIAL is "5")
                ["bsnltopup"] = "4",
                ["bsnlspecial"] = "5",
                ["idea"] = "6",
                ["jio"] = "11",
                ["reliancejio"] = "11",
                ["vi"] = "23",
                ["vodafone"] = "23",
                ["vodafoneidea"] = "23",
                ["vodafoneideavi"] = "23",
            },
            [PlanApiCatalog.OperatorType.Dth] = new()
            {
                ["airteldth"] = "24",
                ["airteldigitaltv"] = "24",
                ["dishtv"] = "25",
                ["reliancebigtv"] = "26",
                ["bigtv"] = "26",
                ["sundirect"] = "27",
                ["tatasky"] = "28",
                ["tataplay"] = "28",
                ["d2h"] = "29",
                ["videocond2h"] = "29",
                ["videocon"] = "29",
            },
            [PlanApiCatalog.OperatorType.Postpaid] = new()
            {
                ["bsnl"] = "33",
                ["airtel"] = "34",
                ["idea"] = "35",
                ["vi"] = "36",
                ["vodafone"] = "36",
                ["vodafoneidea"] = "36",
                ["vodafoneideavi"] = "36",
                ["aircel"] = "40",
                ["jio"] = "491",
                ["reliancejio"] = "491",
            },
        };

        /// <summary>
        /// The provider type a service belongs to. DTH is checked before prepaid because a DTH
        /// service is often named "DTH Recharge", which also contains "recharge".
        /// </summary>
        private static string TypeOf(string serviceName, string operatorName)
        {
            string text = $"{serviceName} {operatorName}".ToLowerInvariant();
            if (text.Contains("dth") || text.Contains("d2h")) return PlanApiCatalog.OperatorType.Dth;
            if (text.Contains("postpaid")) return PlanApiCatalog.OperatorType.Postpaid;
            if (text.Contains("prepaid") || text.Contains("mobile") || text.Contains("recharge")) return PlanApiCatalog.OperatorType.Prepaid;
            if (text.Contains("landline")) return PlanApiCatalog.OperatorType.Landline;
            if (text.Contains("broadband") || text.Contains("fiber") || text.Contains("internet")) return PlanApiCatalog.OperatorType.Broadband;
            if (text.Contains("fastag")) return PlanApiCatalog.OperatorType.Fastag;
            if (text.Contains("electric")) return PlanApiCatalog.OperatorType.Electricity;
            if (text.Contains("water")) return PlanApiCatalog.OperatorType.Water;
            if (text.Contains("insurance")) return PlanApiCatalog.OperatorType.Insurance;
            if (text.Contains("gas")) return PlanApiCatalog.OperatorType.Gaspipeline;
            return string.Empty;
        }

        public async Task<string> ResolveOpCode(int operatorId)
        {
            if (operatorId <= 0) return string.Empty;

            // 1. Per-deployment override.
            string configured = (_config[$"{OverrideSection}:{operatorId}"] ?? string.Empty).Trim();
            if (configured.Length > 0) return configured;

            var op = await _operators.GetOperatorById(operatorId);
            if (op == null) return string.Empty;

            // 2. The master already holds a code the provider publishes.
            string stored = (op.Opcode ?? string.Empty).Trim();
            if (stored.Length > 0 && stored.All(char.IsDigit) && PlanApiCatalog.IsKnownCode(stored)) return stored;

            string serviceName = op.ServiceMaster?.Name ?? string.Empty;
            string type = TypeOf(serviceName, op.OperatorName);
            if (type.Length == 0) return string.Empty;
            string name = Key(op.OperatorName);
            if (name.Length == 0) return string.Empty;

            // 3. Curated alias for this provider type.
            if (Aliases.TryGetValue(type, out var aliases) && aliases.TryGetValue(name, out var aliased)) return aliased;

            // 4. Name match inside the same provider type — exact first, then either side containing
            //    the other (so "Airtel Xstream Fiber" still finds "AirtelBroadband" but "ION" never
            //    swallows an unrelated longer name).
            var candidates = PlanApiCatalog.OfType(type);
            var exact = candidates.FirstOrDefault(c => Key(c.Name) == name);
            if (exact != null) return exact.OpCode;

            var partial = candidates.FirstOrDefault(c =>
            {
                string candidate = Key(c.Name);
                return candidate.Length >= 4 && name.Length >= 4 && (candidate.Contains(name) || name.Contains(candidate));
            });
            return partial?.OpCode ?? string.Empty;
        }
    }
}
