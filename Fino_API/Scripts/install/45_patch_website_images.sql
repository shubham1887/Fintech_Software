/* ============================================================================
   Public website — image enrichment for Home / Services / About / Contact
   ----------------------------------------------------------------------------
   The four pages already exist in dbo.WebsitePageMasters with good copy, but
   every hero slide and text section had an EMPTY image, so the public site
   (PublicSite.tsx) rendered text on a flat gradient — no real image slider,
   which looked plain. This script injects high-quality images (and a map on
   Contact, plus an image banner on About/Contact) WITHOUT touching any copy.

   How it works: surgical REPLACE on unique anchor strings inside SectionsJson,
   so all existing text/HTML is preserved exactly. Each page is only updated if
   it does not already contain an image URL, so this is IDEMPOTENT — re-running
   is a no-op and never duplicates images.

   Images are Unsplash stock (verified reachable). Swap any URL for your own
   asset later in the admin Website Builder (Settings -> Website).

   NOTE: the public endpoint / repo caches pages in memory — after this seed,
   RESTART THE API (or re-save any page in the Website Builder) so the images
   show up on the live site.
   ============================================================================ */
SET NOCOUNT ON;

DECLARE @img_pay      nvarchar(200) = N'https://images.unsplash.com/photo-1563986768609-322da13575f3?auto=format&fit=crop&w=1200&q=80'; -- mobile payment
DECLARE @img_phone    nvarchar(200) = N'https://images.unsplash.com/photo-1601597111158-2fceff292cdc?auto=format&fit=crop&w=1200&q=80'; -- hand + phone
DECLARE @img_dash     nvarchar(200) = N'https://images.unsplash.com/photo-1460925895917-afdab827c52f?auto=format&fit=crop&w=1200&q=80'; -- analytics dashboard
DECLARE @img_charts   nvarchar(200) = N'https://images.unsplash.com/photo-1579621970563-ebec7560ff3e?auto=format&fit=crop&w=1200&q=80'; -- finance charts
DECLARE @img_laptop   nvarchar(200) = N'https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?auto=format&fit=crop&w=1200&q=80'; -- laptop finance
DECLARE @img_cards    nvarchar(200) = N'https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?auto=format&fit=crop&w=1200&q=80'; -- cards / money
DECLARE @img_hands    nvarchar(200) = N'https://images.unsplash.com/photo-1521791136064-7986c2920216?auto=format&fit=crop&w=1200&q=80'; -- handshake
DECLARE @img_trade    nvarchar(200) = N'https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=1200&q=80'; -- trading screen

/* --------------------------------------------------------------------- HOME */
UPDATE dbo.WebsitePageMasters
SET SectionsJson =
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(SectionsJson,
      N'"ctaText":"Become a Partner"',            N'"image":"' + @img_pay   + N'","ctaText":"Become a Partner"'),
      N'"ctaText":"Explore Services"',            N'"image":"' + @img_phone + N'","ctaText":"Explore Services"'),
      N'"ctaText":"Partner Login"',               N'"image":"' + @img_dash  + N'","ctaText":"Partner Login"'),
      N'"side":"right","html":"<p>Fino is a complete',
                                                  N'"side":"right","image":"' + @img_charts + N'","html":"<p>Fino is a complete'),
    UpdatedateTime = SYSUTCDATETIME()
WHERE Slug = 'home' AND SectionsJson NOT LIKE N'%"image":"http%';

/* ----------------------------------------------------------------- SERVICES */
UPDATE dbo.WebsitePageMasters
SET SectionsJson =
    REPLACE(
    REPLACE(SectionsJson,
      N'"ctaText":"Become a Partner"',            N'"image":"' + @img_laptop + N'","ctaText":"Become a Partner"'),
      N'"side":"left","html":"<p>Every service carries',
                                                  N'"side":"left","image":"' + @img_cards + N'","html":"<p>Every service carries'),
    UpdatedateTime = SYSUTCDATETIME()
WHERE Slug = 'services' AND SectionsJson NOT LIKE N'%"image":"http%';

/* -------------------------------------------------------------------- ABOUT */
/* Prepend a hero image banner, and give the "Who we are" block an image. */
UPDATE dbo.WebsitePageMasters
SET SectionsJson =
    REPLACE(
    REPLACE(SectionsJson,
      N'[{"type":"textImage","heading":"Who we are"',
      N'[{"type":"hero","slides":[{"heading":"About Fino","sub":"A technology company bringing every neighbourhood store into the digital-payments economy.","image":"' + @img_hands + N'"}]},{"type":"textImage","heading":"Who we are"'),
      N'"side":"right","html":"<p>Fino is a technology company',
      N'"side":"right","image":"' + @img_trade + N'","html":"<p>Fino is a technology company'),
    UpdatedateTime = SYSUTCDATETIME()
WHERE Slug = 'about' AND SectionsJson NOT LIKE N'%"image":"http%';

/* ------------------------------------------------------------------ CONTACT */
/* Prepend a hero banner, give the intro block an image, and embed a map. */
UPDATE dbo.WebsitePageMasters
SET SectionsJson =
    REPLACE(
    REPLACE(
    REPLACE(SectionsJson,
      N'[{"type":"textImage","heading":"We would love to hear from you"',
      N'[{"type":"hero","slides":[{"heading":"Get in touch","sub":"Our support desk is open seven days a week, 9 AM to 9 PM.","image":"' + @img_cards + N'"}]},{"type":"textImage","heading":"We would love to hear from you"'),
      N'"side":"right","html":"<p>Questions about onboarding',
      N'"side":"right","image":"' + @img_phone + N'","html":"<p>Questions about onboarding'),
      N'"mapEmbed":""',
      N'"mapEmbed":"https://www.google.com/maps?q=MG+Road+Pune&output=embed"'),
    UpdatedateTime = SYSUTCDATETIME()
WHERE Slug = 'contact' AND SectionsJson NOT LIKE N'%"image":"http%';

SELECT Slug, Title,
       CASE WHEN SectionsJson LIKE N'%"image":"http%' THEN 'yes' ELSE 'no' END AS HasImages,
       CASE WHEN SectionsJson LIKE N'%output=embed%'  THEN 'yes' ELSE 'no' END AS HasMap
FROM dbo.WebsitePageMasters
WHERE Slug IN ('home','services','about','contact')
ORDER BY MenuOrder;
GO
