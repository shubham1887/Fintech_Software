namespace FintechApp.Responses.SlabMaster
{
    public class SlabMasterResponse
    {
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }

        // ── Delegation ─────────────────────────────────────────────────────────────────────
        // Carried on the admin listing so the operator can tell their OWN rate cards from the
        // ones resellers cut below them. Without it the list is a flat set of names and the
        // operator cannot see, at a glance, which card is theirs to edit.
        public string OwnerName { get; set; } = "Admin";
        public bool IsDelegatedSlab { get; set; }
        public string ParentSlabName { get; set; } = string.Empty;

        /// <summary>ACTIVE, or INVALID when a band on it exceeds the card it was cut from.</summary>
        public string Status { get; set; } = string.Empty;
    }

    public class SlabMasterDropdownResponse
    {
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
    }
}
