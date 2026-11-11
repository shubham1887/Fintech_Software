/*
    Auto-forward every complaint to the provider that carried the transaction.

    What was wrong: the forwarder existed but could never fire. It keys off the complaint's
    ProviderID, and nothing on the "raise a complaint" form asks a retailer which upstream rail
    carried their recharge - they have no idea, and should not - so ProviderID arrived as 0 and the
    forward returned immediately. The code side now reads the provider, its request id and its
    reference off the transaction being complained about. This side gives it somewhere to send to.

    Two things here:

      1. ProviderComplaintID on ComplaintMasters - the complaint number the vendor hands back
         ({"Status":"SUCCESS","ComplaintID":"34669"}). It is the only handle for chasing the
         complaint with them afterwards, and buried in a free-text remark it cannot be searched.

      2. A COMPLAINT endpoint per provider that actually carries recharges, built from THAT
         provider's own recharge URL - same scheme, same host, same /RechargeAPI/v1/ path, with
         RaiseComplaint on the end. Deliberately derived rather than typed: these rails are
         white-labelled (api.payin.live and api.jaijuirecharge.in are the same platform under two
         names), so hard-coding one vendor's host would send one client's complaints to another
         client's vendor, with that vendor's token. Deriving it means each install forwards to the
         gateway it is actually buying from, and an install with no recharge provider configured
         gets no row and forwards nothing.

    Credentials are NOT written here. The URL carries [UserName] and [Token], filled at send time
    from the provider's own LoginID / Token - exactly as the recharge URLs on the same rails
    already do - so nothing has to be retyped per client and no secret lands in this script.
*/

-------------------------------------------------------------------------------
-- 1. Where the vendor's complaint number is kept.
-------------------------------------------------------------------------------
IF COL_LENGTH('dbo.ComplaintMasters', 'ProviderComplaintID') IS NULL
    ALTER TABLE dbo.ComplaintMasters ADD ProviderComplaintID VARCHAR(100) NULL;
GO

-- Chasing a vendor starts from their number, so it has to be findable by it.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ComplaintMasters_ProviderComplaintID'
               AND object_id = OBJECT_ID('dbo.ComplaintMasters'))
    CREATE INDEX IX_ComplaintMasters_ProviderComplaintID
        ON dbo.ComplaintMasters (ProviderComplaintID)
        WHERE ProviderComplaintID IS NOT NULL;
GO

-------------------------------------------------------------------------------
-- 2. The COMPLAINT endpoint, one per provider that has a recharge endpoint.
--
-- GET is used rather than POST because the vendor documents both and the GET form needs no
-- header: everything is in the query string, so there is one less thing configured wrongly on a
-- client install. The POST form works too if an operator switches MethodType to POST and puts
-- KeyData in HeaderJson - the code builds a real JSON body from the same query string either way.
-------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.ProviderDetails', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.ProviderMasters', N'U') IS NOT NULL
BEGIN
    DECLARE @added INT = 0;

    -- One source row per provider: its recharge TRANSACTION endpoint, which is the only place the
    -- vendor's real host and API path are recorded. Done in two steps on purpose — the query
    -- string has to be cut off BEFORE the path is measured, or the length and the search run
    -- against two different strings and the result is the old URL with the new one glued onto it.
    ;WITH stripped AS (
        SELECT
            pd.ProviderDetailID,
            pd.ProviderID,
            pd.ServiceID,
            CASE WHEN CHARINDEX('?', pd.ProviderURL) > 0
                 THEN LEFT(pd.ProviderURL, CHARINDEX('?', pd.ProviderURL) - 1)
                 ELSE pd.ProviderURL END AS PathOnly
        FROM dbo.ProviderDetails pd
        JOIN dbo.ProviderMasters pm ON pm.ProviderID = pd.ProviderID
        WHERE pd.IsActive = 1
          AND pd.IsDeleted = 0
          AND pm.IsActive = 1
          AND pm.IsDelete = 0
          AND UPPER(pd.UrlType) = 'TRANSACTION'
          AND pd.ProviderURL LIKE '%/RechargeAPI/%'
          -- A placeholder provider is not a gateway; forwarding a customer's complaint to
          -- google.com would look like it worked and reach nobody.
          AND pd.ProviderURL NOT LIKE '%google.com%'
    ),
    src AS (
        SELECT
            ProviderID,
            ServiceID,
            -- Everything up to and including the last "/" — i.e. the vendor's host plus its API
            -- path prefix ("https://host/RechargeAPI/v1/"), with the action name dropped.
            LEFT(PathOnly, LEN(PathOnly) - CHARINDEX('/', REVERSE(PathOnly)) + 1) AS BasePath,
            ROW_NUMBER() OVER (PARTITION BY ProviderID ORDER BY ProviderDetailID) AS rn
        FROM stripped
    )
    INSERT INTO dbo.ProviderDetails
        (ProviderID, ServiceID, UrlType, ProviderURL, MethodType, Balance, HeaderJson,
         DateFormat, IsActive, TransactionIDPrefix, [DateTime], UpdatedateTime, IsDeleted)
    SELECT
        s.ProviderID,
        s.ServiceID,
        'COMPLAINT',
        s.BasePath + 'RaiseComplaint?Mobile=[UserName]&APIToken=[Token]'
            + '&APIUserRequestID=[APIUserRequestID]&ConsumerNumber=[Number]'
            + '&CustomerRemark=[Remark]&Amount=[Amount]',
        'GET',
        0,
        '',
        '',
        1,
        '',
        SYSUTCDATETIME(), SYSUTCDATETIME(), 0
    FROM src s
    WHERE s.rn = 1
      AND NOT EXISTS (
            SELECT 1 FROM dbo.ProviderDetails x
            WHERE x.ProviderID = s.ProviderID
              AND x.IsDeleted = 0
              AND UPPER(x.UrlType) IN ('COMPLAINT', 'COMPLAIN', 'DISPUTE', 'GRIEVANCE'));

    SET @added = @@ROWCOUNT;

    IF @added > 0
    BEGIN
        PRINT 'Complaint forwarding: added ' + CAST(@added AS varchar(10))
            + ' COMPLAINT endpoint(s), derived from each provider''s own recharge URL.';
        PRINT 'Check them under Providers -> Endpoints, and make sure each provider''s Login ID';
        PRINT '  (registered mobile) and Token are set under Providers -> Add API - the URL reads';
        PRINT '  [UserName] and [Token] from there at send time.';
    END
    ELSE
    BEGIN
        PRINT 'Complaint forwarding: no COMPLAINT endpoint added (one already exists, or no';
        PRINT '  recharge provider is configured on this install).';
    END
END
GO
