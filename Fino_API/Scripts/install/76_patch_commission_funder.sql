/*
    Who funded each tier's commission.

    Until now the platform operator funded every downline commission: a retailer's ₹10 and a
    distributor's ₹2 were both credited by admin off one shared rate card, and the posting code
    found the admin ancestor and debited it. That is still exactly what happens on an install that
    does not delegate.

    With delegation, the money moves one step at a time instead. Admin puts a reseller's whole net
    commission on the reseller's own line - ₹12 on the distributor line, rather than the old ₹10
    retailer + ₹2 distributor split - and credits the distributor that ₹12. The distributor's own
    card then says what the retailer gets, and the distributor's WALLET is debited for it. The ₹2
    margin is the gap between those two postings; nothing stores it, which is why it cannot
    disagree with the rate card.

    These three columns record who paid, per tier, at the moment the transaction was initiated.
    They are not re-derived at refund time on purpose: the refund path reverses the postings that
    were actually made, and a card re-priced in between would otherwise credit money back to
    somebody who never paid it.

    0 means the admin ancestor - the value every existing row takes, so nothing about already
    settled money changes.
*/

IF COL_LENGTH('dbo.TransactionMasters', 'RetailerCommissionFunderID') IS NULL
    ALTER TABLE dbo.TransactionMasters ADD RetailerCommissionFunderID INT NOT NULL CONSTRAINT DF_TxnMaster_RetailerFunder DEFAULT(0);
GO

IF COL_LENGTH('dbo.TransactionMasters', 'DistributerCommissionFunderID') IS NULL
    ALTER TABLE dbo.TransactionMasters ADD DistributerCommissionFunderID INT NOT NULL CONSTRAINT DF_TxnMaster_DistributerFunder DEFAULT(0);
GO

IF COL_LENGTH('dbo.TransactionMasters', 'SuperDistributerCommissionFunderID') IS NULL
    ALTER TABLE dbo.TransactionMasters ADD SuperDistributerCommissionFunderID INT NOT NULL CONSTRAINT DF_TxnMaster_SuperDistributerFunder DEFAULT(0);
GO

/*
    A reseller needs to find the transactions it funded - that is its purchase side, and the other
    half of the margin its own commission credits show. Filtered to the delegated rows, which on
    most installs is none of them.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TransactionMasters_RetailerFunder' AND object_id = OBJECT_ID('dbo.TransactionMasters'))
    CREATE INDEX IX_TransactionMasters_RetailerFunder
        ON dbo.TransactionMasters (RetailerCommissionFunderID, DateTime)
        WHERE RetailerCommissionFunderID > 0;
GO
