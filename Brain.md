# 🧠 Brain.md — Fintech Portal Knowledge Base

> Single source of truth for the migration from the **legacy `UI/`** app to the new
> **`Fino_Web`** front-end (backed by **`Fino_API`**). Covers old structure, the API
> contract, the new screen framework, every admin page, and all components/methods.
>
> Last updated: 2026-06-24

---

## 0. The three codebases

| Folder | What it is | Stack |
|--------|-----------|-------|
| `UI/` | **Legacy** front-end (reference only — DO NOT copy its design) | CRA + React 16, Material-UI, Formik + Yup, custom ReactTable, axios |
| `Fino_API/` | **New** backend (.NET 9, EF Core 9) — what we integrate with | C#, JWT, AES, MS SQL (DB: `FintechApp`) |
| `Fino_Web/` | **New** front-end we are building (unique design, white-label) | Vite 8, React 19, TypeScript, Tailwind v4, TanStack Query 5, RHF + zod, axios, crypto-js, recharts |

**Golden rule:** Read the legacy `UI/` page to learn the *data* (columns, filters, form fields,
endpoint), then rebuild it in `Fino_Web` with **our own design** — never copy the MUI look
(legal reason). Endpoints/field-names come from `Fino_API`, NOT from the old UI.

---

## 1. Legacy `UI/` — structure & how it works

```
UI/
├─ src/
│  ├─ App.js                         # Router: mounts OuterLayoutNavigator + InnerLayoutNavigator
│  ├─ Config.js                      # env config (API URL, client name, flags) from public/config
│  ├─ ConstComponents.js             # LOGOUT(), global consts
│  ├─ Navigation/
│  │   ├─ InnerLayoutNavigator.js    # ~82 KB — THE route table: every /Path → component
│  │   └─ OuterLayoutNavigator.js    # login / public routes
│  ├─ Pages/
│  │   ├─ Layouts/                   # InnerLayout (sidebar+topbar), OuterLayout
│  │   ├─ OuterPages/                # Login, signup
│  │   └─ InnerPages/<Feature>/*.js  # ~100 feature folders (the actual pages)
│  ├─ Components/
│  │   ├─ Api/*API.js                # endpoint wrappers: PostAPI(request,'Controller/Action')
│  │   ├─ Inputes/                   # MaterialTextField, FormikMaterialSelect, AsyncAutoComplete,
│  │   │                             #   MaterialDatePickerField, CustomButton
│  │   ├─ ReactTable/ReactTable.js   # the data-grid (columns[], serverSide pagination, MoreMenu)
│  │   ├─ ReportHelper.js            # dropdown option lists (StatusItems/ComplaintItems/ExportTypeItems…)
│  │   ├─ Modal/, Alert/, Loader/, SummaryCards/
│  └─ Assets/, Theme/, styles/
```

### Legacy page pattern (every report/master looks like this)
1. `Formik` form holds the **filters** (FromDate, ToDate, Status, Service→Operator, User autocomplete, Criteria…).
2. `createColumns()` builds a `columns[]` array (`{label, name, numeric, customStatus, Image, hide, MoreMenu, options}`).
3. `<ReactTable data={rows} columns={columns} serverSide getCallback=… />` renders the grid with server-side paging.
4. Row actions live in a **MoreMenu** (`options:[{Title:'Print', Callback}]`) + modals (Print receipt, Complaint, Log…).
5. `callSelectData()` builds a request object → `await API_FN(JSON.stringify(req))` → `JSON.parse(response.data)` → `data.list / data.Count / data.HeaderSummary`.

### Legacy API helper
`Components/Helper/Helpers.js` → `PostAPI(request, 'Controller/Action')`:
- Encrypts `request` + `KeyData`, posts multipart, decrypts response, returns `{isAuthorized,status,data}`.
- All endpoint files are `Components/Api/*API.js`. Examples:
  - `TransactionReportAPI.js` → `SelectForRechargeFilter` = `TransactionReport/SelectForRechargeFilter`
  - `SlabApi.js` → `Select`/`Add`/`Update`/`Delete` (OLD names — new backend differs! see §3)

---

## 2. API integration contract (AES + envelope) — used by `Fino_Web/src/lib/`

- **Encryption:** AES-256-CBC + PKCS7. Key `Talk2m@FintechApp@SecretKey32Chr`, IV `FintechApp@16Byt` (UTF-8 → Base64). Matches `appsettings Encryption:Key/IV`. (`src/lib/crypto.ts`)
- **Request:** `multipart/form-data` with two fields:
  - `Request` = `enc(JSON.stringify(payload))`
  - `KeyData` = `enc(JSON.stringify({ UserID, UserTypeID, SessionToken: AccessToken, DeviceType:'Web', IPAddress:'' }))`
  - Header `Authorization: Bearer <AccessToken>`. (Login/refresh send `KeyData=enc('{}')`, no bearer.)
- **Response envelope (camelCase):** `{ statusCode, status, message, data }`. `status === 'SUCCESS'` = ok.
- **`data`** is itself **AES-encrypted** on most endpoints → decrypt → `JSON.parse` → **PascalCase** object/array. A few endpoints return plain JSON → client try-decrypts then falls back to raw. (`src/lib/apiClient.ts` `apiPost`)
- **401** → `Auth/refresh` ({AccessToken,RefreshToken}) → retry once → else logout.
- **Backend-stub signal:** HTTP 200, `status:"FAILED"`, `message:"Feature under development."` → our fetchers map to empty (designed empty-state, no crash).
- **Helpers:** `src/api/list.api.ts fetchList`, `src/api/reports.api.ts fetchReport`, `TxnReportScreen fetchTxn` — all return `[]` on a business FAIL.

**Dev:** backend `http` profile = `http://localhost:5266`; `Fino_Web/.env` → `VITE_API_URL=http://localhost:5266/api/` (direct, CORS open in Dev). Seed admin: mobile `9999999999` / `Admin@123` (UserTypeID=1, UserID=2).

---

## 3. New backend `Fino_API` — controller patterns (AUTHORITATIVE)

### Master controllers — UNIFORM pattern (verified)
Every master controller (`api/[controller]`) exposes:
```
POST GetAll        (KeyData only)        → list
POST GetById       {<Entity>MasterID}    → one
POST GetDropdown   (KeyData only)        → id/name pairs
POST Create        CreateXRequest        → insert (full typed object)
POST Update        UpdateXRequest        → see update mode below
POST Delete        {<idField>}           → delete
```
⚠️ **The legacy UI used `Select`/`Insert` — the new backend uses `GetAll`/`Create`. Always use the new names.**

### Update mode VARIES per master (both supported by `useCrud`)
| Mode | Payload | Masters |
|------|---------|---------|
| **field-level** (default) | `{idField, FieldName, OldValue, NewValue}` (one call per changed field) | Service, Operator, ProviderMaster, BBPSConfiguration |
| **object** (`mode:'object'`) | `{idField, ...allFields}` (one call) | Slabs, DenominationMaster, ProviderPurchase, UserSlabMapping, SlabDetail |

### idFields & create-field names (verified from DTOs)
| Master | Controller | idField | Key create fields |
|--------|-----------|---------|-------------------|
| Service | `Service` | `ServiceID` | Name, ServiceCategory, HSNCode, Charge, MinBalance, IsActive, IsDisplay, IsDefault, IsOutside, IsBBPS, IsSubscriptionAllow |
| Operator | `Operator` | `OperatorID` | ServiceID, OperatorName, Opcode, IsBBPS, Min/MaxRechargeAmount, BusinessModel, HSNCode, AccountStartWith, MinLength, MaxLength, UISequence, … |
| Slabs | `Slabs` | `SlabMasterID` | SlabName, IsActive |
| Denomination | `DenominationMaster` | `DenominationMasterID` | Description, DenominationRange, IsStop |
| Provider | `ProviderMaster` | `ProviderID` | ProviderName, ProviderUrl, ProviderUniqueID, IPAddress, LoginID, Token, SupportNumber, LowBalanceAmount |
| BBPS Config | `BBPSConfiguration` | `BBPSConfigurationID` | OperatorID, Label, Type, PlaceHolder, IsRequired, ErrorMeeage, Sequence, Length |
| Provider Purchase | `ProviderPurchase` | `ProviderPurchaseID` | ProviderID, WalletID, Amount, PurchaseDate, Remark |
| User-Slab map | `UserSlabMapping` | `UserSlabMappingID` | UserID, SlabMasterID |
| Slab commission | `SlabDetail` | `SlabDetailID` | SlabMasterID, ServiceID, OperatorID, From/ToAmount, Retailer/Distributor/SuperDistributor/Client Comm (+Type) |

### Report endpoints (legacy-style names, kept; mostly STUBS)
`TransactionReport/*` (`SelectForRechargeFilter`, `SelectForDMTFilter`, `SelectForBBPS`, `Payout`, `MicroATM`, `AEPS`, `ServicewiseSale`, `OperatorSale`, `UserwiseSale`, `SelectTopPerformer`, `SelectPastSuccessFail`, `SelectRangewiseRecharge`, `Income`, `ProviderSale`, `SelectPendingTransactionWithFilter`, `SelectDMTPendingTransactionWithFilter` …), `TransactionDetails/SelectForFilter` & `SelectAdminAccounting`, `Ledger/Select` & `AdminLedger`, `DayBookReport/Select` & `ProviderDaybook`, `TransactionWiseReport/SelectOperatorWisesUserWiseReport` & `SelectUserWiseOperatorWiseSellReport`, `Tax/SelectTDS`, `Fund/SelectWalletTransferReport`, `Users/SelectDownlineReport`/`SelectDatewise`/`SelectUserOnbordingList`.

### Action endpoints (verified wired)
`Fund/Transfer` (FundTransferRequest: ReceiverID, WalletID, Amount, PayMode, BankName, Remark), `Fund/Reverse` (SenderID, WalletID, Amount, Remark), `Fund/AcceptReject` (FundUpdateRequest: FundTransactionID, **Status:'Accept'|'Reject'**, Amount, SenderRemark), `Fund/SelectRequestByFilter`, `TransactionStatusChange/Create` (UserID, TransactionMasterID, NewStatus, Amount, Remark), `ServiceAndUserwiseSubscription/Create` (UserID, ServiceID, Charge, DaysCount…), `OnboardingUser/Create` (Name, MobileNumber, EmailID, PanNo, AadhaarNo, DOB, State, PinCode, BankName, AccountNumber, IFSC, SlabID), `UserBankDetails/Create` (UserID, BankName, HolderName, AccountNumber, IFSC, Branch, UPINumber), `PushNotifications/SendPushNotification` (Title, Description, UserTypeID), `Users/Activate` ({TargetUserID, IsActive}), `Users/Deactivate`, `Users/BulkUpdate`, `Users/TransferUsers`, `Users/SendPassword`, `Reconciliation/Upload`.

---

## 4. New front-end `Fino_Web` — structure

```
Fino_Web/src/
├─ lib/            crypto.ts · apiClient.ts · storage.ts · format.ts · row.ts(getField) · csv.ts · cn.ts · useClientSearch.ts · roles.ts
├─ api/            auth.api.ts · menu.api.ts · list.api.ts · reports.api.ts
├─ theme/          BrandProvider (white-label runtime CSS vars), themes.ts (presets)
├─ components/
│   ├─ ui/         Button, Input, Select, Badge/StatusBadge, Card, Modal, Skeleton, EmptyState,
│   │              PageHeader, StatTile, TableToolbar, DynamicTable, DynamicForm, Autocomplete, Toast
│   └─ layout/     AppShell, Sidebar, Topbar, iconMap (pickIcon), resolveRoute (resolveHref)
├─ screens/        registry.ts · ScreenHost.tsx · crudTypes.ts · useCrud.ts
│   ├─ ReportScreen · ListScreen · CrudScreen · FormScreen      (generic kinds)
│   ├─ TxnReportScreen + txnReportTypes.ts + txnConfigs.ts      (transaction reports)
│   └─ UsersScreen · OperatorsScreen · SlabCommissionScreen · FundRequestsScreen   (bespoke 'custom')
├─ pages/          LoginPage · DashboardPage · Appearance · Profile · PlaceholderPage
└─ App.tsx         routes: /login, /dashboard, /s/* (ScreenHost), /coming-soon, appearance, profile
```

### Screen framework (the heart) — `registry.ts` → `ScreenHost`
Sidebar fetches the **live role menu** (`SidebarMenu/Select`). Each menu URL routes to `/s/<url>`.
`ScreenHost` looks up `findScreen(url)` in the registry and dispatches by **kind**:

| kind | renderer | use | config |
|------|----------|-----|--------|
| `report` | `ReportScreen` | simple date/status/search report | `endpoint` |
| `list` | `ListScreen` | read-only `<X>/GetAll` table + auto stat-strip + search/export | `endpoint`,`payload` |
| `crud` | `CrudScreen` | list + Add + Edit + Delete + **toggle** + **row-links** | `crud: CrudConfig` |
| `form` | `FormScreen` | single action form → one endpoint | `form: FormConfig` |
| `txn` | `TxnReportScreen` | rich transaction report (filters, summary, explicit role-aware columns, pagination, View modal, row actions) | `txn: TxnReportConfig` |
| `custom` | component from `CUSTOM_SCREENS` | bespoke pages | `component` key |

**Add a page = one registry entry.** Bespoke = component + `kind:'custom'` + `CUSTOM_SCREENS` key.
`CUSTOM_SCREENS = { users, operators, slabCommission, fundRequests }`.

### Shared components (methods)
- **DynamicTable** — derives columns from row keys; money/date/status/bool formatting; hides sensitive cols (`password|hash|token|id|guid`); optional `actions(row)` column; skeleton + empty state.
- **DynamicForm** — config-driven fields (`text|number|checkbox|select|date|password|hidden`); `AsyncSelectField` (FK dropdown via `optionsEndpoint`); `initial` prefill; coerces numbers/booleans; optional `onCancel`.
- **Autocomplete** — client-side filtered combobox (user/entity pickers).
- **StatTile / TableToolbar / Modal / Badge / PageHeader(icon)** — design primitives.
- **useCrud(crud)** — `{rows,isFetching, adding,setAdding, editing,setEditing, createMut, updateMut(field|object mode), deleteMut, toggleMut, onDelete}`.
- **getField(row,...names)** — case-insensitive multi-alias field read (PascalCase API rows).
- **pickIcon(title,icon?)** — per-page lucide icon identity.

### White-label theming
`BrandProvider` loads `public/config.json` (appName, logo, theme preset, login copy) → injects ramp as runtime CSS vars `--color-brand-*`. Tailwind utilities reference those vars → app re-themes live. Resell one codebase to many clients (different brand, same code).

---

## 5. Admin portal — full menu (118 URLs, 11 groups) + what each is

> Source: live `SidebarMenu/Select` for admin (UserID 2). Order below = menu order.

**Dashboard** `/AdminDashboard` → bespoke `DashboardPage` (gradient hero, KPI tiles, recharts bar+donut, breakdown cards; `Dashboard/Select`).
**Search** `/Search` → txn-ish report (`TransactionReport/Search`).

**Pending** — `txn`: `/Pending` PendingRecharge (`SelectPendingTransactionWithFilter`), `/PendingDmt` (`SelectDMTPendingTransactionWithFilter`), `/PendingRecon`.

**Fund** — `/VirtualBalance` (list), **`/Fund` Fund Requests = bespoke accept/reject** (`Fund/SelectRequestByFilter` + `Fund/AcceptReject`), `/Fund/Transfer` (form), `/FundReverse` (form), `/FundReverseRequestReport` (report `Fund/ReverseReport`).

**Admin** — `/Complaints` (list), `/Invoice` (list), `/SmsReport` (list), `/EcollectionReport` (list), `/ManualAgentOnboarding` (form `OnboardingUser/Create`), `/ViewAgentOnboarding` (list), `/ProviderLog` `/PaymentGatewayLog` `/ApiUserLog` `/BalanceCapping` (lists), `/AdminAccounting` (txn `TransactionDetails/SelectAdminAccounting`), `/AdminLedger` (txn `Ledger/AdminLedger`), `/ProviderDayBook` (txn `DayBookReport/ProviderDaybook`), `/ProviderSale` (txn), `/PurchaseReport` (txn), `/ChangeStatus` (form `TransactionStatusChange/Create`), `/ManualSuccessFailReport` (report), `/UserOnboardedReport` (txn `Users/SelectUserOnbordingList`), `/TicketReport` (list), `/SenderBeneficiaryReport` (report).

**Report** (all `txn` unless noted) — `/Recharge` (`SelectForRechargeFilter`, 27 cols), `/DMTReport` (`SelectForDMTFilter`), `/AEPSReport` (`AEPS`), `/MicroATMReport` (`MicroATM`), `/PayoutReport` (`Payout`), `/BBPSReport` (`SelectForBBPS`), `/SettlementReport` & `/AccountStatement` (`TransactionDetails/SelectForFilter`), `/Fund/Report`, `/MoveFundsReport` (`Fund/SelectWalletTransferReport`), `/CommissionStructure`, `/SaleReport` (`ServicewiseSale`), `/OperatorSale`, `/UserwiseSale`, `/UserwiseAccounting` (`UserAccounting/UserwiseAccounting`), `/UserLedger` (`Ledger/Select`), `/Daybook/Report` (`DayBookReport/Select`), `/Income`, `/TDS` (`Tax/SelectTDS`), `/PastSuccessFailReport`, `/LoginHistory` (list), `/UserWiseDMTRangeReport`, `/RangeWiseRecharge`, `/ServiceChargeDeductionReport`, `/OperatorWiseUserWiseReport` (`TransactionWiseReport/…`), `/UserWiseServiceSellReport`, `/PayoutBeneficiaryReport`, `/ProviderWisePerformanceGraph`.

**Users** — **`/Users` bespoke** (avatar roster, stat strip, kebab: View / Activate-Deactivate via `Users/Activate`), `/AgentDetailReport` (list), `/CreatePlanIndex` (list), `/AssignPlan` (form `ServiceAndUserwiseSubscription/Create`), `/Users/Inactive` (list IsActive:false), `/DownlineReport` (txn `Users/SelectDownlineReport`), `/Performance` (txn `SelectTopPerformer`), `/Users/UserAlias` (list), `/Users/DatewiseReport` (txn `Users/SelectDatewise`), `/Users/DeletedUsers` (list ShowDeleted), `/Users/UserTransfer` (form `Users/TransferUsers`), `/Users/UserWiseBulkAction` (form `Users/BulkUpdate`), `/IDFYProfile` (list), `/MISInterestedAgents` (list).

**Slab** — **`/Slabs` CRUD** (`Slabs/GetAll|Create|Update(object)|Delete`, toggle, **row-link → Commission**), **`/Slabs/Commission` bespoke** (`SlabCommissionScreen` = `SlabDetail` CRUD per slab), `/Slabs/Assign` CRUD (`UserSlabMapping`).

**Operators** — `/Operators/Add` (form `Operator/Create`), **`/Operators/Index` bespoke card grid** (full CRUD + toggle), `/BBPSConfiguration/Index` CRUD, `/OperatorUserwise` (list).

**API** — `/ProvidersAdd` CRUD (`ProviderMaster`), `/UserwiseSequenceIndex` `/UserWiseDMTSequenceAdd` `/ProvidersSequence` `/RangeWiseSequence` (lists), `/DenominationDetails` CRUD, `/ResponseAdd` (list), `/CircleAPIAdd` (list), `/ReconciliationSettings` (list), `/ReconciliationUpload` (form), `/AddPurchase` CRUD (`ProviderPurchase`).

**Setting** — `/KeySetting` `/UISetting` `/PlanSetting` `/SMSAdd` `/MessageSetting` `/News/Index` `/Slider` `/ServiceWiseSlider` `/SupportAccess` `/DMTBankSetting` `/VirtualAccountSetting` `/MenuAdd` `/BulkMessageIndex` (lists), `/AddBank` (form `UserBankDetails/Create`), `/PushNotifications` (form).

**Service** — `/DMTSettings` `/AEPSSetting` `/MicroATMSetting` `/POSSetting` `/PayoutSetting` `/CashDepositSetting` `/EcollectionSetting` `/eKYCSetting` `/CMSSetting` `/InvoiceSettings` `/VerificationSettings` (lists), **`/Services/Index` CRUD**, `/SeviceswiseWallet` `/ServiceSubscriptionCharge` (lists).

---

## 6. Per-page FUNCTIONS / components (what does something)

- **Active/Inactive toggle** — CRUD masters + Operators + Users (reuses `Update`; mode-aware). Users uses `Users/Activate`.
- **View-details modal** — report rows + Users rows (shows all fields).
- **CRUD row-links** (`crud.links`) — Slab → Commission.
- **Slab Commission setup** (`SlabCommissionScreen`) — per-operator Retailer/Distributor/SuperDistributor/Client commission (+Flat/% type). `SlabDetail` CRUD scoped by `SlabMasterID` (hidden field injection).
- **Fund accept/reject** (`FundRequestsScreen`) — Accept/Reject buttons on PROCESS rows.
- **Report row kebab** — "View details" (real) + Print/Complaint/Log/Callback/Success-Fail labels (UI present; those endpoints are backend stubs).
- **Forms** — Fund Transfer/Reverse, Change Status, Assign Plan, Manual Onboarding, Add Bank, Push Alert, User Transfer, Bulk Action, Recon Upload — all field-matched to backend DTOs.

### ✅ Live-verified end-to-end (backend up :5266)
Slab CRUD · Service CRUD (field-level update) · Users/Activate · OnboardingUser/Create · SlabDetail/Create (commission) · all returned SUCCESS with real DB writes.

---

## 7. Status & what's pending

**Done:** full menu coverage (118/118, no dead-ends) · 33 transaction reports (real columns) · 9 master CRUDs (real fields/endpoints) · 3 bespoke pages (Dashboard, Users, Operators) + Slab Commission + Fund requests · action forms · Active toggle · view modals · white-label theming · seed dummy data (services/operators/slabs/denominations/providers/BBPS).

**Pending:**
- Report action modals with real backends (Print receipt, Raise Complaint, View API Log) — **backend endpoints are stubs**.
- Slab: Range Setup (DMT commission), Copy Commission, Bulk Update.
- Users: Send Password, Edit, Add (full create), KYC approve.
- Many `TransactionReport/*` are **"Feature under development"** stubs → pages show designed empty-state until backend is built. **Backend is the gate, not the UI.**
- Next role after admin: **API User**.

---

## 8. Recipe — add / fix a page (the workflow the user wants)
1. Open the legacy page `UI/src/Pages/InnerPages/<Feature>/*.js` → read `createColumns()` (table cols) + the Formik filters + the form fields.
2. Find the real endpoint in `UI/src/Components/Api/*API.js`, then **confirm the new name** in `Fino_API/Controllers/<X>Controller.cs` (GetAll/Create/…) and field names in `Fino_API/Requests/<Entity>/*.cs`.
3. Add a `registry.ts` entry of the right kind (or a `txnConfigs.ts` config for reports). Use **our** components — never the MUI look.
4. `npm run build` (must be green) → optionally verify live with a throwaway Node script (login → call endpoint → check `status:SUCCESS`).

## 9. Dev run & creds
- Backend: `dotnet run --project Fino_API --launch-profile http` → `http://localhost:5266`.
- Frontend: `cd Fino_Web && npm run dev` (`.env` → `VITE_API_URL=http://localhost:5266/api/`).
- Admin login: **9999999999 / Admin@123**.
- Seed file: `seed_admin.sql` (user types + admin). Dummy masters seeded via API this session.

---

## 10. User Types / Roles  (`Fino_Web/src/lib/roles.ts` ↔ `Fino_API/StaticModels/UserTypeCode.cs`)

| Code | Key | Display name | Notes |
|-----:|-----|--------------|-------|
| 0 | AllUsers | (All) | used as "any" filter |
| **1** | **Admin** | Administrator | full portal (this is what we built) |
| 2 | SuperDistributor | Super Distributor | distributor tier |
| 3 | Distributor | Distributor | distributor tier |
| 4 | Retailer | Retailer | sells to end customers |
| 5 | ApiUser | API User | integration/reseller via API |
| 6 | SupportUser | Support | admin-like, limited |
| 7 | Client | Client | white-label client |
| 9 | Accountant | Accountant | admin tier (accounts) |
| 10 | SDFOS | SD Field Officer | field staff |
| 11 | DFOS | D Field Officer | field staff |
| 999 | DeactiveUsers | (Deactivated) | soft-disabled |

**Helpers (`roles.ts`):** `roleLabel(id)` → name · `isRetailer(id)` · `isDistributorTier(id)` (2|3) · `isAdminTier(id)` (1|6|9).

**Where UserType drives behaviour:**
- **Menu** is role-filtered **server-side** — `SidebarMenu/Select` returns only the logged-in role's tree. The 118-item map in §5 is the **Admin (1)** menu. Other roles get smaller menus (API User next to build).
- **KeyData** sends `UserID` + `UserTypeID`; backend cross-checks them against the JWT (`sub`/claims) → **anti-impersonation**. (Auth fix: `JwtTokenService.ValidateToken` calls `InboundClaimTypeMap.Clear()` so `sub` is readable.)
- **Role-aware report columns** — `ReportColumn.roles?: number[]`; a column shows only if `roles.includes(sessionUserTypeID)`. Conventions used in `txnConfigs.ts`:
  - `ADMIN = [1,6]` → GST/Dist-GST/Super-TDS/Admin-commission/Profit columns.
  - `ADMIN_DIST = [1,6,2,3]` → MDS/DS commission.
  - `ADMIN_API = [1,6,5]` → API Request ID.
- **Entity picker** — `UsersScreen` / report filters show the user/entity Autocomplete only for `ADMIN_ROLES = [1,2,3,6]` (admin/dist/support).
- **Session** (`storage.ts`) keeps `user.{UserID,UserTypeID,PersonName,FirmName,...}`; read via `loadSession()` (e.g. `TxnReportScreen` filters columns by `loadSession()?.user.UserTypeID`).
- **Forms** with role audience use `ROLE_OPTIONS = [{2 Super Distributor},{3 Distributor},{4 Retailer},{5 API User}]` (Push Alert audience, etc.).

---

## 11. Component & method catalog (every reusable piece)

### `src/lib/` (pure helpers)
| File | Exports | Does |
|------|---------|------|
| `crypto.ts` | `encrypt(s)`, `decrypt(s)` | AES-256-CBC enc/dec (Base64); decrypt returns '' on failure |
| `apiClient.ts` | `apiPost<T>(endpoint,payload,{auth})` → `{ok,status,message,data}` | builds multipart Request+KeyData, parses camelCase envelope, decrypts data, 401→refresh→retry |
| `storage.ts` | `loadSession`, `saveSession`, `clearSession`, `patchTokens` | localStorage session {user,tokens} |
| `format.ts` | `inr(n)`, `dateTime(s)`, `apiDate(d)` | ₹ money, dd-MMM-yyyy hh:mm, yyyy-MM-dd |
| `row.ts` | `getField(row,...names)`, type `Row` | case-insensitive multi-alias read of PascalCase API rows |
| `csv.ts` | `exportToCsv(rows,name)` | client CSV download |
| `cn.ts` | `cn(...classes)` | classnames merge |
| `useClientSearch.ts` | `useClientSearch(rows)` → `{query,setQuery,filtered}` | client-side text filter |
| `roles.ts` | `UserTypeCode`, `roleLabel`, role helpers | see §10 |

### `src/api/`
`auth.api` → `login(creds)`, `logout()` · `menu.api` → `getMenu()` (`SidebarMenu/Select`, returns `MenuNode[] {name,url,icon,children}`) · `list.api` → `fetchList(endpoint,payload)` (→`[]` on FAIL) · `reports.api` → `fetchReport(endpoint,filters)`, `defaultReportFilters()`.

### `src/components/ui/` (design primitives)
| Component | Key props | Notes |
|-----------|-----------|-------|
| `Button` | `variant(outline/ghost/…)`, `size(sm)`, `loading`, `onClick`, `type`, `className` | spinner when loading |
| `Input` | `label`, `error`, `hint`, `leftIcon`, …input attrs | 40px field, focus ring |
| `Select` | `label`, `value`, `onChange`, `error`, children `<option>` | |
| `Badge` / `StatusBadge` | `tone(neutral/success/danger/warning/info/brand)` / `status` | StatusBadge maps SUCCESS/PROCESS/FAILURE/REFUNDED/ACCEPT/REJECT → colour |
| `Card` / `CardHeader` / `CardTitle` / `CardBody` | `className`, children | white surface, `rounded-card shadow-card` |
| `Modal` | `open`, `onClose`, `title`, `size(lg)`, children | |
| `Skeleton` / `EmptyState(icon,title,description,action)` | | loading & empty |
| `PageHeader` | `title`, `subtitle`, `actions`, `icon` | brand icon badge |
| `StatTile` | `label`, `value`, `icon`, `tone`, `loading` | KPI tile |
| `TableToolbar` | `query`, `onQuery`, `count`, `onExport` | search + export bar |
| `DynamicTable` | `rows`, `loading`, `actions(row)?`, `maxColumns`, `emptyTitle/Description` | auto columns + money/date/status format, hides sensitive cols |
| `DynamicForm` | `fields:FieldDef[]`, `initial?`, `onSubmit`, `onCancel?`, `submitting`, `submitLabel` | renders text/number/checkbox/select/date/password/**hidden**; `AsyncSelectField` for FK dropdowns; coerces types |
| `Autocomplete` | `label`, `options:{value,label,sublabel}[]`, `value`, `onChange`, `loading` | filtered combobox |
| `Toast` (`useToast()`) | `.success(m)`, `.error(m)`, `.info(m)` | toasts |

### `src/components/layout/`
`AppShell` (sidebar+topbar+outlet) · `Sidebar` (live role menu, collapsible groups, FALLBACK_MENU) · `Topbar` (brand palette switch, profile) · `iconMap.pickIcon(title,icon?)` (per-page lucide icon) · `resolveRoute.resolveHref(node)` (menu node → `/dashboard` | `/s/<url>` | `/coming-soon`).

### `src/screens/` (renderers + config)
- `registry.ts` — `SCREENS: ScreenDef[]`, `findScreen(path)`, helpers `list()/report()/form()`, field arrays, `USER_SELECT/ROLE_OPTIONS/TXN_STATUS_OPTIONS`.
- `ScreenHost.tsx` — dispatch by kind; `CUSTOM_SCREENS` map.
- `crudTypes.ts` — `FieldDef`, `CrudConfig`, `FormConfig`.
- `useCrud.ts` — CRUD mutations + `toggleMut` (mode-aware), `onDelete`.
- Generic: `ReportScreen`, `ListScreen` (auto stat-strip), `CrudScreen` (toggle + row-links + modals), `FormScreen`.
- Transaction: `TxnReportScreen` + `txnReportTypes.ts` (`ReportColumn`,`TxnReportConfig`) + `txnConfigs.ts` (33 report configs).
- Bespoke (`custom`): `UsersScreen`, `OperatorsScreen`, `SlabCommissionScreen`, `FundRequestsScreen`.

### Config type shapes
```ts
ScreenDef   { path, title, kind:'report'|'list'|'crud'|'custom'|'form'|'txn', subtitle?, endpoint?, payload?, crud?, form?, txn?, component? }
FieldDef    { name, label, type?:'text'|'number'|'checkbox'|'select'|'date'|'password'|'hidden', required?, options?, optionsEndpoint?, optionValue?, optionLabel?, defaultValue?, colSpan?, hint? }
CrudConfig  { idField, list:{endpoint,payload?}, create?, update?:{endpoint,fields,mode?:'field'|'object'}, remove?, links?:[{label,to(row)}] }
FormConfig  { endpoint, fields:FieldDef[], submitLabel?, successMessage?, description? }
ReportColumn{ key, label, type?:'text'|'money'|'date'|'status'|'bold'|'image'|'number', roles?:number[] }
TxnReportConfig { endpoint, columns:ReportColumn[], filters?:{service,operator,status,user,complaint,criteria,provider}, actions?:string[] }
```

---

## 12. Key data structures (on the wire)

**KeyData (every authed call):** `{ UserID, UserTypeID, SessionToken: AccessToken, DeviceType:'Web', IPAddress:'' }` (encrypted).

**Login result (`Auth/login` → decrypted data):** `{ user:{ UserID, UserTypeID, PersonName, FirmName, MobileNumber, EmailID, UserType, IsKYCApproved, IsTwoFA, … }, tokens:{ AccessToken, RefreshToken } }`.

**Envelope:** `{ statusCode, status:'SUCCESS'|'FAILED'|'ERROR', message, data }` (camelCase) — `data` usually AES-encrypted; decrypt → PascalCase.

**Report payload (txn):** `{ TakeCount, SkipCount, Search, SortColumnName, SortDirection, FromDate, ToDate, Status, Complaint, ReportBy:'Inline', UserID, ServiceID, OperatorID, ProviderID, ExportType, IsExcelDownload, Criteria, CriteriaText, MinAmount, MaxAmount, Latitude, Longitude }` → response `{ list:[…], Count, HeaderSummary:{…} }`.

**Master list rows (PascalCase) examples:**
- Service: `ServiceID, Name, ServiceCategory, HSNCode, Charge, MinBalance, IsActive, IsDisplay, IsDefault, IsBBPS, …`
- Slab: `SlabMasterID, SlabName, IsActive, DateTime`
- SlabDetail: `SlabDetailID, SlabMasterID, SlabName, ServiceID, ServiceName, OperatorID, OperatorName, FromAmount, ToAmount, RetailerComm, …`

**Status option sets (`ReportHelper`/`txnConfigs`):** recharge `ALL/SUCCESS/PROCESS/FAILURE`; dmt adds `REFUNDED`; complaint `ALL/ACCEPT/REJECT/PROCESS`; export `Select Download/EXCEL/PDF`; fund accept/reject `Accept/Reject`.

**Backend DB:** SQL Server `FintechApp`, EF Core, ~228 tables (`Fino_API/Entities/ApplicationDbContext.cs`). Masters follow `<Entity>Master` / `<Entity>` naming; requests in `Fino_API/Requests/<Entity>/{Create,Update,Delete,Get}XRequest.cs`; controllers in `Fino_API/Controllers/<X>Controller.cs`.

---

## 13. DEEP file-by-file reference (every component — path · exports · props · behaviour)

> Read this to know exactly *what each thing is and where it lives*. Paths are under `Fino_Web/src/`.

### 13.1 App bootstrap & providers
- **`main.tsx`** — React root; renders `<App/>` with `index.css` (Tailwind v4 `@theme` tokens).
- **`App.tsx`** — provider tree + routes. Tree: `QueryClientProvider` → `BrandProvider` → `BrowserRouter` → `AuthProvider` → `ToastProvider` → `Routes`.
  Routes: `/login` (public) · `ProtectedRoute` → `AppShell` wraps → `/dashboard`, `/recharge`, `/transactions`, `/appearance`, `/profile`, `/coming-soon`, **`/s/*` → `ScreenHost`** (the dynamic admin engine) · `*` → `NotFoundPage`.
- **`lib/queryClient.ts`** — `queryClient` (TanStack): `retry:1, refetchOnWindowFocus:false, staleTime:30s`.
- **`routes/ProtectedRoute.tsx`** — `<ProtectedRoute/>`: if `!useAuth().isAuthenticated` → `<Navigate to="/login">` (saves `from`); else `<Outlet/>`.

### 13.2 Auth  (`features/auth/AuthContext.tsx`)
- **`AuthProvider`** — holds `session` (from `loadSession()`); listens to `SESSION_EXPIRED_EVENT` (hard-logout).
- **`useAuth()`** → `{ user:LoginUser|null, isAuthenticated:boolean, signIn(creds)→{ok,message}, signOut() }`. `signIn` calls `auth.api.login`, saves session; `signOut` calls `Auth/logout` then clears.
- Consumed by: Topbar, ProtectedRoute, LoginPage.

### 13.3 Theme / white-label  (`theme/`)
- **`themes.ts`** — `Ramp` (50–950 hex map), `ThemePreset {key,label,swatch,brand}`, `THEMES` (6 presets: **azure**(default), emerald, violet, sunset, rose, ocean), `DEFAULT_THEME='azure'`, `getTheme(key)`.
- **`BrandProvider.tsx`** — `BrandProvider` injects `--color-brand-*` CSS vars on `<html>` (so Tailwind brand utilities re-theme live); loads `public/config.json` (appName, tagline, logoUrl, loginHeadline, loginSubtext, theme). **`useBrand()`** → `{appName,tagline,logoUrl,loginHeadline,loginSubtext, themeKey, themes, setTheme(key)}`. Persists theme in `localStorage 'fino.theme'`.

### 13.4 `lib/` helpers
| File | Exports | Signature / behaviour |
|------|---------|------------------------|
| `cn.ts` | `cn(...x)` | clsx+twMerge classnames |
| `crypto.ts` | `encrypt(s)`, `decrypt(c)` | AES-256-CBC/PKCS7 Base64 (defaults to app key/IV; env override `VITE_ENC_KEY/IV`) |
| `apiClient.ts` | `apiPost<T>(endpoint,payload?,{auth=true})`, `SESSION_EXPIRED_EVENT`, `apiBaseUrl()` | builds multipart `Request`+`KeyData`, reads camelCase envelope via case-insensitive `field()`, `unwrap()` decrypts data→JSON (fallback raw), 401→`tryRefresh`→retry→`forceLogout` |
| `storage.ts` | `loadSession`, `saveSession`, `patchTokens`, `clearSession` | localStorage key `fino.session` → `Session{user,tokens}` |
| `format.ts` | `inr(v)`, `num(v)`, `dateTime(v,pattern?)`, `dateOnly(v)`, `apiDate(Date)` | Intl en-IN money/number; date-fns formatting; `apiDate`=yyyy-MM-dd |
| `row.ts` | `getField(row,...names)`, type `Row` | case-insensitive multi-alias read |
| `csv.ts` | `exportToCsv(rows,filename)` | union-of-keys CSV, BOM, blob download |
| `useClientSearch.ts` | `useClientSearch(rows)` → `{query,setQuery,filtered}` | filters across all row values |
| `roles.ts` | `UserTypeCode`, `roleLabel`, `isRetailer`, `isDistributorTier`, `isAdminTier` | see §10 |

### 13.5 `types/auth.ts`
`LoginUser {UserID,PersonName,FirmName,MobileNumber,EmailID,UserTypeID,UserType,IsKYCApproved,IsTwoFA}` · `Tokens {AccessToken,RefreshToken,ExpiresIn,TokenType,JwtId}` · `LoginResult {user,tokens}` · `Session {user,tokens}`.

### 13.6 `api/`
- `auth.api.ts` — `login(creds)` (`Auth/login`, anonymous), `logout()`.
- `menu.api.ts` — `getMenu()` (`SidebarMenu/Select`) → `MenuNode[] {name,url?,icon?,children?}`.
- `list.api.ts` — `fetchList(endpoint,payload)` → `ListRow[]` (→`[]` on FAIL; unwraps array or first-array-in-object).
- `reports.api.ts` — `fetchReport(endpoint,filters)`, `defaultReportFilters()`, types `ReportFilters/ReportRow`; `normalizeRows` finds `list/List/Data/Items/...`.
- `dashboard.api.ts`, `recharge.api.ts`, `transactionReport.api.ts` — used by the **older standalone pages** (Dashboard/Recharge/Transactions) built before the `/s/*` registry engine.

### 13.7 `components/ui/` (all props verified)
| File | Component(s) | Props |
|------|--------------|-------|
| `Spinner.tsx` | `Spinner` | `className` (border-spinner) |
| `Button.tsx` | `Button` | `variant:'primary'|'secondary'|'outline'|'ghost'|'danger'`, `size:'sm'|'md'|'lg'`, `loading`, + button attrs; shows Spinner when loading |
| `Input.tsx` | `Input` | `label,error,hint,leftIcon` + input attrs (40px, focus ring) |
| `Select.tsx` | `Select` | `label,error` + select attrs + `<option>` children (chevron) |
| `Card.tsx` | `Card,CardHeader,CardTitle,CardBody` | div attrs; `Card` = white `rounded-card border shadow-card` |
| `Badge.tsx` | `Badge`, `StatusBadge` | `Badge tone:'neutral|success|danger|warning|info|brand'`; `StatusBadge status` maps SUCCESS/ACCEPT→green, PROCESS/PENDING→amber, FAIL/REFUND/REJECT→red |
| `Modal.tsx` | `Modal` | `open,onClose,title,size,children` (overlay + panel) |
| `Skeleton.tsx` | `Skeleton` | `className` (shimmer) |
| `EmptyState.tsx` | `EmptyState` | `icon?,title,description?,action?` |
| `Toast.tsx` | `ToastProvider`, `useToast()` | `useToast()→{success,error,info}`; auto-dismiss 4.5s, top-right stack |
| `PageHeader.tsx` | `PageHeader` | `title,subtitle?,actions?,icon?` (brand icon badge) |
| `StatTile.tsx` | `StatTile` | `label,value,icon,tone(classes),loading?` — compact KPI (used by bespoke list pages) |
| `StatCard.tsx` | `StatCard` | `label,value,sub?,icon?,accent:'brand|success|danger|warning|info|neutral',loading?` — bigger KPI (dashboard) |
| `TableToolbar.tsx` | `TableToolbar` | `query,onQuery,count,onExport` |
| `DataTable.tsx` | `DataTable<T>`, `Column<T>` | **typed** columns `{key,header,align?,render?(row)}` — used by older pages |
| `DynamicTable.tsx` | `DynamicTable` | `rows,loading?,maxColumns?,emptyTitle?,emptyDescription?,actions?(row)` — **auto-derives** columns from row keys, money/date/status/bool format, hides `password|hash|token|id|guid|secret`, skeleton/empty (used by list/crud framework) |
| `DynamicForm.tsx` | `DynamicForm` | `fields:FieldDef[],initial?,onSubmit,onCancel?,submitting?,submitLabel?` — renders by FieldDef.type incl. `hidden`; `AsyncSelectField` loads FK options via `optionsEndpoint`; validates required; coerces number/bool |
| `Autocomplete.tsx` | `Autocomplete`, `AutocompleteOption` | `label?,placeholder?,options,value,onChange,loading?` — client-filtered combobox, outside-click close |

> **Two table & two stat components exist:** `DataTable`/`StatCard` (early typed, used by hand-built pages Dashboard/Recharge/Transactions) and `DynamicTable`/`StatTile` (config-driven, used by the `/s/*` admin engine). Prefer the dynamic ones for new admin screens.

### 13.8 `components/layout/`
- **`AppShell.tsx`** — `AppShell`: flex shell (desktop Sidebar + mobile drawer) + `Topbar` + `<main><Outlet/>`. Mobile menu state.
- **`Sidebar.tsx`** — `Sidebar({onNavigate?})`: fetches live role menu (`getMenu`), renders collapsible groups (`MenuEntry` recursive), brand logo/name, `FALLBACK_MENU` (Dashboard/Recharge/Transactions) if API empty; links via `resolveHref`.
- **`Topbar.tsx`** — `Topbar({onMenuClick})`: mobile hamburger, **theme switcher** (`useBrand`), **user menu** (`useAuth`: profile/appearance/sign-out), shows `roleLabel`.
- **`iconMap.ts`** — `pickIcon(name?,icon?)`: keyword→lucide icon (dashboard, recharge, dth, report, fund, wallet, transfer, payout, dmt, aeps, bank, commission, user, operator, provider, setting, …) → fallback `CircleDot`.
- **`resolveRoute.ts`** — `resolveHref(node)`: dashboard→`/dashboard`, registered url→`/s<url>`, else `/coming-soon?title=`.

### 13.9 `screens/` (the dynamic admin engine — see §4 for kinds)
- `registry.ts` — `SCREENS[]`, `findScreen(path)`, `ScreenDef`, `ScreenKind`, `CustomScreenProps`; helpers `list/report/form`; field arrays (SERVICE_CREATE, OPERATOR_CREATE, FUND_TRANSFER_FIELDS, …); `USER_SELECT/ROLE_OPTIONS/TXN_STATUS_OPTIONS`.
- `ScreenHost.tsx` — reads `/s/<sub>`, dispatches by kind; `CUSTOM_SCREENS = {users, operators, slabCommission, fundRequests}`.
- `crudTypes.ts` — `FieldType`, `FieldDef`, `CrudConfig`, `FormConfig`.
- `useCrud.ts` — `useCrud(crud)` → `{rows,isFetching,adding,setAdding,editing,setEditing,createMut,updateMut,deleteMut,toggleMut,onDelete}` (update is field-level or object per `crud.update.mode`; `toggleMut` flips IsActive).
- `ReportScreen.tsx` / `ListScreen.tsx` / `CrudScreen.tsx` / `FormScreen.tsx` — generic renderers (kinds report/list/crud/form).
- `TxnReportScreen.tsx` (+ `txnReportTypes.ts`, `txnConfigs.ts`) — rich transaction reports (filters, summary tiles, role-aware explicit columns, server pagination, View-details modal, row-action kebab). 33 configs.
- Bespoke: `UsersScreen.tsx` (avatar roster + stats + activate/view), `OperatorsScreen.tsx` (logo card grid + CRUD + toggle), `SlabCommissionScreen.tsx` (SlabDetail CRUD per slab), `FundRequestsScreen.tsx` (accept/reject).

### 13.10 `pages/` (top-level routed pages)
- `LoginPage.tsx` — login form (mobile+password, RHF+zod), `useAuth().signIn`, brand copy from `useBrand`.
- `DashboardPage.tsx` — admin dashboard: gradient hero, `StatCard` KPIs, recharts Bar + donut Pie, breakdown cards (`Dashboard/Select`).
- `RechargePage.tsx` / `TransactionsPage.tsx` — early hand-built pages (recharge form / transaction report) using `recharge.api`/`transactionReport.api` + `DataTable`.
- `AppearancePage.tsx` — theme picker (persist). `ProfilePage.tsx` — session user details.
- `PlaceholderPage.tsx` — "coming soon" titled placeholder (for unmapped menu). `NotFoundPage.tsx` — 404.

---

> **TL;DR for a new dev:** `App.tsx` wires providers → `Sidebar` shows the live role menu → clicking routes to `/s/<url>` → `ScreenHost` finds the `registry.ts` entry → renders the matching screen kind (`ReportScreen`/`ListScreen`/`CrudScreen`/`FormScreen`/`TxnReportScreen`/bespoke). Data flows through `apiPost` (AES + envelope). To add a page: read the legacy page + the `Fino_API` controller/DTO, then add one registry entry. To restyle for a client: change `public/config.json` (brand + theme).
