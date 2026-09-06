import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react';
import { Link } from 'react-router-dom';
import {
  ArrowRight,
  BookText,
  Banknote,
  Check,
  ChevronRight,
  Copy,
  Fingerprint,
  Info,
  KeyRound,
  Landmark,
  Search,
  ShieldCheck,
  Smartphone,
  TriangleAlert,
  Webhook,
} from 'lucide-react';
import { apiBaseUrl } from '@/lib/apiClient';
import { useBrand } from '@/theme/BrandProvider';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import type { CustomScreenProps } from './registry';

/* ════════════════════════════════════════════════════════════════════════════
   API Documentation — the integrator-facing reference for the /api/v1 contract.

   Written as a developer portal rather than a console screen: sticky section
   nav, request samples in six languages that stay in sync across the page, and
   copy-to-clipboard on everything an integrator would otherwise retype.
   ════════════════════════════════════════════════════════════════════════════ */

/** Absolute base for the plaintext API, derived from the app's configured API URL. */
function apiV1(): string {
  const base = apiBaseUrl(); // '/api/' or an absolute 'https://host/api/'
  const root = /^https?:\/\//i.test(base) ? base.replace(/\/$/, '') : window.location.origin + base.replace(/\/$/, '');
  return `${root}/v1`;
}

/* ─── Primitives ─────────────────────────────────────────────────────────── */

/** Copy-to-clipboard with a two-second confirmation. */
function useCopy() {
  const [copied, setCopied] = useState(false);
  const timer = useRef<ReturnType<typeof setTimeout>>(undefined);

  useEffect(() => () => clearTimeout(timer.current), []);

  const copy = useCallback((text: string) => {
    void navigator.clipboard?.writeText(text).then(() => {
      setCopied(true);
      clearTimeout(timer.current);
      timer.current = setTimeout(() => setCopied(false), 2000);
    });
  }, []);

  return { copied, copy };
}

function CopyButton({ value, className, light }: { value: string; className?: string; light?: boolean }) {
  const { copied, copy } = useCopy();
  return (
    <button
      type="button"
      onClick={() => copy(value)}
      aria-label={copied ? 'Copied' : 'Copy to clipboard'}
      className={cn(
        'flex items-center gap-1.5 rounded-md px-2 py-1 text-[11px] font-medium transition-colors',
        light
          ? 'text-slate-400 hover:bg-white/10 hover:text-slate-100'
          : 'text-slate-400 hover:bg-slate-100 hover:text-slate-700',
        copied && (light ? 'text-emerald-300' : 'text-emerald-600'),
        className,
      )}
    >
      {copied ? <Check className="size-3.5" /> : <Copy className="size-3.5" />}
      {copied ? 'Copied' : 'Copy'}
    </button>
  );
}

/** HTTP method chip. GET reads, POST writes — coloured so the difference is scannable. */
function MethodChip({ method }: { method: 'GET' | 'POST' }) {
  return (
    <span
      className={cn(
        'rounded px-1.5 py-0.5 font-mono text-[10px] font-bold tracking-wide',
        method === 'GET' ? 'bg-emerald-50 text-emerald-700' : 'bg-brand-50 text-brand-700',
      )}
    >
      {method}
    </span>
  );
}

/** The endpoint header shown at the top of every endpoint section. */
function Endpoint({ path, v1 }: { path: string; v1: string }) {
  const full = `${v1}${path}`;
  return (
    <div className="flex flex-wrap items-center gap-2 rounded-xl border border-slate-200 bg-slate-50/70 px-3 py-2.5">
      <MethodChip method="GET" />
      <MethodChip method="POST" />
      <code className="min-w-0 flex-1 truncate font-mono text-[13px] font-medium text-slate-700">{full}</code>
      <CopyButton value={full} />
    </div>
  );
}

function Callout({
  tone = 'info',
  icon: Icon,
  title,
  children,
}: {
  tone?: 'info' | 'warn' | 'good';
  icon?: typeof Info;
  title?: string;
  children: ReactNode;
}) {
  const tones = {
    info: 'border-brand-200/80 bg-brand-50/60',
    warn: 'border-warning-500/30 bg-warning-50',
    good: 'border-success-500/25 bg-success-50',
  } as const;
  const iconTones = { info: 'text-brand-600', warn: 'text-warning-600', good: 'text-success-600' } as const;
  const Chosen = Icon ?? (tone === 'warn' ? TriangleAlert : Info);

  return (
    <div className={cn('flex gap-3 rounded-xl border px-4 py-3', tones[tone])}>
      <Chosen className={cn('mt-0.5 size-4 shrink-0', iconTones[tone])} />
      <div className="min-w-0 text-sm leading-relaxed text-slate-600">
        {title && <span className="font-semibold text-slate-800">{title} </span>}
        {children}
      </div>
    </div>
  );
}

/* ─── Code panels ────────────────────────────────────────────────────────── */

type Lang = 'curl' | 'node' | 'php' | 'python' | 'java' | 'csharp';

const LANGS: { id: Lang; label: string }[] = [
  { id: 'curl', label: 'cURL' },
  { id: 'node', label: 'Node.js' },
  { id: 'php', label: 'PHP' },
  { id: 'python', label: 'Python' },
  { id: 'java', label: 'Java' },
  { id: 'csharp', label: 'C#' },
];

/**
 * The chosen language is shared by every sample on the page: a PHP integrator picks
 * PHP once and reads the whole document in PHP.
 */
const LangContext = createContext<{ lang: Lang; setLang: (l: Lang) => void }>({
  lang: 'curl',
  setLang: () => {},
});

/** Dark code surface. `label` names the file or shell it belongs in. */
function CodeSurface({ label, code, tabs }: { label?: ReactNode; code: string; tabs?: ReactNode }) {
  return (
    <div className="overflow-hidden rounded-xl border border-ink-800 bg-ink-950 shadow-card">
      <div className="flex items-center gap-2 border-b border-white/10 bg-white/[0.03] pl-3 pr-1.5">
        {tabs ?? <span className="py-2 font-mono text-[11px] font-medium text-slate-400">{label}</span>}
        <div className="ml-auto shrink-0">
          <CopyButton value={code} light />
        </div>
      </div>
      <pre className="overflow-x-auto p-4 font-mono text-[12.5px] leading-[1.7] text-slate-200">
        <code>{code}</code>
      </pre>
    </div>
  );
}

/** A single, language-agnostic block (JSON payloads, HTTP traces). */
function Code({ label, children }: { label?: string; children: string }) {
  return <CodeSurface label={label ?? 'json'} code={children} />;
}

/** A block with one variant per language, driven by the page-wide selection. */
function CodeTabs({ samples }: { samples: Partial<Record<Lang, string>> }) {
  const { lang, setLang } = useContext(LangContext);
  const available = LANGS.filter((l) => samples[l.id]);
  const active = samples[lang] ? lang : available[0]?.id;
  const code = (active && samples[active]) || '';

  return (
    <CodeSurface
      code={code}
      tabs={
        <div className="no-scrollbar -mb-px flex items-stretch gap-0.5 overflow-x-auto">
          {available.map((l) => (
            <button
              key={l.id}
              type="button"
              onClick={() => setLang(l.id)}
              className={cn(
                'whitespace-nowrap border-b-2 px-2.5 py-2 text-[11.5px] font-medium transition-colors',
                l.id === active
                  ? 'border-brand-400 text-white'
                  : 'border-transparent text-slate-400 hover:text-slate-200',
              )}
            >
              {l.label}
            </button>
          ))}
        </div>
      }
    />
  );
}

/* ─── Sample generation ──────────────────────────────────────────────────── */

type Param = [name: string, value: string, comment?: string];

/** Renders the same request in six languages so nothing drifts between them. */
function requestSamples(url: string, params: Param[]): Record<Lang, string> {
  const pad = Math.max(...params.map(([k]) => k.length));
  const q = params.map(([k, v]) => `${k}=${v}`).join('&');

  return {
    curl:
      `curl -X POST "${url}" \\\n` +
      `  -H "Content-Type: application/x-www-form-urlencoded" \\\n` +
      params.map(([k, v]) => `  -d "${k}=${v}"`).join(' \\\n') +
      `\n\n# Every parameter also works as a GET query string:\n` +
      `curl "${url}?${q}"`,

    node:
      `const response = await fetch("${url}", {\n` +
      `  method: "POST",\n` +
      `  headers: { "Content-Type": "application/x-www-form-urlencoded" },\n` +
      `  body: new URLSearchParams({\n` +
      params.map(([k, v]) => `    ${k}:${' '.repeat(pad - k.length)} "${v}",`).join('\n') +
      `\n  }),\n});\n\n` +
      `const data = await response.json();\n\n` +
      `if (data.status === "SUCCESS") {\n` +
      `  // settle in your books\n` +
      `} else if (data.status === "PENDING") {\n` +
      `  // not final — wait for the callback\n` +
      `} else {\n` +
      `  console.error(data.errorcode, data.message);\n` +
      `}`,

    php:
      `<?php\n` +
      `$ch = curl_init("${url}");\n` +
      `curl_setopt_array($ch, [\n` +
      `    CURLOPT_RETURNTRANSFER => true,\n` +
      `    CURLOPT_POST           => true,\n` +
      `    CURLOPT_TIMEOUT        => 60,\n` +
      `    CURLOPT_POSTFIELDS     => http_build_query([\n` +
      params.map(([k, v]) => `        "${k}"${' '.repeat(pad - k.length)} => "${v}",`).join('\n') +
      `\n    ]),\n]);\n\n` +
      `$data = json_decode(curl_exec($ch), true);\n` +
      `curl_close($ch);\n\n` +
      `if ($data["status"] === "SUCCESS") {\n` +
      `    // settle in your books\n` +
      `}`,

    python:
      `import requests\n\n` +
      `response = requests.post(\n` +
      `    "${url}",\n` +
      `    data={\n` +
      params.map(([k, v]) => `        "${k}":${' '.repeat(pad - k.length)} "${v}",`).join('\n') +
      `\n    },\n` +
      `    timeout=60,\n` +
      `)\n\n` +
      `data = response.json()\n\n` +
      `if data["status"] == "SUCCESS":\n` +
      `    pass  # settle in your books\n` +
      `elif data["status"] == "PENDING":\n` +
      `    pass  # not final — wait for the callback`,

    java:
      `import java.net.URI;\n` +
      `import java.net.http.*;\n\n` +
      `String form = String.join("&",\n` +
      params.map(([k, v]) => `    "${k}=${v}"`).join(',\n') +
      `\n);\n\n` +
      `HttpRequest request = HttpRequest.newBuilder()\n` +
      `    .uri(URI.create("${url}"))\n` +
      `    .header("Content-Type", "application/x-www-form-urlencoded")\n` +
      `    .POST(HttpRequest.BodyPublishers.ofString(form))\n` +
      `    .build();\n\n` +
      `HttpResponse<String> response = HttpClient.newHttpClient()\n` +
      `    .send(request, HttpResponse.BodyHandlers.ofString());\n\n` +
      `String json = response.body();`,

    csharp:
      `using var client = new HttpClient();\n\n` +
      `var form = new FormUrlEncodedContent(new Dictionary<string, string>\n` +
      `{\n` +
      params.map(([k, v]) => `    ["${k}"]${' '.repeat(pad - k.length)} = "${v}",`).join('\n') +
      `\n});\n\n` +
      `var response = await client.PostAsync("${url}", form);\n` +
      `var json = await response.Content.ReadAsStringAsync();`,
  };
}

/* ─── Tables ─────────────────────────────────────────────────────────────── */

type ParamRow = { name: string; type: string; required: 'Required' | 'Optional' | 'Conditional'; desc: ReactNode };

function ParamTable({ rows }: { rows: ParamRow[] }) {
  return (
    <div className="overflow-hidden rounded-xl border border-slate-200">
      <table className="w-full border-collapse text-sm">
        <tbody>
          {rows.map((r, i) => (
            <tr key={r.name} className={cn(i > 0 && 'border-t border-slate-100')}>
              <td className="w-px whitespace-nowrap py-3 pl-4 pr-6 align-top">
                <div className="font-mono text-[12.5px] font-semibold text-slate-800">{r.name}</div>
                <div className="mt-1 flex items-center gap-1.5">
                  <span className="font-mono text-[10.5px] text-slate-400">{r.type}</span>
                  <span
                    className={cn(
                      'rounded px-1 py-px text-[9.5px] font-semibold uppercase tracking-wide',
                      r.required === 'Required'
                        ? 'bg-danger-50 text-danger-600'
                        : r.required === 'Conditional'
                          ? 'bg-warning-50 text-warning-600'
                          : 'bg-slate-100 text-slate-400',
                    )}
                  >
                    {r.required === 'Conditional' ? 'cond.' : r.required === 'Required' ? 'req.' : 'opt.'}
                  </span>
                </div>
              </td>
              <td className="py-3 pr-4 align-top text-[13px] leading-relaxed text-slate-600">{r.desc}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

/** Two/three-column reference table with an optional filter for the long code lists. */
function RefTable({
  head,
  rows,
  filterable,
  filterLabel,
}: {
  head: string[];
  rows: string[][];
  filterable?: boolean;
  filterLabel?: string;
}) {
  const [query, setQuery] = useState('');
  const shown = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return rows;
    return rows.filter((r) => r.some((c) => c.toLowerCase().includes(q)));
  }, [query, rows]);

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200">
      {filterable && (
        <div className="flex items-center gap-2 border-b border-slate-100 px-3 py-2">
          <Search className="size-3.5 shrink-0 text-slate-400" />
          <input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={filterLabel ?? 'Filter'}
            className="w-full bg-transparent py-1 text-sm text-slate-700 outline-none placeholder:text-slate-400"
          />
          <span className="shrink-0 text-[11px] tabular-nums text-slate-400">{shown.length}</span>
        </div>
      )}
      <div className="max-h-80 overflow-auto">
        <table className="w-full text-sm">
          <thead className="sticky top-0 z-10">
            <tr className="bg-slate-50 text-left text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
              {head.map((h) => (
                <th key={h} className="border-b border-slate-100 px-4 py-2.5">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {shown.map((r, i) => (
              <tr key={i} className="border-t border-slate-50 hover:bg-slate-50/60">
                <td className="whitespace-nowrap px-4 py-2 font-mono text-[12px] font-semibold text-brand-700">{r[0]}</td>
                <td className="px-4 py-2 text-slate-700">{r[1]}</td>
                {r[2] !== undefined && <td className="px-4 py-2 text-slate-400">{r[2]}</td>}
              </tr>
            ))}
            {shown.length === 0 && (
              <tr>
                <td colSpan={head.length} className="px-4 py-8 text-center text-sm text-slate-400">
                  Nothing matches “{query}”.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

/* ─── Section shell ──────────────────────────────────────────────────────── */

function Section({ id, n, title, children }: { id: string; n: number; title: string; children: ReactNode }) {
  return (
    <section id={id} className="scroll-mt-6 border-t border-slate-200/80 pt-10 first:border-0 first:pt-0">
      <div className="mb-4 flex items-center gap-2.5">
        <span className="font-mono text-[11px] font-semibold tabular-nums text-brand-500">
          {String(n).padStart(2, '0')}
        </span>
        <h2 className="font-display text-lg font-bold text-slate-900">{title}</h2>
      </div>
      <div className="space-y-4 text-[14px] leading-relaxed text-slate-600">{children}</div>
    </section>
  );
}

/** Inline code, for a field or value mentioned in prose. */
function C({ children }: { children: ReactNode }) {
  return (
    <code className="rounded bg-slate-100 px-1.5 py-0.5 font-mono text-[12px] text-slate-700">{children}</code>
  );
}

/* ─── Reference data ─────────────────────────────────────────────────────── */

const SECTIONS = [
  { id: 'overview', label: 'Overview' },
  { id: 'auth', label: 'Authentication' },
  { id: 'envelope', label: 'Response envelope' },
  { id: 'recharge', label: 'Recharge' },
  { id: 'status', label: 'Status check' },
  { id: 'balance', label: 'Balance' },
  { id: 'operators', label: 'Operators' },
  { id: 'callbacks', label: 'Callbacks' },
  { id: 'codes', label: 'Status & error codes' },
  { id: 'operator-codes', label: 'Operator codes' },
  { id: 'state-codes', label: 'State codes' },
  { id: 'checklist', label: 'Go-live checklist' },
];

const PRODUCTS = [
  { key: 'recharge', label: 'Recharge & Bill Payment', icon: Smartphone, available: true },
  { key: 'aeps', label: 'AEPS', icon: Fingerprint, available: false },
  { key: 'dmt', label: 'DMT / Money Transfer', icon: Landmark, available: false },
  { key: 'microatm', label: 'Micro ATM', icon: Banknote, available: false },
];

const STATUS_VALUES: string[][] = [
  ['SUCCESS', '0', 'Final. Consumer served, wallet debited.'],
  ['PENDING', '1', 'Not final. Accepted and debited; settles later.'],
  ['FAILED', '2', 'Final. Declined — amount refunded to your wallet.'],
  ['ERROR', '3 · 4 · 5 · 9', 'No transaction exists. Auth, validation or system failure.'],
];

const ERROR_CODES: string[][] = [
  ['AUTH_USERID_REQUIRED', 'userid missing, or not a positive number.'],
  ['AUTH_TOKEN_REQUIRED', 'token was not sent.'],
  ['AUTH_USER_NOT_FOUND', 'No such account.'],
  ['AUTH_NOT_API_USER', 'The account is not an API user.'],
  ['AUTH_ACCOUNT_INACTIVE', 'The account is inactive.'],
  ['AUTH_INVALID_TOKEN', 'Token mismatch. Regenerate it on API Access.'],
  ['AUTH_IP_NOT_WHITELISTED', 'IP Security is on and the calling IP is not listed.'],
  ['VAL_REQUESTID_REQUIRED', 'requestid missing (always on /recharge; on /status unless txnid is sent).'],
  ['VAL_NUMBER_REQUIRED', 'number was not sent.'],
  ['VAL_AMOUNT_INVALID', 'amount missing, not a number, or not greater than zero.'],
  ['VAL_OPERATOR_REQUIRED', 'operator was not sent.'],
  ['VAL_OPERATOR_INVALID', 'Unknown or inactive operator code. Call /operators.'],
  ['TXN_REJECTED', 'Declined before a transaction existed — balance, limit, PIN, operator off. Nothing charged.'],
  ['TXN_NOT_FOUND', '/status found no transaction for that requestid / txnid.'],
  ['SYS_ERROR', 'Unexpected server error. Retry with the SAME requestid — it is idempotent.'],
];

const OPERATORS: string[][] = [
  ['AIRT', 'Airtel', 'Prepaid Recharge'],
  ['JIO0', 'Reliance Jio', 'Prepaid Recharge'],
  ['VODA', 'Vodafone Idea (Vi)', 'Prepaid Recharge'],
  ['BSNL', 'BSNL', 'Prepaid Recharge'],
  ['AIRTEL', 'Airtel', 'Postpaid Bill Payment'],
  ['RELIAN', 'Reliance Jio', 'Postpaid Bill Payment'],
  ['ADTH', 'Airtel Digital TV', 'DTH Recharge'],
  ['TPLY', 'Tata Play', 'DTH Recharge'],
  ['DISH', 'Dish TV', 'DTH Recharge'],
  ['D2H', 'd2h', 'DTH Recharge'],
  ['SUNDIR', 'Sun Direct', 'DTH Recharge'],
  ['BESC', 'BESCOM', 'Electricity Bill Payment'],
  ['MSEB', 'MSEB', 'Electricity Bill Payment'],
  ['ACTFIB', 'ACT Fibernet', 'Broadband Bill Payment'],
  ['AIRTE1', 'Airtel Xstream Fiber', 'Broadband Bill Payment'],
  ['BSNLFT', 'BSNL FTTH', 'Broadband Bill Payment'],
];

const STATES: string[][] = [
  ['01', 'Jammu & Kashmir'], ['02', 'Himachal Pradesh'], ['03', 'Punjab'], ['04', 'Chandigarh'],
  ['05', 'Uttarakhand'], ['06', 'Haryana'], ['07', 'Delhi'], ['08', 'Rajasthan'], ['09', 'Uttar Pradesh'],
  ['10', 'Bihar'], ['11', 'Sikkim'], ['12', 'Arunachal Pradesh'], ['13', 'Nagaland'], ['14', 'Manipur'],
  ['15', 'Mizoram'], ['16', 'Tripura'], ['17', 'Meghalaya'], ['18', 'Assam'], ['19', 'West Bengal'],
  ['20', 'Jharkhand'], ['21', 'Odisha'], ['22', 'Chhattisgarh'], ['23', 'Madhya Pradesh'], ['24', 'Gujarat'],
  ['27', 'Maharashtra'], ['29', 'Karnataka'], ['30', 'Goa'], ['32', 'Kerala'], ['33', 'Tamil Nadu'],
  ['34', 'Puducherry'], ['36', 'Telangana'], ['37', 'Andhra Pradesh'], ['38', 'Ladakh'],
];

/** Verifying our callback signature, in the language the reader picked. */
const SIGNATURE_SAMPLES: Record<Lang, string> = {
  curl:
    `# Replay a callback against your own endpoint to test it end to end.\n` +
    `# Sign the body with your API token exactly as we do:\n\n` +
    `BODY='{"status":"SUCCESS","requestid":"REF001","txnid":10234}'\n` +
    `SIG=$(printf '%s' "$BODY" | openssl dgst -sha256 -hmac "YOUR_API_TOKEN" -hex | awk '{print $2}')\n\n` +
    `curl -X POST "https://your-server.com/callback" \\\n` +
    `  -H "Content-Type: application/json" \\\n` +
    `  -H "X-Signature: $SIG" \\\n` +
    `  -d "$BODY"`,

  node:
    `const crypto = require("crypto");\n\n` +
    `// Express: capture the RAW body — re-serialising the parsed object\n` +
    `// changes the bytes and the signature will never match.\n` +
    `app.post("/callback",\n` +
    `  express.raw({ type: "application/json" }),\n` +
    `  (req, res) => {\n` +
    `    const expected = crypto\n` +
    `      .createHmac("sha256", process.env.FINO_API_TOKEN)\n` +
    `      .update(req.body)\n` +
    `      .digest("hex");\n\n` +
    `    if (expected !== req.headers["x-signature"]) return res.sendStatus(401);\n\n` +
    `    const payload = JSON.parse(req.body.toString("utf8"));\n` +
    `    settle(payload.requestid, payload.status);   // must be idempotent\n` +
    `    res.sendStatus(200);\n` +
    `  });`,

  php:
    `<?php\n` +
    `// Read the RAW body — json_decode + re-encode would change the bytes.\n` +
    `$body = file_get_contents("php://input");\n` +
    `$expected = hash_hmac("sha256", $body, getenv("FINO_API_TOKEN"));\n\n` +
    `if (!hash_equals($expected, $_SERVER["HTTP_X_SIGNATURE"] ?? "")) {\n` +
    `    http_response_code(401);\n` +
    `    exit;\n` +
    `}\n\n` +
    `$payload = json_decode($body, true);\n` +
    `settle($payload["requestid"], $payload["status"]);   // must be idempotent\n\n` +
    `http_response_code(200);`,

  python:
    `import hmac, hashlib, os\n` +
    `from flask import Flask, request, abort\n\n` +
    `app = Flask(__name__)\n\n` +
    `@app.post("/callback")\n` +
    `def callback():\n` +
    `    body = request.get_data()          # RAW bytes, not request.json\n` +
    `    expected = hmac.new(\n` +
    `        os.environ["FINO_API_TOKEN"].encode(),\n` +
    `        body,\n` +
    `        hashlib.sha256,\n` +
    `    ).hexdigest()\n\n` +
    `    if not hmac.compare_digest(expected, request.headers.get("X-Signature", "")):\n` +
    `        abort(401)\n\n` +
    `    payload = request.get_json()\n` +
    `    settle(payload["requestid"], payload["status"])   # must be idempotent\n` +
    `    return "", 200`,

  java:
    `import javax.crypto.Mac;\n` +
    `import javax.crypto.spec.SecretKeySpec;\n` +
    `import java.nio.charset.StandardCharsets;\n` +
    `import java.util.HexFormat;\n\n` +
    `// rawBody must be the exact bytes received, before any JSON parsing.\n` +
    `Mac mac = Mac.getInstance("HmacSHA256");\n` +
    `mac.init(new SecretKeySpec(\n` +
    `    System.getenv("FINO_API_TOKEN").getBytes(StandardCharsets.UTF_8),\n` +
    `    "HmacSHA256"));\n\n` +
    `String expected = HexFormat.of().formatHex(mac.doFinal(rawBody));\n\n` +
    `if (!expected.equals(request.getHeader("X-Signature"))) {\n` +
    `    response.setStatus(401);\n` +
    `    return;\n` +
    `}`,

  csharp:
    `using System.Security.Cryptography;\n` +
    `using System.Text;\n\n` +
    `[HttpPost("/callback")]\n` +
    `public async Task<IActionResult> Callback()\n` +
    `{\n` +
    `    // Read the RAW body before model binding touches it.\n` +
    `    using var reader = new StreamReader(Request.Body);\n` +
    `    string body = await reader.ReadToEndAsync();\n\n` +
    `    using var hmac = new HMACSHA256(\n` +
    `        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("FINO_API_TOKEN")!));\n\n` +
    `    string expected = Convert.ToHexString(\n` +
    `        hmac.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();\n\n` +
    `    if (expected != Request.Headers["X-Signature"]) return Unauthorized();\n\n` +
    `    var payload = JsonSerializer.Deserialize<CallbackPayload>(body);\n` +
    `    Settle(payload!.RequestId, payload.Status);   // must be idempotent\n` +
    `    return Ok();\n` +
    `}`,
};

/* ─── The document ───────────────────────────────────────────────────────── */

function RechargeDoc({ appName, v1 }: { appName: string; v1: string }) {
  const rechargeSamples = useMemo(
    () =>
      requestSamples(`${v1}/recharge`, [
        ['userid', '14'],
        ['token', 'YOUR_API_TOKEN'],
        ['requestid', 'REF001'],
        ['operator', 'AIRT'],
        ['number', '9876543210'],
        ['amount', '199'],
      ]),
    [v1],
  );

  const statusSamples = useMemo(
    () =>
      requestSamples(`${v1}/status`, [
        ['userid', '14'],
        ['token', 'YOUR_API_TOKEN'],
        ['requestid', 'REF001'],
      ]),
    [v1],
  );

  const balanceSamples = useMemo(
    () =>
      requestSamples(`${v1}/balance`, [
        ['userid', '14'],
        ['token', 'YOUR_API_TOKEN'],
      ]),
    [v1],
  );

  const operatorSamples = useMemo(
    () =>
      requestSamples(`${v1}/operators`, [
        ['userid', '14'],
        ['token', 'YOUR_API_TOKEN'],
      ]),
    [v1],
  );

  return (
    <div className="space-y-10">
      {/* ── 01 Overview ───────────────────────────────────────────────── */}
      <Section id="overview" n={1} title="Overview">
        <p>
          Recharge and bill-payment integration for {appName}. A plain HTTP API — no encryption envelope, no login
          flow, no SDK. Send <C>userid</C> and <C>token</C> as ordinary parameters and receive JSON.
        </p>

        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          <div className="flex items-center gap-3 px-4 py-2.5">
            <span className="shrink-0 text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
              Base URL
            </span>
            <code className="min-w-0 flex-1 truncate font-mono text-[12.5px] text-slate-700">{v1}/</code>
            <CopyButton value={`${v1}/`} />
          </div>
          <div className="grid gap-px border-t border-slate-100 bg-slate-100 sm:grid-cols-2">
            {[
              { k: 'Auth', v: 'userid + token · optional IP allowlist' },
              { k: 'Contract', v: 'v1 · stable, additive changes only' },
            ].map((x) => (
              <div key={x.k} className="bg-white px-4 py-3">
                <div className="text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">{x.k}</div>
                <div className="mt-1 text-[13px] text-slate-700">{x.v}</div>
              </div>
            ))}
          </div>
        </div>

        <Callout tone="info" title="One response shape, everywhere.">
          The reply to <C>/recharge</C>, the reply to <C>/status</C> and the callback we push to you are the
          identical object. Only the <C>event</C> field differs. Write one parser and one settlement handler — that
          is the whole point of this contract.
        </Callout>

        <div className="rounded-xl border border-slate-200 bg-white p-5">
          <div className="mb-3 text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
            Integration in four steps
          </div>
          <ol className="space-y-2.5">
            {[
              <>
                Collect your <C>userid</C> and <C>token</C> from{' '}
                <Link to="/s/ApiAccess" className="font-medium text-brand-600 hover:underline">
                  API Access
                </Link>
                , and set a callback URL for each service.
              </>,
              <>
                Call <C>/recharge</C> with a <span className="font-medium text-slate-700">unique requestid</span> that
                you generate and store.
              </>,
              <>
                Read <C>status</C>. <C>SUCCESS</C> and <C>FAILED</C> are final; <C>PENDING</C> is not — wait for the
                callback.
              </>,
              <>
                Receive the callback, verify <C>X-Signature</C>, and settle idempotently. Use <C>/status</C> to
                reconcile anything you missed.
              </>,
            ].map((text, i) => (
              <li key={i} className="flex gap-3 text-[13.5px] leading-relaxed text-slate-600">
                <span className="mt-px grid size-5 shrink-0 place-items-center rounded-full bg-brand-50 font-mono text-[10px] font-bold text-brand-600">
                  {i + 1}
                </span>
                <span>{text}</span>
              </li>
            ))}
          </ol>
        </div>

        <Callout tone="warn" title="Never branch on the HTTP status.">
          Every endpoint replies <C>200 OK</C> — declines and errors included — so a simple HTTP client can never
          confuse a network failure with a business decline. Branch on <C>status</C> and <C>errorcode</C> in the body.
        </Callout>
      </Section>

      {/* ── 02 Authentication ─────────────────────────────────────────── */}
      <Section id="auth" n={2} title="Authentication">
        <p>
          Both parameters go on every request, to every endpoint. There are no sessions, headers or signatures on the
          request side.
        </p>
        <ParamTable
          rows={[
            { name: 'userid', type: 'integer', required: 'Required', desc: 'Your numeric account id.' },
            {
              name: 'token',
              type: 'string',
              required: 'Required',
              desc: (
                <>
                  Your API token. Regenerate it any time on{' '}
                  <Link to="/s/ApiAccess" className="font-medium text-brand-600 hover:underline">
                    API Access
                  </Link>{' '}
                  — the change is OTP-protected and takes effect immediately.
                </>
              ),
            },
          ]}
        />
        <div className="grid gap-3 sm:grid-cols-2">
          <Callout tone="good" icon={ShieldCheck} title="IP allowlist.">
            Turn on IP Security and list only your server IPs. A call from anywhere else is rejected with{' '}
            <C>AUTH_IP_NOT_WHITELISTED</C>, even with a valid token.
          </Callout>
          <Callout tone="warn" title="Keep the token server-side.">
            It authorises spending from your wallet. Never ship it in a mobile app or browser bundle. If it leaks,
            regenerate immediately — the old token stops working at once.
          </Callout>
        </div>
      </Section>

      {/* ── 03 Envelope ───────────────────────────────────────────────── */}
      <Section id="envelope" n={3} title="Response envelope">
        <p>
          Every reply — and every callback — opens with the same six fields. Parse them once; only the payload block
          below them changes per endpoint.
        </p>
        <Code label="every response">{`{
  "status":     "SUCCESS",   // SUCCESS | PENDING | FAILED | ERROR
  "statuscode": 0,           // numeric mirror of status
  "errorcode":  "",          // stable machine code, "" when there is no error
  "message":    "...",       // human text — display it, never parse it
  "requestid":  "REF001",    // your reference, echoed on EVERY reply

  ...                        // the endpoint's own payload

  "timestamp":  "2026-08-31T14:35:02+05:30",
  "version":    "1.0"        // contract version
}`}</Code>
        <ParamTable
          rows={[
            {
              name: 'status',
              type: 'string',
              required: 'Required',
              desc: 'The outcome. SUCCESS, PENDING and FAILED describe a transaction; ERROR means no transaction was created.',
            },
            { name: 'statuscode', type: 'integer', required: 'Required', desc: 'Numeric form of status, for stacks that prefer switching on an int.' },
            {
              name: 'errorcode',
              type: 'string',
              required: 'Required',
              desc: 'Stable, never reworded, never localised. This is the field to branch on.',
            },
            { name: 'message', type: 'string', required: 'Required', desc: 'Human-readable detail for your logs and support screens.' },
            { name: 'requestid', type: 'string', required: 'Required', desc: 'Your own reference, echoed back on every reply including errors.' },
            { name: 'version', type: 'string', required: 'Required', desc: 'Contract version. Fields are only ever added within a version.' },
          ]}
        />
      </Section>

      {/* ── 04 Recharge ───────────────────────────────────────────────── */}
      <Section id="recharge" n={4} title="Recharge">
        <p>Mobile, DTH and bill payments all run through this one endpoint; the operator code selects the service.</p>
        <Endpoint path="/recharge" v1={v1} />

        <ParamTable
          rows={[
            { name: 'userid', type: 'integer', required: 'Required', desc: 'Your account id.' },
            { name: 'token', type: 'string', required: 'Required', desc: 'Your API token.' },
            {
              name: 'requestid',
              type: 'string',
              required: 'Required',
              desc: 'Your own unique reference. This is the idempotency key — see the note below.',
            },
            { name: 'operator', type: 'string', required: 'Required', desc: 'Operator code, e.g. AIRT, JIO0, ADTH. See section 10.' },
            { name: 'number', type: 'string', required: 'Required', desc: 'Mobile number, DTH subscriber id, or consumer / account number.' },
            { name: 'amount', type: 'decimal', required: 'Required', desc: 'Transaction amount, greater than zero.' },
            { name: 'mobile', type: 'string', required: 'Optional', desc: 'Customer mobile for the confirmation SMS. Defaults to number.' },
            { name: 'pin', type: 'string', required: 'Optional', desc: 'Your mPIN, if PIN protection is enabled on the account.' },
            { name: 'param1 … param3', type: 'string', required: 'Conditional', desc: 'Extra biller fields, as listed by the bill-payment operator.' },
            { name: 'customername', type: 'string', required: 'Conditional', desc: 'Customer name, for bill-payment (BBPS) operators.' },
          ]}
        />

        <Callout tone="good" icon={ShieldCheck} title="Idempotent by requestid.">
          A <C>requestid</C> you have already used is never charged twice — we return the original transaction's
          current result with <C>"duplicate": true</C>. So a timeout or a dropped connection is safe to retry:{' '}
          <span className="font-medium text-slate-700">retry with the same requestid</span>, never a new one.
        </Callout>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Request</div>
          <CodeTabs samples={rechargeSamples} />
        </div>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Response · shared by /recharge, /status and callbacks
          </div>
          <Code label="200 OK">{`{
  "status":       "SUCCESS",
  "statuscode":   0,
  "errorcode":    "",
  "message":      "Recharge successful",
  "requestid":    "REF001",
  "event":        "INITIAL",             // INITIAL | STATUS | CALLBACK
  "final":        true,                  // false while the status can still change
  "duplicate":    false,                 // true when replaying a used requestid
  "txnid":        10234,                 // our transaction id
  "providerref":  "OP123456789",         // operator reference
  "service":      "Prepaid Recharge",
  "serviceid":    4,
  "operator":     "Airtel",
  "operatorcode": "AIRT",
  "number":       "9876543210",
  "amount":       199.00,
  "debitamount":  197.50,                // what actually left your wallet
  "balance":      4820.50,               // your balance after this transaction
  "date":         "2026-08-31 14:35:00",
  "timestamp":    "2026-08-31T14:35:02+05:30",
  "version":      "1.0"
}`}</Code>
        </div>

        <ParamTable
          rows={[
            { name: 'event', type: 'string', required: 'Required', desc: 'Which touch-point produced this payload: INITIAL, STATUS or CALLBACK.' },
            { name: 'final', type: 'boolean', required: 'Required', desc: 'true once the status can no longer change. Settle your books only on final: true.' },
            { name: 'duplicate', type: 'boolean', required: 'Required', desc: 'true when this requestid was already used — the wallet was not debited again.' },
            { name: 'txnid', type: 'integer', required: 'Required', desc: 'Our transaction id. 0 when no transaction was created.' },
            { name: 'providerref', type: 'string', required: 'Required', desc: 'Operator / aggregator reference, when the upstream returned one.' },
            { name: 'debitamount', type: 'decimal', required: 'Required', desc: 'amount + surcharge − discount. Reconcile your wallet against this, not amount.' },
            { name: 'balance', type: 'decimal', required: 'Required', desc: 'Your wallet balance after the transaction settled.' },
            { name: 'date', type: 'string', required: 'Required', desc: 'Transaction time, always yyyy-MM-dd HH:mm:ss (IST).' },
          ]}
        />

        <div className="overflow-hidden rounded-xl border border-slate-200">
          {[
            ['SUCCESS', 'Completed. Consumer served, wallet debited.', 'text-success-600 bg-success-50'],
            ['PENDING', 'Accepted, awaiting operator confirmation. Final status arrives by callback.', 'text-warning-600 bg-warning-50'],
            ['FAILED', 'Rejected — the amount is refunded to your wallet.', 'text-danger-600 bg-danger-50'],
          ].map(([code, desc, tone], i) => (
            <div key={code} className={cn('flex items-start gap-3 px-4 py-3', i > 0 && 'border-t border-slate-100')}>
              <span className={cn('rounded px-1.5 py-0.5 font-mono text-[10.5px] font-bold', tone)}>{code}</span>
              <span className="text-[13px] text-slate-600">{desc}</span>
            </div>
          ))}
        </div>
      </Section>

      {/* ── 05 Status ─────────────────────────────────────────────────── */}
      <Section id="status" n={5} title="Status check">
        <p>
          The current state of a transaction. Returns the <span className="font-medium text-slate-700">exact payload
          of section 4</span>, with <C>"event": "STATUS"</C>.
        </p>
        <Endpoint path="/status" v1={v1} />
        <ParamTable
          rows={[
            { name: 'userid', type: 'integer', required: 'Required', desc: 'Your account id.' },
            { name: 'token', type: 'string', required: 'Required', desc: 'Your API token.' },
            {
              name: 'requestid',
              type: 'string',
              required: 'Conditional',
              desc: 'The requestid you sent on /recharge. Required unless you send txnid.',
            },
            { name: 'txnid', type: 'integer', required: 'Conditional', desc: 'Our transaction id, if you stored it. Used when requestid is absent.' },
          ]}
        />
        <CodeTabs samples={statusSamples} />
        <p>
          If the transaction is still <C>PENDING</C> we re-enquire with the operator before answering, so the reply
          reflects the operator and not just our record. When that enquiry settles the transaction, the callback fires
          as well.
        </p>
        <Callout tone="info" title="Use the callback, not a polling loop.">
          Reserve <C>/status</C> for reconciliation and for the case where your callback endpoint was down. An enquiry
          on a transaction less than a minute old is answered from our record without troubling the operator.
        </Callout>
      </Section>

      {/* ── 06 Balance ────────────────────────────────────────────────── */}
      <Section id="balance" n={6} title="Balance">
        <p>Your wallet balances, with a pre-computed total so a pre-flight check needs one field.</p>
        <Endpoint path="/balance" v1={v1} />
        <CodeTabs samples={balanceSamples} />
        <Code label="200 OK">{`{
  "status":     "SUCCESS",
  "statuscode": 0,
  "errorcode":  "",
  "message":    "Balance fetched successfully.",
  "requestid":  "",
  "balance":    4830.50,                 // total AVAILABLE across all wallets
  "wallets": [
    { "walletid": 1, "wallet": "Recharge Wallet", "balance": 4820.50, "hold": 0.00, "available": 4820.50 },
    { "walletid": 2, "wallet": "AEPS Wallet",     "balance": 10.00,   "hold": 0.00, "available": 10.00 }
  ],
  "timestamp":  "2026-08-31T14:35:02+05:30",
  "version":    "1.0"
}`}</Code>
        <ParamTable
          rows={[
            { name: 'balance', type: 'decimal', required: 'Required', desc: 'Sum of every wallet’s available amount. Spendable right now.' },
            { name: 'wallets[].walletid', type: 'integer', required: 'Required', desc: 'Stable across renames — key your mapping on this, not the name.' },
            { name: 'wallets[].hold', type: 'decimal', required: 'Required', desc: 'Frozen portion that cannot be spent.' },
            { name: 'wallets[].available', type: 'decimal', required: 'Required', desc: 'balance − hold. This is what transactions draw on.' },
          ]}
        />
      </Section>

      {/* ── 07 Operators ──────────────────────────────────────────────── */}
      <Section id="operators" n={7} title="Operators">
        <p>
          The live operator catalogue. Cache it and refresh daily rather than calling it before every transaction.
        </p>
        <Endpoint path="/operators" v1={v1} />
        <CodeTabs samples={operatorSamples} />
        <Code label="200 OK">{`{
  "status":     "SUCCESS",
  "statuscode": 0,
  "errorcode":  "",
  "message":    "Operator list fetched successfully.",
  "requestid":  "",
  "count":      107,
  "operators": [
    { "code": "AIRT", "name": "Airtel",            "serviceid": 4, "service": "Prepaid Recharge" },
    { "code": "ADTH", "name": "Airtel Digital TV", "serviceid": 5, "service": "DTH Recharge" }
  ],
  "timestamp":  "2026-08-31T14:35:02+05:30",
  "version":    "1.0"
}`}</Code>
      </Section>

      {/* ── 08 Callbacks ──────────────────────────────────────────────── */}
      <Section id="callbacks" n={8} title="Callbacks">
        <p>
          Set a callback URL per service on{' '}
          <Link to="/s/ApiAccess" className="font-medium text-brand-600 hover:underline">
            API Access
          </Link>{' '}
          — just your base URL, e.g. <C>https://your-server.com/callback</C>. When a transaction reaches a final
          status, {appName} POSTs the section-4 payload to it with <C>"event": "CALLBACK"</C>.
        </p>

        <Code label="what we send you">{`POST https://your-server.com/callback
Content-Type: application/json
X-Event: CALLBACK
X-Request-Id: REF001
X-Transaction-Id: 10234
X-Attempt: 1/3
X-Signature-Algorithm: HMAC-SHA256
X-Signature: 9f2b8c1d...c41e

{
  "status": "SUCCESS", "statuscode": 0, "errorcode": "", "message": "Recharge successful",
  "requestid": "REF001", "event": "CALLBACK", "final": true, "duplicate": false,
  "txnid": 10234, "providerref": "OP123456789", "service": "Prepaid Recharge", "serviceid": 4,
  "operator": "Airtel", "operatorcode": "AIRT", "number": "9876543210",
  "amount": 199.00, "debitamount": 197.50, "balance": 4820.50,
  "date": "2026-08-31 14:35:00", "timestamp": "2026-08-31T14:35:02+05:30", "version": "1.0"
}`}</Code>

        <RefTable
          head={['Header', 'Meaning']}
          rows={[
            ['X-Event', 'Always CALLBACK.'],
            ['X-Request-Id', 'The requestid you sent — match the callback to your record on this.'],
            ['X-Transaction-Id', 'Our transaction id.'],
            ['X-Attempt', 'Delivery attempt, e.g. 2/3.'],
            ['X-Signature', 'HMAC-SHA256 of the raw body, keyed with your API token, lowercase hex.'],
          ]}
        />

        <div className="pt-1">
          <div className="mb-2 flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            <Webhook className="size-3.5" />
            Verify the signature
          </div>
          <CodeTabs samples={SIGNATURE_SAMPLES} />
        </div>

        <div className="grid gap-3 sm:grid-cols-2">
          <Callout tone="warn" title="Sign the raw bytes.">
            Parsing the JSON and re-serialising it changes whitespace and key order, and the signature will never
            match. Capture the body before your framework touches it.
          </Callout>
          <Callout tone="info" title="Retries and ordering.">
            A non-200 or a timeout is retried up to three times with a widening gap, so handle callbacks{' '}
            <span className="font-medium text-slate-700">idempotently</span>. A callback can also arrive{' '}
            <span className="font-medium text-slate-700">before</span> you finish handling the <C>/recharge</C> reply
            — key both on <C>requestid</C> and let whichever lands first win.
          </Callout>
        </div>

        <p className="text-[12.5px] text-slate-400">
          Integrating an older system built against the legacy query-string GET callback? It is still supported — ask
          us to switch your deployment's callback mode; nothing changes on your side.
        </p>
      </Section>

      {/* ── 09 Codes ──────────────────────────────────────────────────── */}
      <Section id="codes" n={9} title="Status & error codes">
        <div className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-slate-400">status · statuscode</div>
        <RefTable head={['status', 'statuscode', 'Meaning']} rows={STATUS_VALUES} />
        <div className="mb-1 pt-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">errorcode</div>
        <p>
          Stable and never reworded — branch on these, not on <C>message</C>.
        </p>
        <RefTable head={['errorcode', 'Cause']} rows={ERROR_CODES} filterable filterLabel="Filter error codes" />
      </Section>

      {/* ── 10 Operator codes ─────────────────────────────────────────── */}
      <Section id="operator-codes" n={10} title="Operator codes">
        <p>
          The most common codes. Call <C>{`${v1}/operators`}</C> for the complete, always-current list.
        </p>
        <RefTable
          head={['Code', 'Operator', 'Service']}
          rows={OPERATORS}
          filterable
          filterLabel="Filter by code, operator or service"
        />
      </Section>

      {/* ── 11 State codes ────────────────────────────────────────────── */}
      <Section id="state-codes" n={11} title="State codes">
        <p>Standard two-digit state codes, used where a biller requires a state.</p>
        <RefTable head={['Code', 'State']} rows={STATES} filterable filterLabel="Filter states" />
      </Section>

      {/* ── 12 Checklist ──────────────────────────────────────────────── */}
      <Section id="checklist" n={12} title="Go-live checklist">
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          {[
            ['Unique requestid per transaction', 'Generate and store it. It is your key for callbacks, status checks and reconciliation.'],
            ['Retry with the same requestid', 'On a timeout or network error. Never generate a new one for a retry — that is how double charges happen.'],
            ['Settle only on final: true', 'PENDING is not an outcome. Treat it as in-flight.'],
            ['Callback endpoint live and idempotent', 'Reachable from the public internet, returns 200 quickly, and tolerates a repeat delivery.'],
            ['X-Signature verified on the raw body', 'Reject anything that does not match. This is what proves the callback came from us.'],
            ['Token server-side only', 'Never in a mobile app or browser bundle. Regenerate immediately if exposed.'],
            ['IP Security enabled', 'Whitelist only your production server IPs.'],
            ['errorcode handled, not message', 'Messages are for humans; codes are the contract.'],
          ].map(([title, desc], i) => (
            <div key={title} className={cn('flex gap-3 px-4 py-3', i > 0 && 'border-t border-slate-100')}>
              <span className="mt-0.5 grid size-4 shrink-0 place-items-center rounded-full border border-slate-300">
                <Check className="size-2.5 text-slate-300" />
              </span>
              <div className="min-w-0">
                <div className="text-[13.5px] font-medium text-slate-800">{title}</div>
                <div className="mt-0.5 text-[12.5px] leading-relaxed text-slate-500">{desc}</div>
              </div>
            </div>
          ))}
        </div>
      </Section>
    </div>
  );
}

/* ─── Screen ─────────────────────────────────────────────────────────────── */

export function ApiDocsScreen(_props: CustomScreenProps) {
  const { appName } = useBrand();
  const v1 = apiV1();
  const [product, setProduct] = useState('recharge');
  const [lang, setLang] = useState<Lang>('curl');
  const [active, setActive] = useState(SECTIONS[0].id);
  const current = PRODUCTS.find((d) => d.key === product) ?? PRODUCTS[0];

  // Highlight the section the reader is actually looking at. Only a narrow band near the
  // top of the viewport counts as "reading here". The observer reports CHANGES, not the
  // full picture, so visibility is accumulated in a set — otherwise scrolling past several
  // sections at once leaves the highlight on whichever one last fired an event.
  useEffect(() => {
    if (product !== 'recharge') return;

    const visible = new Set<string>();
    const observer = new IntersectionObserver(
      (entries) => {
        for (const e of entries) {
          if (e.isIntersecting) visible.add(e.target.id);
          else visible.delete(e.target.id);
        }
        const first = SECTIONS.find((s) => visible.has(s.id));
        if (first) setActive(first.id);
      },
      { rootMargin: '-8% 0px -75% 0px', threshold: 0 },
    );

    SECTIONS.forEach((s) => {
      const el = document.getElementById(s.id);
      if (el) observer.observe(el);
    });
    return () => observer.disconnect();
  }, [product]);

  const langValue = useMemo(() => ({ lang, setLang }), [lang]);

  return (
    <LangContext.Provider value={langValue}>
      <PageHeader
        title="API Documentation"
        subtitle={`Integrate ${appName} services into your own application`}
        icon={BookText}
        actions={
          <Link to="/s/ApiAccess">
            <Button variant="outline">
              <KeyRound className="size-4" /> API Access
            </Button>
          </Link>
        }
      />

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-[236px_minmax(0,1fr)]">
        {/* ── Sidebar ─────────────────────────────────────────────────── */}
        <aside className="h-max lg:sticky lg:top-4">
          <nav className="space-y-0.5 rounded-card border border-slate-200/70 bg-white p-2 shadow-card">
            {PRODUCTS.map((d) => (
              <button
                key={d.key}
                type="button"
                disabled={!d.available}
                onClick={() => d.available && setProduct(d.key)}
                className={cn(
                  'flex w-full items-center justify-between gap-2 rounded-lg px-3 py-2 text-left text-[13px] transition-colors',
                  product === d.key ? 'bg-brand-50 font-semibold text-brand-700' : 'text-slate-600 hover:bg-slate-50',
                  !d.available && 'cursor-not-allowed opacity-45 hover:bg-transparent',
                )}
              >
                <span className="flex min-w-0 items-center gap-2.5">
                  <d.icon className={cn('size-4 shrink-0', product === d.key ? 'text-brand-500' : 'text-slate-400')} />
                  <span className="truncate">{d.label}</span>
                </span>
                {!d.available && (
                  <span className="shrink-0 rounded bg-slate-100 px-1.5 py-0.5 text-[9.5px] font-semibold uppercase tracking-wide text-slate-400">
                    Soon
                  </span>
                )}
              </button>
            ))}
          </nav>

          {product === 'recharge' && (
            <div className="mt-4 hidden lg:block">
              <div className="px-3 pb-2 text-[10px] font-semibold uppercase tracking-wider text-slate-400">
                On this page
              </div>
              <nav className="space-y-px border-l border-slate-200">
                {SECTIONS.map((s) => (
                  <a
                    key={s.id}
                    href={`#${s.id}`}
                    className={cn(
                      '-ml-px flex items-center gap-2 border-l-2 py-1.5 pl-3 pr-2 text-[12.5px] transition-colors',
                      active === s.id
                        ? 'border-brand-500 font-medium text-brand-700'
                        : 'border-transparent text-slate-500 hover:border-slate-300 hover:text-slate-700',
                    )}
                  >
                    {s.label}
                  </a>
                ))}
              </nav>

              <Link
                to="/s/ApiAccess"
                className="mt-4 flex items-center justify-between gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-[12.5px] text-slate-600 transition-colors hover:border-brand-200 hover:text-brand-700"
              >
                <span className="flex items-center gap-2">
                  <KeyRound className="size-3.5 text-slate-400" />
                  Get your credentials
                </span>
                <ArrowRight className="size-3.5 shrink-0" />
              </Link>
            </div>
          )}
        </aside>

        {/* ── Content ─────────────────────────────────────────────────── */}
        <div className="min-w-0">
          <div className="rounded-card border border-slate-200/70 bg-white p-6 shadow-card sm:p-9">
            {/* Document header */}
            <header className="mb-9 border-b border-slate-200/80 pb-7">
              <div className="mb-2 flex flex-wrap items-center gap-2">
                <span className="rounded-md bg-brand-50 px-2 py-0.5 font-mono text-[10.5px] font-bold tracking-wide text-brand-700">
                  v1
                </span>
                <span className="flex items-center gap-1 text-[11px] text-slate-400">
                  <ChevronRight className="size-3" />
                  REST · JSON · no SDK required
                </span>
              </div>
              <h1 className="font-display text-2xl font-bold text-slate-900">{current.label}</h1>
              {product === 'recharge' && (
                <p className="mt-2 max-w-2xl text-[14px] leading-relaxed text-slate-500">
                  One request, one response shape, one callback. Everything below is the published contract — fields
                  are added within a version, never renamed or removed.
                </p>
              )}
            </header>

            {product === 'recharge' ? (
              <RechargeDoc appName={appName} v1={v1} />
            ) : (
              <div className="py-16 text-center">
                <current.icon className="mx-auto size-8 text-slate-300" />
                <p className="mt-3 text-sm text-slate-400">{current.label} documentation is coming soon.</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </LangContext.Provider>
  );
}
