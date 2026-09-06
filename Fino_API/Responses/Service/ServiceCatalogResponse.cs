namespace FintechApp.Responses.Service
{
    /// <summary>One service category group (e.g. "Mobile Services") with its services and their operators.</summary>
    public class ServiceCatalogResponse
    {
        public string Category { get; set; } = string.Empty;
        public List<CatalogServiceItem> Services { get; set; } = new();
    }

    public class CatalogServiceItem
    {
        public int ServiceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceImagePath { get; set; } = string.Empty;
        public int UISequence { get; set; }
        public decimal Charge { get; set; }
        public decimal MinBalance { get; set; }
        public bool IsActive { get; set; }
        public bool IsDisplay { get; set; }
        public bool? IsBBPS { get; set; }
        /// <summary>Report mapping (ServiceKinds constant, empty = auto by keywords, NONE = no report).</summary>
        public string ReportKind { get; set; } = string.Empty;
        public int? WalletID { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public int OperatorCount { get; set; }
        /// <summary>The caller holds this service but cannot transact on it right now — an admin
        /// set it to "Lock" on their user record, or their KYC is not approved yet. It stays
        /// visible so they know it exists. Services set to "Off" are dropped from the catalog
        /// entirely.</summary>
        public bool IsLocked { get; set; }
        /// <summary>Why it is locked, for the tile ("KYC pending" / "Locked by your service
        /// provider"). Empty when the service is usable.</summary>
        public string LockReason { get; set; } = string.Empty;
        public List<CatalogOperatorItem> Operators { get; set; } = new();
    }

    public class CatalogOperatorItem
    {
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string OperatorImagePath { get; set; } = string.Empty;
        public string Opcode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
