namespace FintechApp.StaticModels
{
    /// <summary>
    /// The one place the GST fields on a rate card row are validated, so the provider band screen
    /// and the slab screen cannot drift into accepting different things.
    ///
    /// A blank value is ACCEPTED and means DEFAULT — fall through to the commission tax profile.
    /// An unrecognised one is refused outright rather than normalised away: silently reading a
    /// typed "Incl." as DEFAULT would leave somebody certain they had set the band to inclusive.
    /// </summary>
    public static class ChargeGstValidation
    {
        public const string TypeMessage =
            "GST type must be INCLUSIVE, EXCLUSIVE, NONE, or DEFAULT to follow the commission tax profile.";

        public const string RateMessage = "GST % must be between 0 and 100. Use 0 to take the rate from the tax engine.";

        public static bool IsKnownType(string? value)
        {
            string v = (value ?? string.Empty).Trim();
            if (v.Length == 0) return true;
            foreach (var known in ChargeGstType.All)
                if (v.Equals(known, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
