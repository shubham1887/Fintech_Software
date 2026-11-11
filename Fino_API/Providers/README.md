# Naya provider kaise add karein

Is folder ka matlab: **provider integrations**. Do tarah ke hote hain. **Pehle ye decide karo ki
aapka provider kaun sa hai** — galat raasta chunne se mehnat zaaya hoti hai.

| | **Template provider** | **Coded provider** |
|---|---|---|
| Kya hai | Normal HTTP API — URL, parameters, JSON/XML reply | Aisa protocol jo URL template se nahi ban sakta |
| Example | Payin, zyadatar recharge/BBPS aggregators | Pay1 (API key ka hash har call mein), JaiJui (apni client library), Nexmint (har call par 5-second JWT) |
| Kitna kaam | **Zero code.** Sirf panel se | Ek project + **ek adapter class** |
| Deploy chahiye | Nahi | Haan |

**Simple test:** provider ne aisa URL diya hai jo aap Postman ya browser mein paste karke chala
sakte ho? → **Template provider** hai, code likhne ki zaroorat nahi.

Code sirf tab chahiye jab request mein kuch aisa ho jo string mein value bhar ke nahi banta —
signature, hash, SDK handshake, mutual TLS.

---

# Raasta A — Template provider (code bilkul nahi)

Sab kuch panel se, isi order mein. Developer ki zaroorat nahi.

### Step 1 — Provider add karo
**API Providers → Add provider** — naam, base URL, Login ID, Token. Save → Active.

### Step 2 — Endpoint banao
**Setting → Request → Add endpoint**

Har service aur har kind (Transaction / Balance / Status / Bill fetch) ke liye ek endpoint.
URL/body template mein placeholders:

```
https://example.com/api/pay?key=[Token]&num=[Number]&amt=[Amount]&op=[OperatorCode]&ref=[RequestID]
```

POST ke liye `?` ke baad `{` se shuru hone wala hissa JSON body ban jaata hai.

> Form par jo placeholder chips dikhte hain, wo engine se hi aate hain — matlab jo chip dikh raha
> hai, engine usi ko replace kar sakta hai. Apne se naya token mat likho.

### Step 3 — Operators map karo
**Setting → Configuration** — har operator ke saamne **provider ka apna code**.

**Ye step skip mat karna.** Iske bina provider tak kuch nahi jaata, chahe baaki sab set ho.

### Step 4 — Response rules banao
**Setting → Response** — reply ko Success / Failed / Pending mein badalne ke rules.

Provider ka asli sample reply paste karke **Test** dabao — rule us sample par chal jaayega,
provider ko call kiye bina.

### Step 5 — Routing
**Provider Routing** → service chuno → provider par tick → priority number → Save.

---

# Raasta B — Coded provider (ek class)

Maan lo naya provider **abc** hai, aur `Fino.AbcApi` project ke roop mein aaya hai
(bilkul `Fino.Pay1Api` jaisa).

**Kul kaam: 2 nayi file + 2 line. Bas.**

### Step 1 — Project ko jodo

`Fino_API/FintechApp.csproj` mein (line ~31 ke paas ye already hai):

```xml
<ProjectReference Include="..\Fino.JaiJuiApi\Fino.JaiJuiApi.csproj" />
<ProjectReference Include="..\Fino.Pay1Api\Fino.Pay1Api.csproj" />
```

Ek line add karo:

```xml
<ProjectReference Include="..\Fino.AbcApi\Fino.AbcApi.csproj" />
```

`FintechApp.sln` mein bhi project add karo, taaki root se `dotnet build` uthaa le.

> **Sirf `.dll` hai, source nahi?** Utne se kaam nahi chalega. System mein koi plugin loader nahi
> hai — usey pata hi nahi chalega ki konsi class banani hai aur konsa method call karna hai. DLL
> ka public API padh kar (reflection se dikh jaata hai) Step 2–3 phir bhi likhne padenge. Sahi
> shakal: source ke saath sibling project.

### Step 2 — Nayi file 1: credentials ka mapping

`Fino_API/Providers/AbcProviderResolver.cs`

```csharp
using FintechApp.Entities;

namespace FintechApp.Providers
{
    public static class AbcProviderResolver
    {
        // Panel mein jis provider ke naam mein "ABC" hai, wahi ye integration hai.
        public const string Marker = "ABC";

        public static bool IsAbc(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Replace(" ", string.Empty).Contains(Marker, StringComparison.OrdinalIgnoreCase);

        public static AbcCredentials CredentialsFrom(ProviderMaster p) => new()
        {
            BaseUrl   = p.ProviderUrl?.Trim() ?? string.Empty,   // khali -> library ka default
            PartnerId = p.LoginID?.Trim()     ?? string.Empty,
            ApiKey    = p.Token?.Trim()       ?? string.Empty,
        };
    }
}
```

**Do zaroori baatein:**

1. Credentials **ProviderMaster row** se aate hain, `appsettings.json` se nahi. Isi wajah se admin
   panel se hi provider add/edit/activate kar leta hai, aur ek hi build kai clients ko chalta hai.
2. **Marker hi pehchaan hai.** Jo bhi provider row banaye, usey bata do ki naam mein `ABC` hona
   zaroori hai.

### Step 3 — Nayi file 2: adapter

`Fino_API/Providers/Coded/AbcCodedProvider.cs`

```csharp
using FintechApp.Entities;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.StaticModels;

namespace FintechApp.Providers.Coded
{
    public class AbcCodedProvider : CodedProviderBase
    {
        private readonly Fino.AbcApi.Interfaces.IAbcApiService _abc;

        public AbcCodedProvider(
            Fino.AbcApi.Interfaces.IAbcApiService abc,
            IProviderRequestResponseLogRepository logRepo) : base(logRepo)
            => _abc = abc;

        // Ye provider mera hai?
        public override bool CanHandle(ProviderMaster p) => AbcProviderResolver.IsAbc(p.ProviderName);

        // Konsa rail serve karta hai. Yahi Provider Routing mein dikhata hai.
        public override string Family => ServiceKinds.BBPS;

        public override async Task<RechargeProviderResult?> Dispatch(
            ProviderMaster provider, ProviderConfiguration route, RechargeDispatchContext context)
        {
            // route.OpCode = is operator ke liye abc ka apna code
            // context.*    = consumer number, amount, hamari request id
            var reply = await _abc.Pay(/* abc ki request */, AbcProviderResolver.CredentialsFrom(provider));

            // Har call log mein jaani chahiye — Provider Logs report yahi padhti hai.
            await WriteLog(provider.ProviderID, context.UserID, context.TransactionMasterID,
                context.ServiceID, context.IPAddress, "Abc Pay …", reply.Data);

            return new RechargeProviderResult
            {
                ProviderID = provider.ProviderID,
                ProviderRequestID = context.ProviderRequestID,
                ProviderRefNo = reply.TransactionID,
                Status  = StatusType.Success,   // niche "Paise ke rules" dekho
                Message = reply.Message,
            };
        }
    }
}
```

**Sirf utne methods likho jo abc deti hai.** Chaaron optional hain:

| Method | Kab likhein |
|---|---|
| `Dispatch` | Provider transaction karta hai (recharge / bill payment) |
| `FetchBill` | Payment se pehle bill detail laata hai |
| `CheckStatus` | Transaction ka status bata sakta hai |
| `CheckBalance` | Apna wallet balance deta hai |

Jo chhod doge, wo `null` return karega — gateway samajh jaata hai "mera nahi" aur normal raaste
par chala jaata hai. **`JaiJuiCodedProvider.cs` dekho — usme sirf ek method hai.**

### Step 4 — Program.cs mein ek line

`Fino_API/Program.cs` (line ~319) — ye already hai:

```csharp
builder.Services.AddScoped<ICodedProvider, FintechApp.Providers.Coded.Pay1CodedProvider>();
builder.Services.AddScoped<ICodedProvider, FintechApp.Providers.Coded.JaiJuiCodedProvider>();
```

Ek line add karo:

```csharp
builder.Services.AddScoped<ICodedProvider, FintechApp.Providers.Coded.AbcCodedProvider>();
```

`Fino.AbcApi` ko jo chahiye (jaise `AddAbcApi()` extension), wo bhi register karo — jaise
`AddPay1Api()` line ~239 par hai.

### Step 5 — Callback (sirf agar provider result push karta ho)

`Fino_API/Services/Callbacks/AbcCallbackParser.cs`, `IProviderCallbackParser` implement karke.
Program.cs mein `GenericProviderCallbackParser` se **upar** register karo (wo baaki sab claim kar
leta hai).

Callback ka shared address: `/api/Callback/p/{CallbackKey}` — key provider row par dikhti hai.

### Step 6 — Build aur deploy

```
cd C:\Users\Administrator\Desktop\Fintech_Software\Fino_API
dotnet build
```

Phir self-contained publish + IIS deploy (recipe neeche "Deploy" section mein).

---

# Raasta C — Paisa bahar bhejne wala rail (DMT / Payout)

**`ICodedProvider` se alag hai.** Recharge/BBPS gateway se nahi jaata. DMT, Payout aur VendorPay —
teeno `IBankTransferProviderClient` → `DynamicBankTransferProviderClient` se jaate hain, aur wahan
se us provider ki library mein (`Fino.JaiJuiApi`, `Fino.NexmintApi`).

Konsa gateway chalega: **"DMT Settings" screen ki active rows, sabse chhota `Sequence` jeetta
hai.** Row apna `ProviderCode` = "Add API" wale provider ka `ProviderName` rakh ke us provider se
judti hai; credentials wahi se aate hain, appsettings se nahi.

| Provider | Resolver | Library | Rail |
|---|---|---|---|
| JaiJui | `JaiJuiGatewayResolver` | `Fino.JaiJuiApi` | Pure DMT + payout DMT |
| Nexmint | `NexmintGatewayResolver` | `Fino.NexmintApi` | Payout (single call, sender register nahi) |

### Nexmint payout — panel par kya bharna hai

"Add API" (API Providers) mein ek row:

| Field | Kya daalein |
|---|---|
| `ProviderName` | Naam mein **Nexmint** aana chahiye (match isi se hota hai) |
| `LoginID` | `client_id` |
| `Token` | `client_secret` — dono JWT isi se sign hote hain |
| `ProviderUniqueID` | **webhook secret** — ye alag key hai, client secret nahi |
| `ProviderUrl` | khaali chhodo (appsettings ka default), ya `.../gateway/sandbox` test ke liye |
| `IPAddress` | Nexmint ke webhook IP (khaali = koi bhi IP, callback key hi ekmatra lock) |

Phir "DMT Settings" mein ek active row jiska `ProviderCode` = upar wala `ProviderName`, aur
`Sequence` baaki rows se chhota.

**Us row par `IsPayoutDMT` tick karna mat bhoolna.** Nexmint payout rail hai — koi sender
register nahi hota, koi OTP nahi. Tick na ho to `DmtGatewayModeService` usey pure DMT samajhta
hai aur har transfer se pehle sender registration maangta hai, jo Nexmint ke paas hai hi nahi.

**Nexmint panel mein** (Developer Settings): apna server IP whitelist karao, aur Payout direction
ka webhook URL set karo — `https://<host>/api/Callback/p/<provider row ki CallbackKey>`.

### Do baatein jo paise ki hain

1. **Payout hamesha `pending` return karta hai.** Final result webhook se aata hai
   (`NexmintCallbackParser`) ya status enquiry se — dono hamare `order_id` (= `ProviderRequestID`)
   par chalte hain.
2. **Timeout / HTTP 500 ko FAIL mat samajhna.** Transfer upar ja chuka ho sakta hai. Code use
   PENDING rakhta hai; FAIL karne ka matlab hota retailer ko refund **aur** bank se paisa bhi
   chala jaana. Sirf 400/401/403 (saaf inkaar) par hi fail hota hai.

Bina credentials ke poora rail check karna ho: `dotnet run --project tools/verify-nexmint-payout`.

---

## ⚠️ Jisko haath NAHI lagana

**`Services/ProviderGatewayService.cs`**

Us file mein ab kisi provider ka naam hai hi nahi — wo registry se poochta hai. Agar provider add
karne ke liye usey edit karna pad raha hai, to **adapter mein kuch chhoot raha hai**.

---

# Panel side — deploy ke baad

### Step 7 — Provider add karo

**API Providers → Add provider**

| Field | Kya daalein |
|---|---|
| Provider name | **`Abc`** — naam mein "ABC" hona zaroori hai |
| Login ID | abc ka partner / member ID |
| API token | abc ki key |
| Base URL | khali chhodo (library ka default lag jaayega) |

Save → Active karo.

Row par **"Built-in · BBPS"** badge aa jaayega. **Ye badge dikh gaya = Step 1-6 sahi hue.**

### Step 8 — Operators map karo

**Setting → Configuration** — service chuno, har operator ke saamne abc ka operator code, Save.

Adapter khud se routes nahi banata. Ye step zaroori hai.

Request aur Response tab par kuch nahi karna — wahan likha aa jaayega
*"nothing to add here"*.

### Step 9 — Routing

**Provider Routing** → service chuno → abc par tick → priority number → Save.

Agar Step 8 nahi kiya aur yahan tick kar diya, to cell par amber warning dikhegi:
*"Not mapped — add this operator on the provider's Configuration tab."*

---

# 💰 Paise ke rules — ye style nahi hai, paisa hai

Adapter mein `Status` set karte waqt:

| Situation | Status | Kyun |
|---|---|---|
| Request hamare server se **gayi hi nahi** (credentials / opcode missing) | `IsConfigured = false` | Paisa safe hai — gateway sequence mein agla provider try karega |
| Request **gayi, par jawab nahi mila** | **`Pending`** | Payment ho bhi sakta hai |
| Jawab mein **saaf success** | `Success` | |
| Jawab mein **saaf failed** | `Failed` | Pipeline refund karega |
| **Baaki sab kuch** | **`Pending`** | Jo samajh na aaye, uspar paisa wapas mat karo |

> Jawab na mile aur aap `Failed` likh do → system refund kar dega, par provider ne paisa bhej diya
> hoga. **Double loss.** Isliye hamesha `Pending`.

---

# Test kaise karein

1. **Credentials pahunch rahe hain?** — API Providers mein us row par balance refresh ka icon
   dabao. Live call jaati hai, jawab (achha ya bura) jaisa hai waisa dikhta hai.
2. **Call ja rahi hai?** — Reports → **Provider Logs**. Adapter ne `WriteLog` se jo bheja, request
   aur raw reply ke saath yahan aata hai.
3. **Poora flow** — sabse chhoti amount ki **ek real transaction**. Status dekho → status
   re-check karao → callback aata ho to wo bhi.

**Teeno agree karein, tabhi integration poora hai.**

---

# Checklist

| Step | File | Kab chahiye |
|---|---|---|
| 1 | `FintechApp.csproj`, `FintechApp.sln` | hamesha |
| 2 | `Providers/AbcProviderResolver.cs` | hamesha |
| 3 | `Providers/Coded/AbcCodedProvider.cs` | hamesha |
| 4 | `Program.cs` — 1 line | hamesha |
| 5 | `Services/Callbacks/AbcCallbackParser.cs` | provider result push karta ho |
| 6 | build + deploy | hamesha |
| 7-9 | panel (add → map → route) | hamesha |

---

# Reference ke liye

| Cheez | File |
|---|---|
| Adapter ka contract | `Interfaces/Providers/ICodedProvider.cs` |
| Base class (WriteLog, Truncate) | `Providers/Coded/CodedProviderBase.cs` |
| **Poora example** (chaaron methods) | `Providers/Coded/Pay1CodedProvider.cs` |
| **Chhota example** (sirf ek method) | `Providers/Coded/JaiJuiCodedProvider.cs` |
| Registry | `Providers/CodedProviderRegistry.cs` |
| Callback plug point | `Interfaces/Services/IProviderCallbackParser.cs` |
| Konsi service konsa rail | `StaticModels/ServiceFamily.cs` |

`Family` ki valid values (`StaticModels/ServiceKinds.cs`): `RECHARGE`, `DMT`, `BBPS`, `PAYOUT`,
`AEPS`, `MATM`, `CASHDEPOSIT`.

---

# Deploy recipe

```powershell
# 1. Publish — SELF-CONTAINED hona zaroori hai, warna HTTP 500.31 aata hai
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=false -p:DebugType=none -o <staging>

# 2. App pool band karo
C:\Windows\system32\inetsrv\appcmd.exe stop apppool /apppool.name:"api.finocore.net"

# 3. Copy — /E se, /MIR se KABHI nahi
robocopy <staging> C:\inetpub\wwwroot\api.finocore.net /E `
  /XF appsettings.Production.json fcm-service-account.json version.json `
  /XD wwwroot Logs

# 4. App pool chalu karo
C:\Windows\system32\inetsrv\appcmd.exe start apppool /apppool.name:"api.finocore.net"

# 5. Check — pehle ~10 second 503 aana normal hai
#    https://api.finocore.net/health  ->  {"status":"ok","database":"up"}
```

**`appsettings.Production.json` kabhi overwrite mat karna** — live connection string, keys aur
branding usi mein hai.
