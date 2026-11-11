/* ============================================================================
   Mobile layout: composition settings
   ----------------------------------------------------------------------------
   The phone/app home screen is now assembled from interchangeable blocks rather
   than being one screen with a few knobs on it. Three new UISetting keys say
   WHICH block renders in each slot:

       MobileBalanceStyle    card | hero | strip | plain | split
       MobileServiceStyle    grid | dense | cards | list | sections | focus
       MobileActivityStyle   list | compact | timeline | table

   The app falls back to the preset's own blocks when a key is missing, so this
   script is not required for correctness - it exists so that the rows are
   visible (and editable) in Settings -> Appearance on an install that was set
   up before the blocks existed.

   The seeded value is derived from the preset THIS database already has, so an
   existing install keeps exactly the screen it had. 46_patch_app_notifications
   seeded MobileLayout itself; it is shipped and must not be edited, hence this
   separate script.
   ========================================================================= */

IF OBJECT_ID(N'[UISettingMasters]') IS NOT NULL
BEGIN
    DECLARE @preset NVARCHAR(50) = (
        SELECT TOP 1 LOWER(LTRIM(RTRIM([Value])))
        FROM [UISettingMasters]
        WHERE [Key] = 'MobileLayout' AND [IsDeleted] = 0
    );
    SET @preset = ISNULL(NULLIF(@preset, ''), 'classic');

    ;WITH seed([Key], [Value], [Description]) AS (
        SELECT 'MobileBalanceStyle',
               CASE @preset
                   WHEN 'compact'  THEN 'strip'
                   WHEN 'vibrant'  THEN 'hero'
                   WHEN 'minimal'  THEN 'plain'
                   WHEN 'business' THEN 'split'
                   ELSE 'card'
               END,
               'Balance block on the phone home screen: card | hero | strip | plain | split'

        UNION ALL
        SELECT 'MobileServiceStyle',
               CASE @preset
                   WHEN 'compact'  THEN 'dense'
                   WHEN 'vibrant'  THEN 'cards'
                   WHEN 'minimal'  THEN 'list'
                   WHEN 'business' THEN 'sections'
                   WHEN 'focus'    THEN 'focus'
                   ELSE 'grid'
               END,
               'Service catalogue on the phone home screen: grid | dense | cards | list | sections | focus'

        UNION ALL
        SELECT 'MobileActivityStyle',
               CASE @preset
                   WHEN 'compact'  THEN 'compact'
                   WHEN 'minimal'  THEN 'timeline'
                   WHEN 'business' THEN 'table'
                   WHEN 'focus'    THEN 'compact'
                   ELSE 'list'
               END,
               'Recent transactions on the phone home screen: list | compact | timeline | table'
    )
    INSERT INTO [UISettingMasters] ([Key], [Value], [Description], [DateTime], [UpdatedateTime], [IsDeleted])
    SELECT s.[Key], s.[Value], s.[Description], SYSUTCDATETIME(), SYSUTCDATETIME(), 0
    FROM seed s
    WHERE NOT EXISTS (
        SELECT 1 FROM [UISettingMasters] u WHERE u.[Key] = s.[Key] AND u.[IsDeleted] = 0
    );

    -- "focus" is a sixth preset; the description the admin reads should list it.
    UPDATE [UISettingMasters]
       SET [Description] = 'Phone/app home layout preset: classic | compact | vibrant | minimal | business | focus',
           [UpdatedateTime] = SYSUTCDATETIME()
     WHERE [Key] = 'MobileLayout'
       AND [IsDeleted] = 0
       AND [Description] <> 'Phone/app home layout preset: classic | compact | vibrant | minimal | business | focus';

    UPDATE [UISettingMasters]
       SET [Description] = 'Show the balance block on the phone home screen (true | false)',
           [UpdatedateTime] = SYSUTCDATETIME()
     WHERE [Key] = 'MobileShowBalanceCard'
       AND [IsDeleted] = 0
       AND [Description] <> 'Show the balance block on the phone home screen (true | false)';
END;
GO
