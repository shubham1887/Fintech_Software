# Enterprise roadmap

What to build next, in the order that a payments platform actually needs it. Every item below was
found in *this* codebase, with the file to start from — none of it is generic advice.

Ordering rule used throughout: **anything that can lose money or leak a customer's identity comes
before anything that adds a feature.** A fintech product is sold on the first list and demoed on
the second.

---

## P0 — before the next client goes live

### 1. Password hashing is SHA-256, unsalted

`Helpers/EncryptDecrypt.cs:99` hashes with a single pass of SHA-256 and no salt. Consequences, in
order of how quickly they bite:

* a commodity GPU tries billions of SHA-256 candidates per second, so the whole password file is
  crackable offline in hours;
* identical passwords produce identical hashes, so one leaked hash identifies every account that
  shares that password across *all* your clients' databases.

**Do:** add `BCrypt.Net-Next` (or ASP.NET Core's `PasswordHasher<T>`, which is PBKDF2). Store an
algorithm marker with the hash. On login: if the stored hash is the old format and the password
verifies, re-hash and save — the migration then completes itself as users log in, with no reset
email and no downtime. Touch points: `Services/AuthService.cs`, `Services/CredentialService.cs`,
the MPIN path (`MpinHash`), and the admin "reset password" flow.

*Effort: 2–3 days including a careful read of every call site. This is the single most important
backend item.*

### 2. Finish the IDOR sweep

`SECURITY_AND_CHANGES_REPORT.md` §2 lists the controllers already fixed and, explicitly, the ones
still taking a client-supplied `UserID` for a per-user read: `UserBankDetailsController`,
`eKYCMasterController`, the three OTP controllers, `UserBalanceHistoryController`,
`SentSMSController`. Each is "any signed-in retailer can read another merchant's data by changing
an id".

**Do:** apply the same `SeesAll` / owner-scope treatment already used in `BeneficiaryController`.
Then, to stop the class recurring, add a test that walks every `[JwtAuthorization]` controller
action taking a `UserID` and asserts it either scopes to the caller or is admin-gated. That test is
worth more than the individual fixes.

### 3. No brute-force lockout on login

`Services/CredentialService.cs:103` reads a `WrongPasswordCount` setting and records attempts, but
there is no IP-level throttle and no rate limiter registered anywhere in `Program.cs`. A mobile
number is ten digits; an attacker with a list of numbers and the hundred commonest passwords does
not need to be clever.

**Do:** `builder.Services.AddRateLimiter(...)` with a fixed window on `/api/Auth/*`,
`/api/Credential/*` and the OTP endpoints, keyed by IP **and** by mobile number (an attacker
rotating IPs against one account and one IP against many accounts are different attacks). Return
429 with a `Retry-After`. Add a progressive delay on the account itself.

*Effort: 1 day. .NET 9 has the rate limiter built in; no package needed.*

### 4. The wallet lock is in-process — the platform cannot be scaled

`Processing/ConcurrencyControl.cs:52` is a `ConcurrentDictionary` of locks inside one process, and
the caches are `IMemoryCache`. Both are correct for exactly one instance. Run two — behind a load
balancer, or during a rolling restart where old and new overlap for thirty seconds — and two
requests can debit the same wallet simultaneously. **Double spend.**

**Do:** two things, in this order.

1. Make it *safe* to run one instance: it already is — so document, in the installer output, that
   this build is single-instance, and make the deployment enforce it.
2. Make it *possible* to run several: move the wallet lock to a SQL application lock
   (`sp_getapplock`, already used by `DatabaseBootstrapper`) or Redis, and move `IMemoryCache` to
   `IDistributedCache`. Hangfire's recurring jobs need the same treatment — today two instances
   would each run the settlement sweep.

*Effort: 1 week for the lock and cache. Do it before the first client who needs more than one
server — not after.*

### 5. Secrets sit in plain files

`appsettings.Production.json` holds the SQL password, the JWT secret and the AES key. The installer
locks the file to SYSTEM and Administrators, which stops another user on the box reading it, but
not a backup that walks off.

**Do:** for Windows, encrypt the section with DPAPI at rest, or move the connection string to a
Windows-authenticated login so there is no password to store. For cloud, Key Vault / Secrets
Manager with the app's managed identity. Rotate the JWT secret on a schedule (accept the previous
key for its remaining token lifetime, so rotation does not log everyone out).

---

## P1 — what "enterprise" is actually judged on

### 6. There are no automated tests

Not one test project in the repository. For a system that moves money, that is the finding a
serious buyer's diligence will lead with.

**Start narrow, where a bug costs money:**

* the commission engine — slab boundaries, the split down the tree, GST and TDS rounding;
* the wallet ledger — debit/credit/hold/release/reversal, including the double-debit case from
  item 4;
* DMT routing — `RangeWiseSequence` overlap rejection, provider failover;
* the AES envelope and JWT gates — a request with no signature, a replayed nonce, an expired token.

Use xUnit with EF Core's SQLite in-memory provider for service-level tests, and `WebApplicationFactory`
for the authorization gates. Aim for "every money path has a test", not a coverage percentage.

### 7. Nothing is observable

Logging is `ILogger` to console/Event Log; `RequestLoggingMiddleware` writes request logs to the
database. When a client calls at 11pm saying "transfers are failing", there is no way to answer
"since when, how many, which provider" without writing a query.

**Do:**

* structured logging (Serilog → file + Seq/Elastic) with a correlation id per request, carried
  into provider calls so one transaction can be followed end to end;
* metrics (OpenTelemetry → Prometheus): transactions/minute by service and status, provider latency
  and error rate, wallet debit failures, queue depth in Hangfire;
* alerts that page a human: provider success rate below X for five minutes, settlement sweep not
  run, `/health` down, disk under 10%.

`/health` now exists and the installer already waits on it — that is the hook to build on.

### 8. No maker–checker on money-moving admin actions

An admin can credit a wallet, change a commission slab or reroute a provider alone. In a regulated
distribution business that is both a fraud risk and an audit finding.

**Do:** an approval queue for a defined set of actions (wallet credit above a threshold, slab
change, provider routing change, user-type promotion). Second admin approves; both identities and
the before/after values are recorded immutably. The `Modes`/`SubModes` fund-request flow already
has the shape — extend the pattern rather than inventing a second one.

### 9. Admin audit trail is partial

`LoginHistory`, `WrongPasswordLogs` and the API request/response logs exist, but there is no single
"who changed what" trail across the master screens.

**Do:** an `AuditEvent` table written from a single EF `SaveChanges` interceptor — actor, entity,
key, before, after, IP, correlation id. Append-only, with deletes denied at the database role. One
interceptor covers all 289 controllers; per-controller logging never stays complete.

### 10. Disaster recovery is a backup script and hope

`Backup-Fino.ps1` writes a nightly `.bak`. That is an RPO of up to 24 hours — one day of
transactions — and an untested RTO.

**Do:** agree the numbers with each client and then meet them. Realistically: full nightly +
transaction-log backups every 15 minutes (requires the FULL recovery model), copied off the
machine; a restore rehearsal every quarter with the elapsed time written down. For clients paying
for it, SQL Server Always On or log shipping to a standby.

---

## P2 — features that win deals

Roughly in the order clients in this market ask for them.

| # | Feature | Why it sells | Where it plugs in |
|---|---|---|---|
| 11 | **Reconciliation engine** | Every distributor's real pain: provider statement vs internal ledger, daily, by hand. Import the provider file, auto-match, show only the exceptions. | New service beside `StatusCheck`; the provider logs already hold what is needed. |
| 12 | **Self-service B2B portal** | API partners onboarding themselves — keys, IP allowlist, sandbox, usage graphs, downloadable docs — instead of emailing you. | `ApiUser*` controllers and the signature/nonce scheme already exist. |
| 13 | **Real-time dashboard** | A live ledger and provider-health wall is what makes a demo land. | SignalR over the existing transaction pipeline. |
| 14 | **Risk / velocity engine** | Rules on volume per sender, per beneficiary, per device, per hour; auto-hold suspicious transfers. Also what a compliance auditor asks for. | Sits in front of `DMTSenderFlowService`; the monthly cap logic is the seed of it. |
| 15 | **White-label mobile app pipeline** | `Fino_Android/whitelabel` exists — finish it into a one-command build so a client's branded APK is a config change, like `config.json` is for the panel. | `Fino_Android/whitelabel`, `tools/`. |
| 16 | **Commission simulator** | "What if I give retailers 0.4% on DMT?" answered before it is saved, across the whole tree. | Pure function over the commission engine — also the first thing item 6 should test. |
| 17 | **Scheduled/bulk operations** | Bulk wallet top-ups, scheduled recharges, CSV onboarding of retailers — how a distributor with 500 retailers actually works. | Hangfire is already in the stack. |
| 18 | **Tenant-aware reporting warehouse** | Heavy reports on a read replica or a nightly star schema, so a year-end export stops competing with live transactions. | Read-only replica + the existing report endpoints. |

---

## What this deployment kit already fixed

For completeness, so the list above is not read as the whole gap:

* Database installs itself from versioned, hash-tracked scripts — no manual SQL per client.
* Every installation gets its own JWT secret and AES key/IV; the shipped defaults are blanked at
  build time.
* CORS is configurable and no longer hard-coded to "allow any origin" (the template still defaults
  to `*` for upgrade compatibility — installs written by the installer do not).
* `/health` exists, and the installer and container healthcheck both gate on it.
* Upgrades take a backup and roll back automatically when the new build does not come up.
* Panel and API are one origin, one certificate, one service.
