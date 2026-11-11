/*
    UserMasters.SessionEpoch - the generation number of a user's live sessions.

    Access tokens in this API are self-contained JWTs. Signing out revoked the REFRESH token, so
    the session could not be extended, but the access token already issued kept working until it
    expired - up to JwtSettings:ExpiryMinutes later. That left two real problems:

      * Two browser tabs signed in as the same user share one localStorage token. Signing out in
        one tab left the other tab holding a token the server still honoured, so it went on
        transacting as a user who believed they had signed out.
      * A token copied off the wire, or off a shared retail machine, stayed usable for the rest of
        its hour with no way to cancel it.

    Every access token now carries the generation it was minted under (claim "sepoch"), and
    JwtAuthorizationAttribute refuses a token whose generation is not the current one. Signing out
    and changing a password move this column on by one, which ends every session at once.

    Existing tokens carry no claim and read as generation 0, which matches the default below - so
    nobody signed in at the moment this is applied is thrown out; they keep working until their
    next sign-out.

    Idempotent - safe to re-run.
*/

IF COL_LENGTH(N'dbo.UserMasters', N'SessionEpoch') IS NULL
BEGIN
    ALTER TABLE dbo.UserMasters
        ADD [SessionEpoch] INT NOT NULL CONSTRAINT DF_UserMasters_SessionEpoch DEFAULT (0);
END
GO
