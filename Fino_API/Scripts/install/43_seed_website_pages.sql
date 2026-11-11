/* ============================================================================
   Public website â€” default pages (Home / Services / About / Contact).

   GENERATED FILE â€” do not edit by hand.
   Regenerate with:  pwsh ./deploy/tools/Export-WebsitePages.ps1

   The public site is data, not code: without these rows a new installation
   serves an empty website. Each page is inserted only when its slug is absent,
   so this never overwrites content a client has already edited in
   Settings -> Website.
   ============================================================================ */

SET NOCOUNT ON;

-- home
IF NOT EXISTS (SELECT 1 FROM WebsitePageMasters WHERE Slug = N'home')
BEGIN
    INSERT INTO WebsitePageMasters
        (Slug, Title, MetaDescription, SectionsJson, MenuOrder, ShowInMenu, IsPublished, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        (N'home', N'Home', N'Fino — recharges, AEPS and bill payments for your business. Industry-best commissions, instant settlements.',
         N'[{"type":"hero","slides":[{"heading":"Grow your business with Fino","sub":"Recharges,  AEPS and bill payments — one platform, industry-best commissions and instant settlements for your counter.","image":"https://images.unsplash.com/photo-1563986768609-322da13575f3?auto=format&fit=crop&w=1200&q=80","ctaText":"Become a Partner","ctaLink":"/contact"},{"heading":"Every service your customers need","sub":"Mobile & DTH recharge, BBPS bill payments, AEPS banking and payouts — all from a single wallet and a single login.","image":"https://images.unsplash.com/photo-1601597111158-2fceff292cdc?auto=format&fit=crop&w=1200&q=80","ctaText":"Explore Services","ctaLink":"/services"},{"heading":"Settlements you can set your watch to","sub":"Real-time wallet, a transparent ledger for every rupee, and 24×7 instant payout to your bank account.","image":"https://images.unsplash.com/photo-1460925895917-afdab827c52f?auto=format&fit=crop&w=1200&q=80","ctaText":"Partner Login","ctaLink":"/login"}]},{"type":"features","heading":"Everything a retail counter needs","items":[{"icon":"Smartphone","title":"Mobile & DTH Recharge","text":"All major operators — Jio, Airtel, Vi, BSNL, Tata Play and more — with instant confirmation and automatic refund on failure."},{"icon":"Users","title":"AEPS Banking","text":"Aadhaar-enabled cash withdrawal, balance enquiry and mini statement — turn your shop into a neighbourhood banking point."},{"icon":"Zap","title":"BBPS Bill Payments","text":"Electricity, water, gas, broadband, FASTag and credit-card bills on the official Bharat BillPay rails with instant confirmation."},{"icon":"Wallet","title":"Instant Settlements","text":"Real-time wallet with a transparent ledger and instant payout to your bank — your money is never stuck."},{"icon":"Shield","title":"Bank-grade Security","text":"AES-encrypted APIs, JWT-secured sessions and role-based access control on every single request."}]},{"type":"textImage","heading":"Built for India''s distribution network","side":"right","image":"https://images.unsplash.com/photo-1579621970563-ebec7560ff3e?auto=format&fit=crop&w=1200&q=80","html":"<p>Fino is a complete B2B fintech platform for retailers, distributors and super distributors. Onboard your network, assign commission slabs per service and operator, and track every transaction in real time.</p><ul><li><strong>Retailers</strong> serve customers with recharge, AEPS and bill payments.</li><li><strong>Distributors</strong> manage retailer networks, transfer funds and monitor performance.</li><li><strong>Super Distributors</strong> run entire regions with full slab and wallet control.</li></ul><p>Every role gets its own dashboard, wallet, ledger and reports — with commissions credited the moment a transaction succeeds.</p>"},{"type":"stats","items":[{"value":"10,000+","label":"Retail partners"},{"value":"₹50 Cr+","label":"Monthly volume"},{"value":"28","label":"States covered"},{"value":"99.9%","label":"Platform uptime"}]},{"type":"features","heading":"Start in three simple steps","items":[{"icon":"Users","title":"1 · Register","text":"Share your details on the contact page or call us — our team completes your KYC and onboarding the same day."},{"icon":"Clock","title":"2 · Get activated","text":"Receive your login, add funds to your wallet and get your commission slab assigned by your distributor."},{"icon":"TrendingUp","title":"3 · Start earning","text":"Serve customers from day one and watch commissions credit to your wallet in real time, on every transaction."}]},{"type":"cta","heading":"Start your fintech business today","text":"Join thousands of partners who serve their neighbourhoods and earn on every transaction — no setup cost, full training and dedicated support.","buttonText":"Contact Us","buttonLink":"/contact"}]',
         1, 1, 1, GETDATE(), GETDATE(), 0);
END;
GO

-- services
IF NOT EXISTS (SELECT 1 FROM WebsitePageMasters WHERE Slug = N'services')
BEGIN
    INSERT INTO WebsitePageMasters
        (Slug, Title, MetaDescription, SectionsJson, MenuOrder, ShowInMenu, IsPublished, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        (N'services', N'Services', N'Mobile recharge, DTH, BBPS bill payments, AEPS banking and payouts — every service on one Fino login.',
         N'[{"type":"hero","slides":[{"heading":"Every service, one login","sub":"A complete suite of banking and payment services for your counter — activated on a single wallet with per-service commission slabs.","image":"https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?auto=format&fit=crop&w=1200&q=80","ctaText":"Become a Partner","ctaLink":"/contact"}]},{"type":"features","heading":"Recharge & Bill Payments","items":[{"icon":"Smartphone","title":"Prepaid & Postpaid Recharge","text":"All telecom operators with live plan browsing, instant confirmation and auto-refund on failed transactions."},{"icon":"Globe","title":"DTH & Broadband","text":"Tata Play, Dish TV, Airtel Digital, d2h and all major broadband providers — recharge in seconds."},{"icon":"Zap","title":"Utility Bills (BBPS)","text":"Electricity, water, piped gas, LPG booking, FASTag recharge and credit-card bill payments on official BBPS rails."}]},{"type":"features","heading":"Banking Services","items":[{"icon":"Users","title":"AEPS Cash Services","text":"Aadhaar-enabled withdrawal, deposit, balance enquiry and mini statement using any fingerprint device (RD service)."},{"icon":"Wallet","title":"Payouts & Settlement","text":"Move earnings to your bank instantly, anytime. Transparent wallet ledger with every credit and debit accounted."}]},{"type":"textImage","heading":"Transparent commissions, real-time credit","side":"left","image":"https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?auto=format&fit=crop&w=1200&q=80","html":"<p>Every service carries a commission slab that your distributor assigns to you. The moment a transaction succeeds, your share is credited to your wallet — no waiting for month-end.</p><ul><li>Operator-wise and slab-wise commission structure</li><li>Live commission reports for every role in the chain</li><li>TDS and GST handled with downloadable statements</li></ul>"},{"type":"cta","heading":"Want the full commission chart?","text":"Talk to our team and get the latest slab structure for your region.","buttonText":"Get in Touch","buttonLink":"/contact"}]',
         2, 1, 1, GETDATE(), GETDATE(), 0);
END;
GO

-- about
IF NOT EXISTS (SELECT 1 FROM WebsitePageMasters WHERE Slug = N'about')
BEGIN
    INSERT INTO WebsitePageMasters
        (Slug, Title, MetaDescription, SectionsJson, MenuOrder, ShowInMenu, IsPublished, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        (N'about', N'About Us', N'Fino brings every neighbourhood store into the digital-payments economy — recharge, AEPS and BBPS with real-time settlements.',
         N'[{"type":"hero","slides":[{"heading":"About Fino","sub":"A technology company bringing every neighbourhood store into the digital-payments economy.","image":"https://images.unsplash.com/photo-1521791136064-7986c2920216?auto=format&fit=crop&w=1200&q=80"}]},{"type":"textImage","heading":"Who we are","side":"right","image":"https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=1200&q=80","html":"<p>Fino is a technology company on a mission to bring every neighbourhood store into the digital-payments economy. We build the rails that let a local shop become a full-service banking and payments point.</p><p>Our platform powers retailers, distributors and super distributors across India with recharge, AEPS, BBPS and payout services — backed by real-time settlements and honest, transparent commissions.</p><p>We believe the corner shop is the most trusted financial touchpoint in India. Our job is to give it enterprise-grade tools.</p>"},{"type":"features","heading":"What we stand for","items":[{"icon":"HeartHandshake","title":"Partner First","text":"Your growth is our metric. Fair slabs, no hidden charges and support that actually picks up the phone."},{"icon":"Zap","title":"Speed Everywhere","text":"Instant transactions, instant commissions, instant settlements. Waiting is a bug, not a feature."},{"icon":"Shield","title":"Security by Default","text":"End-to-end encryption, audited access policies and anti-fraud checks on every request."},{"icon":"TrendingUp","title":"Built to Scale","text":"From one counter to a network of thousands — the same platform grows with you."}]},{"type":"stats","items":[{"value":"2019","label":"Founded"},{"value":"10K+","label":"Partners onboarded"},{"value":"24×7","label":"Service & settlements"},{"value":"4.8★","label":"Partner rating"}]},{"type":"cta","heading":"Grow with us","text":"Whether you run one counter or a distribution network, we would love to work with you.","buttonText":"Become a Partner","buttonLink":"/contact"}]',
         3, 1, 1, GETDATE(), GETDATE(), 0);
END;
GO

-- contact
IF NOT EXISTS (SELECT 1 FROM WebsitePageMasters WHERE Slug = N'contact')
BEGIN
    INSERT INTO WebsitePageMasters
        (Slug, Title, MetaDescription, SectionsJson, MenuOrder, ShowInMenu, IsPublished, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        (N'contact', N'Contact', N'Contact Fino — onboarding, commissions and support, 7 days a week.',
         N'[{"type":"hero","slides":[{"heading":"Get in touch","sub":"Our support desk is open seven days a week, 9 AM to 9 PM.","image":"https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?auto=format&fit=crop&w=1200&q=80"}]},{"type":"textImage","heading":"We would love to hear from you","side":"right","image":"https://images.unsplash.com/photo-1601597111158-2fceff292cdc?auto=format&fit=crop&w=1200&q=80","html":"<p>Questions about onboarding, commissions or a live transaction? Reach us on any channel below — our support desk is open <strong>7 days a week, 9 AM to 9 PM</strong>.</p><p>For transaction disputes, please keep your Transaction ID handy so we can resolve it faster.</p>"},{"type":"contact","address":"2nd Floor, Business Park, MG Road, Pune, Maharashtra 411001","phone":"+91 98765 43210","email":"support@fino.example","mapEmbed":"https://www.google.com/maps?q=MG+Road+Pune&output=embed"},{"type":"features","heading":"Dedicated support for every partner","items":[{"icon":"Phone","title":"Priority Helpline","text":"Distributors and super distributors get a dedicated relationship manager and priority line."},{"icon":"Clock","title":"Fast Resolution","text":"Most transaction issues are resolved within 24 hours with automatic refund on failures."},{"icon":"Users","title":"Onboarding Help","text":"Free training for you and your staff — in person or on a call, in your language."}]},{"type":"cta","heading":"Ready to start?","text":"Call us or drop an email — onboarding takes less than a day.","buttonText":"Partner Login","buttonLink":"/login"}]',
         4, 1, 1, GETDATE(), GETDATE(), 0);
END;
GO
