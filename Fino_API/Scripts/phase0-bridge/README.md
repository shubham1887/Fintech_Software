# Phase 0 — Gross-to-Taxable Bridge

CA ke "1 crore kahan gaya" wale sawaal ka jawab nikalne ke liye read-only reports.
Koi naya table nahi banta, koi code change nahi hota — yeh sab **purane data par SELECT** hai.

Poora design: *Finocore — Dynamic GST, TDS & Accounting Blueprint*, Section 7 aur 14.

---

## Pehle yeh samajh lo

Aapka recharge business trading model par chalta hai:

```
Closing stock = Opening stock + Purchase − Sale
```

Yani **19 cr purchase − 18 cr sale = 1 cr closing stock**. Agar provider wallets mein
pada hua total ~1 crore hai, to paisa kahin gaya nahi — wo aapka **stock** hai, jo
balance sheet mein current asset ki tarah dikhna chahiye tha. Us case mein koi GST
liability nahi banti: purchase par ITC liya, sale hone par output GST bharoge.

Teen mein se ek jawab milega:

| Provider wallets ka closing total | Matlab | Action |
| --- | --- | --- |
| ~₹1 crore | Gap poora stock hai | CA ko balance confirmation do |
| ~₹0 | Stock nahi hai — sale invoice nahi bani | Exchange legs aur non-GST parties pakdo |
| Beech mein, maano ₹60 L | 60 L stock + 40 L unbilled sale | Dono alag-alag settle karo |

---

## Scripts

| File | Kya jawab deta hai |
| --- | --- |
| `00_provider_wallet_closing.sql` | **Core test.** Provider wallets ka live balance, cut-off date ka closing, derived purchase (top-ups), throughput, aur stock tie-out |
| `01_gross_to_taxable_bridge.sql` | Wallet-wise aur service-wise bridge, har service ka billing basis, aur company-level `UnexplainedGap` |
| `02_exchange_partner_register.sql` | Peer-to-peer transfers, reciprocal pairs (asli exchange), API users ka net position, aur line-by-line detail |
| `03_non_gst_party_register.sql` | Registered vs unregistered turnover, party-wise detail, PAN/state-code exceptions, 194H threshold tracker |
| `04_data_quality_checks.sql` | HSN/SAC galat ya missing, GST-inclusive flag, company profile, orphan transactions, status sanity, invoice numbering |

Sab scripts `SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED` use karte hain, taaki
live install block na ho.

---

## Kaise chalayein

### Option A — PowerShell (saare reports + CSV export)

```powershell
.\Run-Phase0.ps1 -Database finocore_db -From 2025-04-01 -To 2026-03-31
```

Doosre server par, SQL authentication ke saath:

```powershell
.\Run-Phase0.ps1 -Server "SQLPROD\INST1" -Database finocore_db -From 2025-04-01 -To 2026-03-31 -UserName sa -Password "***" -OutDir D:\phase0-out
```

Har script ka CSV `out-<timestamp>\` folder mein banta hai. Wahi CA ko bhejna hai.

### Option B — SSMS

Har `.sql` file kholo, upar `@From` / `@To` apne FY par set karo, aur F5. Har script
kai result sets deta hai, har ek par `Section` column laga hai.

---

## Production par chalane se pehle

1. **Date range set karo.** Default FY 2025-26 (`2025-04-01` se `2026-03-31`) hai.
2. **Table naming check karo.** Is codebase mein naming consistent nahi hai —
   zyada-tar tables plural hain (`TransactionMasters`, `UserMasters`,
   `ProviderMasters`), par kuch singular hain (`WalletMaster`,
   `UserBalanceMaster`). Scripts `finocore_db` ke actual schema par verify ki gayi
   hain. Agar aapke prod schema mein farq hai to script pehle hi error de degi —
   chupchaap galat number nahi degi.
3. **Balance table.** Live user balance `UserWalletMasters` mein hai
   (`Balance` + `HoldBalance`), `UserBalanceMaster` mein nahi — wo khali rehta hai.
4. **Pehle backup copy par chalao** agar ho sake. Queries read-only hain, par 19 crore
   ke data par bade scan honge.

---

## Basis map — isko CA se confirm karna hai

`01_gross_to_taxable_bridge.sql` ke upar ek `@BasisMap` table hai jo har
`ServiceCategory` ko billing basis aur sahi SAC se jodti hai:

- `GROSS` — recharge, e-topup. Taxable value = poora top-up (Ezytm, Erech ka model)
- `COMMISSION` — DMT, AEPS. Taxable value = commission earned (JaiJui ka model)
- `CHARGE` — payout, verification, BBPS. Taxable value = charge

**Yeh mapping edit karni hai** CA ka jawab aane ke baad (Blueprint Section 15, sawaal #2).
Jab tak Section D (`UNMAPPED SERVICES`) khali na ho, bridge adhoora hai.

---

## CA ko kya dena hai

Priority order:

1. `00_provider_wallet_closing.csv` → section **A-TOTAL** ka `GrandTotalClosingStock`
   — yahi "1 crore" ka seedha jawab hai
2. `01_gross_to_taxable_bridge.csv` → section **C** ka `UnexplainedGap`
   — stock aur wallet balance nikaalne ke baad jo bacha
3. `02_exchange_partner_register.csv` → section **B** ka `TotalTurnoverMissed`
   — barter ka wo turnover jo books mein aana chahiye tha
4. `03_non_gst_party_register.csv` → section **A** ka unregistered turnover
5. `04_data_quality_checks.csv` → jo bhi issue mile, taaki CA ko pata ho kin numbers
   par kitna bharosa karna hai

---

## Phase 0 ki limitations — CA ko yeh bhi batana

Yeh honest list hai. In sab ka permanent fix Phase 1–3 mein hai.

- **Purchase derived hai, actual nahi.** System mein purchase invoice ka table hai hi
  nahi. Script provider wallet ke balance jump se top-up derive karta hai. Isko
  **apne actual purchase bills se cross-check karna zaroori hai** — dono match nahi
  karein to difference khud ek finding hai.
- **Agar kisi provider par koi transaction hi nahi hui**, to uska top-up derive nahi
  hoga. Us provider ka purchase manually add karna padega.
- **Sale bhi proxy hai.** Sale invoice table nahi hai, isliye script wallet top-up ko
  sale ka base maanta hai (aapki current practice yahi hai).
- **Live balance aaj ka hai, cut-off date ka nahi.** `ProviderMasters.RechargeBalance`
  current value hai. FY ke turant baad chalao to yahi sahi hai; baad mein chalao to
  section B (`ClosingBalanceAtCutOff`) dekho.
- **PENDING transactions** na sale hain na stock — wo bridge ka residue banti hain.
  Pehle unko settle karao (`04` ka section E).
- **194H ka number upper bound hai.** Supreme Court (Bharti Cellular, 2024) ke hisaab
  se recharge par diya gaya discount commission nahi hai, isliye uspar 194H lagta hi
  nahi. System mein abhi `RelationshipType` field nahi hai, isliye script recharge
  discount aur DMT commission ko alag nahi kar sakta.
- **Commission ka sign** kuch jagah negative aata hai. Threshold check `ABS()` par hai,
  aur jahan sign ulta hai wahan `SignWarning` column aata hai.

---

## Safety

Saare scripts **sirf SELECT** chalate hain. Koi `INSERT`, `UPDATE`, `DELETE`, ya DDL
nahi. `Run-Phase0.ps1` original `.sql` files ko modify nahi karta — wo output folder
mein ek temp copy banakar usme `@From`/`@To` set karta hai.
