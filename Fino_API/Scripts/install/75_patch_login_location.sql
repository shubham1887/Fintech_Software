/*
    Where a sign-in happened.

    LoginHistories has carried a Location column since the beginning and nothing ever wrote to it:
    the login path set it to an empty string unconditionally, so the Login History report's
    Location column was blank on every row ever recorded - a report about where people sign in
    from, with the one field that answers that permanently empty.

    This adds the coordinates as their own columns, so the report has a latitude and a longitude to
    show (and a map link to build) rather than one free-text field that may or may not hold a pair.
    Text rather than DECIMAL on purpose: every client sends them as strings, and an empty value has
    to mean "the device did not say" - which a numeric column cannot express, since 0,0 is a real
    place in the Atlantic.

    Nothing is back-filled, because there is nothing to back-fill: no row has ever had a location.
    Rows written from here on carry whatever the signing-in client could supply, and a client that
    supplies nothing still signs in exactly as before.

    Idempotent - safe to re-run.
*/

IF COL_LENGTH(N'dbo.LoginHistories', N'Latitude') IS NULL
BEGIN
    ALTER TABLE dbo.LoginHistories ADD
        [Latitude]  VARCHAR(32) NOT NULL CONSTRAINT DF_LoginHistories_Latitude DEFAULT (''),
        [Longitude] VARCHAR(32) NOT NULL CONSTRAINT DF_LoginHistories_Longitude DEFAULT ('');
END
GO
