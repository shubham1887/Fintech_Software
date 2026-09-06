namespace Project.StaticModel
{
    public static class FundTransferType
    {
        internal const string TransferRequest = "TRANSFER_REQUEST";
        internal const string Transfer = "TRANSFER";
        internal const string Reverse = "REVERSE";
        internal const string ReverseRequest = "REVERSE_REQUEST";

        internal const string Raised = "RAISED";
        internal const string Received = "RECEIVED";
        internal const string PGReceived = "PG-RECEIVED";
        internal const string ECReceived = "EC-RECEIVED";

        internal const string All = "ALL";
        internal const string ReverseCredit = "REVERSECREDIT";
        internal const string ThroughDirectTransfer = "DIRECT_TRANSFER";
        internal const string ThroughDirectReverse = "DIRECT_REVERSE";
        internal const string ThroughWalletReverse = "WALLET_REVERSE";
        internal const string ThroughWalletTransfer = "WALLET_TRANSFER";
        internal const string ThroughWalletToOther = "WALLET_TO_OTHER";
        internal const string TotalTransfer = "TOTAL_TRANSFER";
        internal const string ThroughDirectAutoTransfer = "AUTO_DIRECT_TRANSFER";

        internal const string ThroughRequestTransfer = "BY_REQUEST_TRANSFER";
        internal const string ThroughReverseRequestTransfer = "BY_REVERSE_REQUEST_TRANSFER";
        internal const string ECollection = "E-COLLECTION";
        internal const string PGTransfer = "PG-TRANSFER";
        internal const string RefrralTransfer = "RefrralTransfer";
    }
}