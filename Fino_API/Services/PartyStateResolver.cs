using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Services;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IPartyStateResolver" />
    public class PartyStateResolver : IPartyStateResolver
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// StateID → GST state code, built once per process from StateMasters.
        ///
        /// It has to be built rather than stored because the two numbering systems are unrelated:
        /// StateMasters is seeded alphabetically (Andhra Pradesh = 1, Arunachal = 2, …) while the
        /// GST code is statutory and in a different order entirely (Andhra Pradesh = 37, Arunachal
        /// = 12). The join is the state's NAME, which both sides agree on.
        /// </summary>
        private static Dictionary<int, string>? _byStateId;
        private static readonly object Gate = new();

        public PartyStateResolver(ApplicationDbContext context) => _context = context;

        public async Task<string> Resolve(
            int userId, PartyTaxProfile? profile, string gstin, string? businessGstStateCode)
        {
            // 1. The verified record, where somebody set it deliberately.
            string fromProfile = Clean(profile?.PlaceOfSupplyStateCode);
            if (fromProfile.Length > 0) return fromProfile;

            // 2. What onboarding typed into the user's own GST state field.
            string fromUser = Clean(businessGstStateCode);
            if (fromUser.Length > 0) return fromUser;

            // 3. The first two digits of a GSTIN ARE its state code.
            if (!string.IsNullOrWhiteSpace(gstin) && gstin.Trim().Length == 15)
            {
                string fromGstin = Clean(gstin.Trim()[..2]);
                if (fromGstin.Length > 0) return fromGstin;
            }

            // 4. The state onboarding picked from the dropdown. This one is new, and it is the
            //    reason most parties need nothing typed at all: the address step already captured
            //    a state, it was simply never translated into the GST code.
            int stateId = await _context.UserMasters.AsNoTracking()
                .Where(u => u.UserMasterID == userId)
                .Select(u => u.StateID ?? 0)
                .FirstOrDefaultAsync();
            if (stateId > 0)
            {
                var map = await StateIdMap();
                if (map.TryGetValue(stateId, out var code)) return code;
            }

            // Nothing known. Empty, never "00" — see the note in TaxInvoiceService.LoadParty.
            return string.Empty;
        }

        public async Task<Dictionary<int, string>> StateIdMap()
        {
            if (_byStateId != null) return _byStateId;

            var states = await _context.StateMasters.AsNoTracking()
                .Select(s => new { s.StateID, s.StateName })
                .ToListAsync();

            var map = new Dictionary<int, string>();
            foreach (var s in states)
            {
                string code = GstStates.CodeForName(s.StateName);
                if (code.Length > 0) map[s.StateID] = code;
            }

            lock (Gate) { _byStateId = map; }
            return map;
        }

        /// <summary>A two-digit code, or empty. "7" becomes "07"; nothing becomes nothing.</summary>
        private static string Clean(string? raw)
        {
            string s = (raw ?? string.Empty).Trim();
            if (s.Length == 0) return string.Empty;
            if (s.Length > 2) s = s[..2];
            s = s.PadLeft(2, '0');
            // "00" is not a state. It is what the old padding produced from an empty value, and
            // some rows will carry it; treat it as "not set" rather than passing it on.
            return s == "00" ? string.Empty : s;
        }
    }
}
