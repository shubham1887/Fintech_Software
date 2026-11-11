# Fino — Items 26–33: changes, security fixes & handover

_Prepared 2026-09-14. Builds deployed to `Desktop\deploy ui` and `Desktop\deploy api`._
_New web bundle: `index-CWi6EkZ_.js` (check this hash in DevTools → Network to confirm the cache is fresh)._

---

## 1. Feature / defect items

| # | Item | Status | What was done |
|---|------|--------|---------------|
| 26 | Userwise DMT Sequence — "banaya nahi hai" | **Built** | Was a read-only stub. Now a full CRUD screen: pick user → service → operator (cascades) → provider. Backend `Insert` upserts (clears the same user+operator rule first). Response now carries names, not raw ids. |
| 27 | DMT Range Sequence — pending | **Built** | Full CRUD: service/operator/from-amount/to-amount → provider. Backend now **rejects overlapping amount ranges** for the same operator (ambiguous routing was possible before). Names enriched in the response. |
| 28 | CircleAPI — "banana baki hai" | **Built** | Full CRUD (lookup URL + active toggle). |
| 29 | UI Settings — horizontal nav & responsiveness broken | **Fixed** | Rewrote the top navigation. See §3. |
| 30 | Plan Setting — remove | **Removed** | Gone from `registry.ts` and the backend menu. |
| 31 | News & Notifications — "banana hai" | **Built** | Full CRUD (publish / draft / edit / remove). Server stamps the author from the session; drafts are hidden from non-admins; only admin/support can publish. |
| 32 | Tickets — "dynamic wala" | **Built** | New working queue: raise, track, answer, close, with status tabs, filters, and a figures bar. Plus a Ticket Categories admin CRUD. New `TicketMaster/Resolve` endpoint sets status + resolution note together and stamps who closed it. |
| 33 | Security — "bhoot sare loophole" incl. Android | **In progress** | Real fixes landed this session; full audit is multi-pass. See §2. |

All five new screens reuse the existing config-driven CRUD/report system, so they match the rest of the panel.

---

## 2. Security (item 33)

### The main vulnerability class found: IDOR (broken object-level authorization)

A family of endpoints was marked `[JwtAuthorization]` (any signed-in user) and then trusted a
**client-supplied `UserID`** — or no owner check at all — when reading or writing per-user records.
Effect: **any signed-in retailer could read, edit or delete another merchant's data** — including
saved bank accounts, UPI payees and customer PII — by changing an id in the request.

### Fixed this session

| Controller | Data exposed | Fix |
|------------|--------------|-----|
| `BeneficiaryController` | DMT bank beneficiaries (name, account no, IFSC) | `GetAll` scoped to owner; `GetById`/`Update`/`Delete` ownership-checked; `GetByUserID` pinned to caller; `Create` stamps owner from session. Admin/support still see all. |
| `PayoutBeneficiaryController` | Payout bank accounts money is sent to | same treatment |
| `UpiBeneficiaryController` | Saved UPI payees | same treatment |
| `UpiSenderController` | UPI senders (customers) | same treatment |
| `SenderController` | DMT senders (customer name, mobile, address) | same treatment |
| `NewsController` | — | Publishing/editing/deleting gated to admin+support; author stamped from session; drafts hidden from non-admins |
| `TicketMasterController` | Support tickets (subject/body) | Non-admins scoped to their own tickets; ownership enforced on read/update/delete; status+resolution moved to an admin/support-only `Resolve` endpoint |
| `TicketCategoryController` | — | Category create/update/delete gated to admin |
| `CircleAPIMaster`, `RangeWiseSequence`, `UserwiseDMTSequence` | Provider routing (where real money goes) | Elevated to **admin-only** |

_(Two of this same class — `ComplaintMaster/GetAll` and `LoginHistory/GetAll` — were fixed earlier in the project.)_

### Still to fix — same pattern, next pass (prioritised)

These `[JwtAuthorization]` controllers still take a client `UserID` for a per-user read without
scoping to the caller. They should get the identical `SeesAll`/owner-scope treatment. Ordered by
sensitivity:

1. **`UserBankDetailsController`** — the retailer's own bank accounts (settlement). High.
2. **`eKYCMasterController`** — KYC records (identity documents). High.
3. **`SenderOTPController`, `BeneficiaryOTPController`, `PayoutBeneficiaryOTPController`** — OTP records tied to a sender/beneficiary. High (OTP replay / enumeration).
4. **`UserBalanceHistoryController`** — another user's balance movements. Medium–high.
5. **`SentSMSController`** — messages sent to a user (may contain OTPs/PII). Medium.

Beyond these, ~90 controllers reference `request.UserID`; **most are already `[JwtAuthorization(Admin)]`
master screens where that is legitimate.** The disciplined next step is to enumerate every
non-admin-gated controller and confirm each per-user read is caller-scoped — this is a genuine
multi-session audit, not a one-shot claim.

### Password hashing — flagged, not yet changed

Passwords are stored as **plain SHA-256, unsalted** (`EncryptDecrypt.HashPassword`). This is not
adequate for a payments platform — it is fast to brute-force and identical passwords produce
identical hashes. Recommend migrating to a slow, salted KDF (BCrypt/PBKDF2/Argon2) with a
transparent rehash-on-login. This was **not** changed here because it touches every login and needs
a migration path; it is the single most important backend security item after the IDORs.

### Android — reviewed, and it is in good shape

Contrary to the concern, the Android app's security is solid:

- Cleartext traffic disabled in production (`network_security_config`); localhost/`10.0.2.2` exception is dev-only.
- SSL errors are **cancelled**, never bypassed (`onReceivedSslError` → `handler.cancel()`).
- `mixedContentMode = NEVER_ALLOW`, `allowFileAccess = false`, `allowContentAccess = false`.
- The native JS bridge (`secureGet`/`secureSet`/file save/biometric) is **effectively origin-locked**: `UrlRouter` sends every non-portal host to an external Custom Tab, and `isInternalHost` uses a strict dot-boundary allowlist (`h == a || h.endsWith(".$a")`) — no loose substring match, so look-alike hosts don't match.
- `allowBackup=false`; secrets in `EncryptedSharedPreferences`.

One inherent note (no change needed now): the bridge is injected on every in-portal page, so any
future XSS **on the portal origin itself** would reach the bridge. That is mitigated by the portal
being first-party; keep the web app free of injection sinks.

---

## 3. UI Settings — horizontal navigation (item 29)

The horizontal top nav was unusable because its dropdown menus were clipped inside the header's
scroll container, and the bar overflowed on smaller screens. Rewrites:

- Dropdown panels now render in a **portal at measured coordinates**, so nothing clips them, and they flip left near the right edge.
- The bar **measures itself and moves overflow into a "More" menu**, re-measuring on resize — verified at 1024 / 1280 / 1440 / 1920 px with no horizontal page scroll.
- Header controls (bell, theme, user, chevrons) now switch colour with the coloured app bar — they were near-invisible dark-on-dark before.

Verified in a browser harness (overflow math, portal placement, no `document` x-scroll).

---

## 4. Verification status

- **Backend:** builds clean (0 errors), Release-published, deployed.
- **Web:** type-checks clean, production build succeeds, deployed.
- **Tickets screen:** mounted with real-shaped data in a harness — table, figures bar, filters and all six status tabs render.
- **TopNav:** measured in a live harness across four widths + portal/clip check.
- **Not done:** full click-through E2E of the new CRUD screens against the live API (blocked — needs an admin login I can't obtain here). The screens use the already-proven `CrudScreen` path, and every endpoint they call was confirmed present in the backend.

---

## 5. Deploy note

No new database migration is needed for items 26–33 (all five tables already existed). The
`wallet_hold_history.sql` script from the earlier batch still needs to be run on the DB if it
hasn't been yet.
