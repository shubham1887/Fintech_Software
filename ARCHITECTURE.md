# Architecture & Design Logic

**Fino** — multi-tenant fintech distribution platform (recharge, DMT, AEPS, BBPS, payouts, wallets, settlements).

This document explains **why the system is shaped the way it is**. It is the reference a new
engineer reads before touching code, and the contract an existing one checks a change against.
It describes the design *rules*; the code is the implementation of those rules.

> Companion documents: `Brain.md` (legacy→new migration knowledge base), `Final_Audit_Report.md`
> (controller/endpoint audit). This document supersedes neither — it sits above both.

---

## 1. System context

Three codebases, one product.

| Codebase | Role | Stack |
|---|---|---|
| `Fino_API/` | The system of record. Owns money, identity, policy, provider integration. | .NET 9, EF Core 9, MS SQL |
| `Fino_Web/` | The operator console. Every role (admin → retailer) uses this one app. | Vite, React 19, TypeScript, Tailwind v4, TanStack Query |
| `UI/` | Legacy React 16 app. **Reference only** — read it to learn *data shapes*, never copy its design. | CRA + Material-UI |
| `Fino.PlanApi/`, `Fino.JaiJuiApi/` | Vendor SDKs isolated as separate libraries so vendor churn never reaches core code. | .NET class libraries |

**External actors:** recharge/DMT aggregators (many, hot-swappable), SMS gateways, KYC/eKYC
providers, payment gateways, and B2B API consumers who integrate *us* as their provider.

**The domain in one sentence:** a tree of distributors and retailers holds prepaid wallet float
with us; they sell services to end-customers; we buy those services wholesale from aggregators;
the margin is split down the tree by commission slabs.

Every architectural decision below follows from that sentence.

---

## 2. Architecture at a glance

```
┌──────────────────────────────────────────────────────────────────────┐
│  Fino_Web  — one console, role-shaped by the server-filtered menu     │
│  registry-driven screens · AES transport · TanStack Query cache       │
└───────────────────────────────┬──────────────────────────────────────┘
                                │  AES-encrypted multipart envelope
                                ▼
┌──────────────────────────────────────────────────────────────────────┐
│  Controllers  (289)  — thin. Decrypt → validate → delegate → serialise│
├──────────────────────────────────────────────────────────────────────┤
│  Authorization gates (run before every action, in this order)         │
│   1. JWT signature + expiry            2. Attribute role list         │
│   3. ApiAccessPolicy  (which endpoint) 4. KeyData cross-check         │
│   5. HierarchyScopePolicy (whose rows)                                │
├──────────────────────────────────────────────────────────────────────┤
│  Services  (276)  — ALL business rules, ALL authorization of data      │
│   money engine · commission engine · provider dispatch · reporting    │
├──────────────────────────────────────────────────────────────────────┤
│  Repositories  (233)  — persistence + cache. No business rules.        │
├──────────────────────────────────────────────────────────────────────┤
│  EF Core → MS SQL  (236 entities)                                     │
└──────────────────────────────────────────────────────────────────────┘
        ▲                                        │
        │ status-check daemon (hosted service)   ▼ provider seam (interfaces)
        └──────────────── aggregators / SMS / KYC / PG ────────────────
```

**Layer rule.** A controller never touches a repository. A repository never makes a decision.
Everything that can be got wrong lives in a service, because that is the only layer that is
both testable and consistently reachable.

---

## 3. The request lifecycle

This is the single most important flow in the system. Every authenticated call follows it.

```
1  Client builds { Request, KeyData } — both AES-256-CBC encrypted JSON, posted as multipart form
2  ExceptionHandlingMiddleware  → any unhandled throw becomes a clean 500 envelope, never a stack trace
3  RequestLoggingMiddleware
4  JwtAuthorizationAttribute
     a. Validate the bearer token's signature and expiry
     b. Attribute role list (e.g. [JwtAuthorization(UserTypeCode.Admin)]) — may only RESTRICT
     c. ApiAccessPolicy    — is this ROLE allowed to call this CONTROLLER/ACTION at all?
     d. KeyData cross-check — the identity in the body must equal the identity in the signed token
     e. HierarchyScopePolicy — does the body name a user OUTSIDE the caller's branch?
5  Controller: decrypt Request → deserialise DTO → `keyDataModel.IsValidModalState()`
6  Service: apply business rules, re-scope data to the caller, return (failMessage, data)
7  Controller: `ResponseProcess.GetSuccessResponse(json)` / `GetFailResponse(message)`
8  Client: decrypt `Data`, or parse it as plain JSON if it is not encrypted
```

### The response envelope

Every endpoint returns the same shape. This is non-negotiable — the client has exactly one
parser.

```jsonc
{ "StatusCode": 200, "Status": "SUCCESS", "Message": "…", "Data": "<json or AES blob>" }
```

`Status` is the business outcome; `StatusCode` is the transport outcome. A *business* failure
(insufficient balance, no records) is `400 / FAILED` with a human message — **not** an exception.
Exceptions are reserved for genuine faults.

**Design note — the two `Data` conventions.** Master/CRUD endpoints encrypt `Data`; report
endpoints return plain JSON. This is historical, not intentional. The client handles both
(`unwrap()` in `Fino_Web/src/lib/apiClient.ts` tries decrypt, falls back to parse). New endpoints
should follow whichever convention their neighbours use rather than introduce a third.

---

## 4. Security model

Authorization is **three concentric gates**, deliberately separated because they answer three
different questions. Getting one right does not make the others unnecessary.

### Gate 1 — Authentication: *who are you?*
JWT bearer token, validated on signature and expiry. `Fino_API/Helpers/JwtTokenService.cs`.

### Gate 2 — Endpoint authorization: *may your role call this at all?*
`Fino_API/StaticModels/ApiAccessPolicy.cs` — a central, **deny-by-default** map keyed by
controller (and by action where a controller mixes duties).

- An unlisted controller is Admin/Support only. Adding a controller does not accidentally
  expose it — you must opt in.
- Reference masters are readable by lower roles but writable only by admin: a `WriteVerb`
  regex (`Create|Update|Delete|…`) forces admin unless the controller is in `SelfServiceWrite`.
- Per-deployment overrides come from config (`ApiAccess:Overrides`), so a reseller can re-scope
  access without a recompile.

### Gate 3 — Data-scope authorization: *whose rows may you name?*
`Fino_API/StaticModels/HierarchyScopePolicy.cs` + `Services/UserScopeService.cs`.

Gate 2 says a distributor may call `Users/Select`. Gate 3 stops that distributor passing
*another branch's* `UserID` in the body. It works **generically**: the decrypted request body is
walked (JSON scan and DTO reflection) and any field with a known user-reference name
(`UserID`, `TargetUserID`, `ReceiverID`, `ParentID`, …) is checked against the caller's branch.

- `ScopeKind.Downline` — self + descendants (the default).
- `ScopeKind.Network` — also the upline chain, for fields that legitimately point upward
  (a fund *request* goes up to the parent).
- Mode is configurable: `HierarchyScope:Mode` = `Enforce` (default) / `Audit` / `Off`.

Because it is generic, **a new controller is covered the moment it uses the standard field
names.** There is nothing per-controller to remember.

### Anti-impersonation
The body carries `KeyData` (an encrypted identity block) because a lot of business logic reads
`keyDataModel.UserID`. Gate 4 cross-checks that block against the signed token, so a valid
token cannot be used to act as somebody else by editing the payload.

### Defence in depth inside services
The gates are the perimeter. **Services scope again anyway.** Every report method resolves its
own visible set:

> Admin/Support ⇒ everyone. Anyone else ⇒ self + full downline. A requested `UserID` outside
> that branch is **ignored, never honoured** — narrowing a filter must never widen reach.

This is the rule that makes an accidentally-broad endpoint harmless rather than a data breach.

### Transport encryption
AES-256-CBC + PKCS7, key/IV per deployment (`Encryption:Key` / `Encryption:IV`, mirrored in the
web app's `config.json` or `VITE_ENC_*`). This is a white-label obfuscation and integration
convention inherited from the legacy platform — **it is not a substitute for TLS**, and it is not
treated as one.

---

## 5. The money engine

The part of the system that must never be "roughly right".

### Invariants

1. **A wallet balance is only ever changed by one atomic statement.**
   `UserWalletMasterRepository.TryApplyDelta` is a single conditional SQL `UPDATE` with `OUTPUT`,
   so the affordability check and the write happen under the same database row-lock, and the
   caller gets the exact pre/post balance back for the ledger row without a second (racy) read.
   Zero rows affected = rejected. This is correct under concurrency *and across app instances*,
   with no queue and no application-level lock.

   ```sql
   UPDATE UserWalletMasters
   SET Balance = Balance + @delta, BalanceHash = NULL
   OUTPUT deleted.Balance AS OpeningBalance, inserted.Balance AS ClosingBalance
   WHERE UserID = @userId AND WalletID = @walletId
     AND (@allowOverdraft = 1 OR (Balance + @delta - (CASE WHEN @respectHold = 1 THEN HoldBalance ELSE 0 END)) >= @eps)
   ```

   `allowOverdraft` exists for the admin float account only. `respectHold` stops a normal debit
   eating into the frozen portion of a balance.

2. **Wallet balances are never cached.** A read-through cache on a balance is a correctness bug,
   not a performance win. (Caching *was* removed for exactly this reason.)

3. **Every balance movement writes a ledger row.** `TransactionDetails` is the append-only
   ledger; `TransactionMaster` is the business transaction. Reports reconcile against the ledger,
   not against recomputed sums.

4. **Money moves before the external call, and is refunded if that call fails.** Never the
   reverse — a crash after dispatch must not leave a free recharge.

### Transaction state machine

```
                 ┌────────────► SUCCESS ──────────► (terminal)
                 │
 PENDING ────────┼────────────► FAILED ─── refund + full commission reversal ──► (terminal)
   ▲             │
   │             └────────────► REFUND
   │
 status-check daemon re-enquires every StatusCheck:IntervalMinutes,
 admin can force a state via TransactionProcessing/UpdateStatusManually
```

Provider spellings differ (`FAILED`/`FAILURE`, `PENDING`/`PROCESS`, `REFUND`/`REFUNDED`), so
status comparison always goes through the synonym helpers rather than `==`.

### The recharge pipeline (canonical write path)

`Services/RechargeService.cs` — read it once and every other money flow makes sense.

```
 1. Validate request
 2. Resolve operator → service
 3. Resolve commission/surcharge from the user's assigned SLAB (SlabCommissionResolver)
 4. Balance check — the retailer must afford principal + every surcharge
 5. Create the transaction row as PENDING
 6. Debit the wallet (AccountProcessing writes the ledger row)
 7. Post retailer + distributor + super-distributor commissions UP-FRONT
 8. Dispatch to the aggregator  ← the only external, failable step
 9. Persist exactly what the provider replied (ref no, raw response, provider balances)
10. Compute admin commission = provider commission − (R + D + S), credit the admin wallet
11. Settle: SUCCESS / leave PENDING / FAILED ⇒ refund + reverse every commission tier
```

**Why commissions post at step 7, before the outcome is known:** every party is posted at
initiation and a failure reverses *all* of it in one place (`RefundTransaction`). The
alternative — posting on success — needs the same reversal logic anyway, plus a second code path
for the pending case. One posting path and one reversal path is fewer ways to be wrong.

### Commission is data, not code
Rates live in `SlabMaster` / `SlabDetail`, keyed by service, operator and amount band, with a
type (`Percentage` / `Flat`) per tier. A user gets a slab via `UserSlabMapping`. A **negative**
retailer commission is a *surcharge* — reports must split the two so a fee never displays as a
negative earning.

> **Key Setting vs Slab.** `SettingMaster` ("Key Setting") holds client-function ON/OFF toggles
> only. Charges and commission come from the Slab. Do not put money in Key Setting.

---

## 6. Provider integration — the seam

Aggregators churn: they change endpoints, go down, get replaced. The design assumption is that
**every provider will eventually be swapped**, so no provider name appears in core business code.

```
RechargeService ──► IRechargeProviderClient ──► DynamicRechargeProviderClient
                                                       │
                                                       ├─ reads ProviderMaster / ProviderConfiguration
                                                       ├─ endpoint templates + response rules from the ADMIN PANEL
                                                       └─ operator sequence decides who is tried, in what order
```

- **Config-driven dispatch.** Endpoint URLs, request templates and response-parsing rules are
  admin-panel data (`ProviderGatewayService`). Onboarding an aggregator is configuration, not a
  release.
- **Credentials live in `ProviderMaster`, not `appsettings.json`.** They are per-deployment and
  operator-editable.
- **Fail-safe default.** When nothing is configured the `Unconfigured*ProviderClient` is wired,
  which **fails and refunds**. A production build can never silently auto-succeed a transfer.
  A sandbox client exists for dev/UAT and is opt-in via config (`DMTProvider:Mode=Simulation`).
- **Vendor SDKs are separate assemblies** (`Fino.PlanApi`, `Fino.JaiJuiApi`) so their models
  never leak into the domain.
- **Asynchronous settlement is expected.** `RechargeStatusCheckBackgroundService` re-enquires
  PENDING transactions on a timer; provider callbacks land on `CallbackController`. Both converge
  on the same settlement functions as the synchronous path.

---

## 7. Configuration as data (the white-label rule)

The platform is resold. Anything a reseller might want different **must be data**, not code.

| Configurable | Where it lives | Consequence |
|---|---|---|
| Services offered | `ServiceMaster` (+ `ReportKind`) | Turning a service off hides its tiles *and* its report menu leaves |
| Wallets | `WalletMaster` | Wallet columns in reports are generated, never hard-coded |
| Commission | `SlabMaster` / `SlabDetail` / `UserSlabMapping` | Per-user pricing without a deploy |
| Feature toggles | `SettingMaster` ("Key Setting") | Curated allow-list screen, not a raw key/value editor |
| Providers | `ProviderMaster` + configuration + sequences | Aggregator swap = admin action |
| Menus | `MenuService` node tree, filtered by role **and** by active services | One source of truth for "what can this role see" |
| SMS templates | `MessageSetting` | 24 seeded defaults, fully editable |
| Subscription plans | `SubscriptionPlan` / `UserSubscription` | Whole feature gated by one master toggle |
| Branding / theme | `Fino_Web/public/config.json` + `UISetting` | A deployed build is re-branded **without rebuilding** |
| KYC document set | `VerificationDocumentConfig` | Document master is admin-managed |

**The rule:** if you are about to write a hard-coded id, name or rate, check whether it belongs
in one of these tables first. Service IDs in particular are **not fixed** across deployments —
this is why `ServiceKinds` classifies services by admin mapping + keyword rather than by id.

---

## 8. Reporting layer

Reports are where a fintech platform is actually judged, so they get an explicit design.

**Two services, split by shape of output:**

| | `ITransactionReportService` | `IReportAnalyticsService` |
|---|---|---|
| Returns | Raw transaction rows | Aggregates / roll-ups |
| Wrapper | `TxnReportWrapper { Count, list }` | `AnalyticsWrapper<T> { Count, list, HeaderSummary }` |
| Examples | Recharge, DMT, BBPS, Search, Past Success/Fail | Service-wise sale, Userwise sale, Daybook, Downline, Commission Structure |

**Contract every report obeys:**

1. Signature is `(request, currentUserID, currentUserTypeID)` — the caller's identity is an
   argument, never an ambient assumption.
2. The service scopes its own data (§4, *defence in depth*).
3. `HeaderSummary` is a `Dictionary<string, object>` of KPI name → value; the web report screen
   renders it as the stat strip above the table with **zero per-report frontend work**.
4. "No rows" is a **success with an empty list**, not a failure. A failure message is reserved
   for something the user can act on ("No commission slab is assigned to you yet").
5. Aggregates read the whole window once and group in memory — a single page of transactions
   cannot produce a correct roll-up. A single-user scope is pushed into the SQL query.

**Three bugs this contract was written to prevent** (all of which occurred):

- Passing `currentUserID` into `_txnRepo.GetAll(userId, …)` for an admin. `userId = 0` means
  *the whole network*; passing the admin's own id silently returns nothing.
- Treating the screen's `Status: "ALL"` as a literal status value, which matches no row.
- Matching roles by display name (`"RETAILER"` vs the DB's `"Retailer"`) instead of by
  `UserTypeCode`.

---

## 9. Frontend architecture

### Registry-driven screens
`Fino_Web/src/screens/registry.ts` maps a backend menu URL to a screen *config*. Most screens
need no component at all — adding one is a single entry.

| kind | Renders | Use for |
|---|---|---|
| `report` | date/status/search filters + table | simple report endpoints |
| `txn` | full report screen: filters, KPI strip, row actions, paging, CSV export | every real report |
| `list` | read-only master table | `<Master>/GetAll` |
| `crud` | list + add form + inline edit + delete | reference masters |
| `form` | a single submit form | actions (fund transfer, change status) |
| `custom` | a bespoke component | genuinely unusual screens only |

`TxnReportScreen` is the workhorse: a report is described declaratively as columns
(`{ key, label, type, roles }`) plus which standard filters it wants. `roles` on a column hides
commercially sensitive figures from lower tiers without a second config.

### Access is mirrored, not re-implemented
`Fino_Web/src/lib/access.ts` derives routing permission from **the role-filtered menu the server
returned** — one source of truth, no hand-maintained frontend role lists to drift. It is
explicitly a UX guard; the API enforces the same rules again.

### Data layer
TanStack Query for caching/invalidation; `apiClient.ts` owns the AES envelope, token refresh
(deduped across concurrent 401s) and error normalisation. A 404 becomes a soft "not available
yet" rather than a crash, so an unimplemented endpoint degrades to an empty state.

### Theming
Runtime `config.json` → build-time `VITE_*` → same-origin default. A deployed build is
re-pointed and re-branded by editing one JSON file next to `index.html`.

### UI conventions
English-only, minimal: flat stat strips, plain tables, few boxes. No emoji, no colourful tiles.

---

## 10. Cross-cutting concerns

| Concern | Approach | Caveat to know |
|---|---|---|
| Caching | `MemoryCacheProcessing<T>` — static in-memory list cache, 45-minute absolute expiry, per repository | **A direct SQL seed is invisible until the API restarts.** Never cache balances. |
| Concurrency | DB row-lock via `TryApplyDelta` for money; `ConcurrencyControl` item-level async locks elsewhere | Prefer the database lock — it survives multiple app instances |
| Idempotency | `APIUserRequestID` on `TransactionMaster`, unique per API user | A repeated request replays the original result instead of charging twice |
| Errors | `ExceptionHandlingMiddleware` → uniform 500 envelope; `Logger` writes the detail | Business failures are `FAILED` responses, not exceptions |
| Auditing | `AuditLog`, `TransactionManualUpdate`, provider request/response logs, callback logs | Manual settlements are always attributable |
| Schema | EF Core migrations **plus** hand-written scripts in `Scripts/` for production/data changes | Some features ship as a migration + a seed script; both must run |
| B2B API | `api/v1` (`ApiRechargeController`) — one common response contract for initial/status/callback, differing only by `event` | Callbacks are signed and delivered **async, off the request path** |

---

## 11. How to extend the system

### Add a report
1. Implement it in `ReportAnalyticsService` (aggregate) or `TransactionReportService` (raw rows).
   Scope the data inside the method. Populate `HeaderSummary`.
2. Add the method to the interface; wire a one-line controller action via the shared
   `Analytics<T>` / `Txn` helper.
3. Grant the role in `ApiAccessPolicy` if the controller is not already open.
4. Add a `TxnReportConfig` (columns + filters) and one `registry.ts` entry.
5. Add the node to `MenuService` with its `Roles` (and `Service`, if it should disappear when
   that service is off).

### Add an aggregator
Configure it in the admin panel — provider row, credentials, endpoints, response rules,
operator sequence. **No code change should be required.** If one is, the seam has a gap: fix
the seam rather than special-casing the vendor.

### Add a master screen
One `crud` entry in `registry.ts` against the existing `<Master>/GetAll|Create|Update|Delete`
endpoints. Writes stay admin-only automatically via the `WriteVerb` rule.

### Things that should make you stop
- A hard-coded ServiceID, WalletID or commission rate.
- A controller reaching into a repository.
- A report method that trusts `request.UserID` without scoping it.
- A balance read that is not immediately followed by an atomic conditional update.
- A new response envelope shape.

---

## 12. Known constraints and debt

Recorded honestly so nobody rediscovers them the hard way.

- **~104 endpoints remain "Feature under development." stubs** (from the FinovativeAPI →
  Fino_API port). All report endpoints the console calls are now implemented; the remainder are
  mostly unused legacy surface. Check before assuming an endpoint works.
- **Two `Data` conventions** (encrypted vs plain JSON) — see §3.
- **Static 45-minute repository caches** mean out-of-band SQL changes need an API restart.
- **DMT has no upstream provider commission** — the surcharge the retailer pays funds the whole
  chain, so its margin maths in `BankTransferService` reads differently from `RechargeService`
  even though both resolve from the same slab.
- **AES transport encryption is not TLS.** It is a legacy integration convention.
- `UserAccounting` reconciles **one wallet on one day**; it is not a range report.
- Provider opening/closing balances are not tracked for every provider, so some Provider Sale
  columns are structurally `0`.

---

## 13. The design principles, condensed

1. **Deny by default.** Unlisted means denied — at the endpoint gate and at the data gate.
2. **Scope data in the service, every time.** Perimeter checks are not a licence to trust input.
3. **Money changes in one atomic statement, and is always ledgered.**
4. **Fail safe.** Unconfigured providers refund; they never succeed.
5. **Configuration is data.** If a reseller might want it different, it belongs in a table.
6. **Thin controllers, decisive services, dumb repositories.**
7. **One envelope, one parser, one report contract.** Uniformity is what makes 289 controllers
   maintainable by a small team.
8. **Declarative screens.** A new screen is a config entry, not a component.
9. **Empty is not an error.** Say what the user can do about it.
10. **Assume every external dependency will be replaced.** Keep it behind a seam.
