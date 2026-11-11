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
  Lock,
  Search,
  Send,
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

/**
 * The same SIGNED request in six languages.
 *
 * DMT endpoints are POST-only with a JSON body and an HMAC signature, so a sample has to
 * show the whole ritual rather than a bare form post: build the body once, hash exactly
 * those bytes, sign method + path + timestamp + nonce + hash, then send the same string
 * that was hashed. Re-serialising the body before sending is the one mistake that breaks
 * every signature, so each sample deliberately builds it into a variable first.
 */
function signedSamples(base: string, path: string, body: Param[]): Record<Lang, string> {
  const pad = Math.max(...body.map(([k]) => k.length));
  const jsonBody = (indent: string) =>
    `{${body.map(([k, v]) => `"${k}":"${v}"`).join(',')}}`.length < 72
      ? `{${body.map(([k, v]) => `"${k}":"${v}"`).join(',')}}`
      : '{\n' + body.map(([k, v]) => `${indent}  "${k}": "${v}"`).join(',\n') + `\n${indent}}`;

  const pairs = (indent: string, sep: string, q: string) =>
    body.map(([k, v]) => `${indent}${q}${k}${q}${' '.repeat(pad - k.length)}${sep} "${v}"`).join(',\n');

  return {
    curl:
      `TOKEN="YOUR_API_TOKEN"\n` +
      `PATH_="${path}"\n\n` +
      `# 1. Build the body once. These exact bytes are hashed AND sent.\n` +
      `BODY='${jsonBody('')}'\n\n` +
      `# 2. Freshness: epoch SECONDS, and a nonce you never reuse.\n` +
      `TS=$(date +%s)\n` +
      `NONCE=$(uuidgen)\n\n` +
      `# 3. Sign.\n` +
      `BODY_SHA=$(printf '%s' "$BODY" | openssl dgst -sha256 -hex | sed 's/.*= //')\n` +
      `SIG=$(printf 'POST\\n%s\\n%s\\n%s\\n%s' "$PATH_" "$TS" "$NONCE" "$BODY_SHA" \\\n` +
      `  | openssl dgst -sha256 -hmac "$TOKEN" -hex | sed 's/.*= //')\n\n` +
      `curl -X POST "${base}${path}" \\\n` +
      `  -H "Content-Type: application/json" \\\n` +
      `  -H "X-Api-Mobile: 9000012345" \\\n` +
      `  -H "X-Api-Token: $TOKEN" \\\n` +
      `  -H "X-Api-Timestamp: $TS" \\\n` +
      `  -H "X-Api-Nonce: $NONCE" \\\n` +
      `  -H "X-Api-Signature: $SIG" \\\n` +
      `  --data-binary "$BODY"   # byte-exact: the bytes hashed are the bytes sent`,

    node:
      `import crypto from "node:crypto";\n\n` +
      `const TOKEN = process.env.API_TOKEN;\n` +
      `const PATH  = "${path}";\n\n` +
      `// Build the body ONCE — this exact string is both hashed and sent.\n` +
      `const body = JSON.stringify({\n` +
      pairs('  ', ':', '') +
      `,\n});\n\n` +
      `const ts    = Math.floor(Date.now() / 1000).toString();  // SECONDS, not ms\n` +
      `const nonce = crypto.randomUUID();\n\n` +
      `const bodyHash  = crypto.createHash("sha256").update(body, "utf8").digest("hex");\n` +
      `const canonical = ["POST", PATH, ts, nonce, bodyHash].join("\\n");\n` +
      `const signature = crypto.createHmac("sha256", TOKEN).update(canonical, "utf8").digest("hex");\n\n` +
      `const response = await fetch("${base}${path}", {\n` +
      `  method: "POST",\n` +
      `  headers: {\n` +
      `    "Content-Type":    "application/json",\n` +
      `    "X-Api-Mobile":    "9000012345",\n` +
      `    "X-Api-Token":     TOKEN,\n` +
      `    "X-Api-Timestamp": ts,\n` +
      `    "X-Api-Nonce":     nonce,\n` +
      `    "X-Api-Signature": signature,\n` +
      `  },\n` +
      `  body,  // the same string that was hashed — never re-serialise\n` +
      `});\n\n` +
      `const data = await response.json();`,

    php:
      `<?php\n` +
      `$token = getenv("API_TOKEN");\n` +
      `$path  = "${path}";\n\n` +
      `// Build the body once — hashed and sent as the same bytes.\n` +
      `$body = json_encode([\n` +
      pairs('    ', ' =>', '"') +
      `,\n]);\n\n` +
      `$ts    = (string) time();                  // SECONDS\n` +
      `$nonce = bin2hex(random_bytes(16));\n\n` +
      `$canonical = "POST\\n" . $path . "\\n" . $ts . "\\n" . $nonce . "\\n" . hash("sha256", $body);\n` +
      `$signature = hash_hmac("sha256", $canonical, $token);\n\n` +
      `$ch = curl_init("${base}${path}");\n` +
      `curl_setopt_array($ch, [\n` +
      `    CURLOPT_RETURNTRANSFER => true,\n` +
      `    CURLOPT_POST           => true,\n` +
      `    CURLOPT_TIMEOUT        => 60,\n` +
      `    CURLOPT_POSTFIELDS     => $body,\n` +
      `    CURLOPT_HTTPHEADER     => [\n` +
      `        "Content-Type: application/json",\n` +
      `        "X-Api-Mobile: 9000012345",\n` +
      `        "X-Api-Token: $token",\n` +
      `        "X-Api-Timestamp: $ts",\n` +
      `        "X-Api-Nonce: $nonce",\n` +
      `        "X-Api-Signature: $signature",\n` +
      `    ],\n]);\n\n` +
      `$data = json_decode(curl_exec($ch), true);\n` +
      `curl_close($ch);`,

    python:
      `import hashlib, hmac, json, os, time, uuid, requests\n\n` +
      `token = os.environ["API_TOKEN"]\n` +
      `path  = "${path}"\n\n` +
      `# Build the body once; hash and send the identical bytes.\n` +
      `body = json.dumps({\n` +
      pairs('    ', ':', '"') +
      `,\n})\n\n` +
      `ts    = str(int(time.time()))          # SECONDS\n` +
      `nonce = str(uuid.uuid4())\n\n` +
      `body_hash = hashlib.sha256(body.encode()).hexdigest()\n` +
      `canonical = "\\n".join(["POST", path, ts, nonce, body_hash])\n` +
      `signature = hmac.new(token.encode(), canonical.encode(), hashlib.sha256).hexdigest()\n\n` +
      `response = requests.post(\n` +
      `    "${base}${path}",\n` +
      `    data=body,                          # the signed string itself\n` +
      `    headers={\n` +
      `        "Content-Type":    "application/json",\n` +
      `        "X-Api-Mobile":    "9000012345",\n` +
      `        "X-Api-Token":     token,\n` +
      `        "X-Api-Timestamp": ts,\n` +
      `        "X-Api-Nonce":     nonce,\n` +
      `        "X-Api-Signature": signature,\n` +
      `    },\n` +
      `    timeout=60,\n` +
      `)\n\n` +
      `data = response.json()`,

    java:
      `import java.net.URI;\n` +
      `import java.net.http.*;\n` +
      `import java.security.MessageDigest;\n` +
      `import java.time.Instant;\n` +
      `import java.util.*;\n` +
      `import javax.crypto.Mac;\n` +
      `import javax.crypto.spec.SecretKeySpec;\n\n` +
      `String token = System.getenv("API_TOKEN");\n` +
      `String path  = "${path}";\n\n` +
      `String body  = """\n` +
      `    ${jsonBody('    ')}""";\n\n` +
      `String ts    = Long.toString(Instant.now().getEpochSecond());  // SECONDS\n` +
      `String nonce = UUID.randomUUID().toString();\n\n` +
      `String bodyHash = hex(MessageDigest.getInstance("SHA-256").digest(body.getBytes("UTF-8")));\n` +
      `String canonical = String.join("\\n", "POST", path, ts, nonce, bodyHash);\n\n` +
      `Mac mac = Mac.getInstance("HmacSHA256");\n` +
      `mac.init(new SecretKeySpec(token.getBytes("UTF-8"), "HmacSHA256"));\n` +
      `String signature = hex(mac.doFinal(canonical.getBytes("UTF-8")));\n\n` +
      `HttpRequest request = HttpRequest.newBuilder()\n` +
      `    .uri(URI.create("${base}${path}"))\n` +
      `    .header("Content-Type",    "application/json")\n` +
      `    .header("X-Api-Mobile",    "9000012345")\n` +
      `    .header("X-Api-Token",     token)\n` +
      `    .header("X-Api-Timestamp", ts)\n` +
      `    .header("X-Api-Nonce",     nonce)\n` +
      `    .header("X-Api-Signature", signature)\n` +
      `    .POST(HttpRequest.BodyPublishers.ofString(body))\n` +
      `    .build();\n\n` +
      `// static String hex(byte[] b) { StringBuilder s = new StringBuilder();\n` +
      `//   for (byte x : b) s.append(String.format("%02x", x)); return s.toString(); }`,

    csharp:
      `using System.Security.Cryptography;\n` +
      `using System.Text;\n\n` +
      `var token = Environment.GetEnvironmentVariable("API_TOKEN")!;\n` +
      `var path  = "${path}";\n\n` +
      `// Build the body once; the same string is hashed and sent.\n` +
      `var body  = System.Text.Json.JsonSerializer.Serialize(new\n` +
      `{\n` +
      body.map(([k, v]) => `    ${k}${' '.repeat(pad - k.length)} = "${v}",`).join('\n') +
      `\n});\n\n` +
      `var ts    = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();  // SECONDS\n` +
      `var nonce = Guid.NewGuid().ToString();\n\n` +
      `var bodyHash  = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();\n` +
      `var canonical = string.Join("\\n", "POST", path, ts, nonce, bodyHash);\n\n` +
      `using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(token));\n` +
      `var signature  = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();\n\n` +
      `using var client = new HttpClient();\n` +
      `var content = new StringContent(body, Encoding.UTF8, "application/json");\n` +
      `content.Headers.Add("X-Api-Mobile",    "9000012345");\n` +
      `content.Headers.Add("X-Api-Token",     token);\n` +
      `content.Headers.Add("X-Api-Timestamp", ts);\n` +
      `content.Headers.Add("X-Api-Nonce",     nonce);\n` +
      `content.Headers.Add("X-Api-Signature", signature);\n\n` +
      `var response = await client.PostAsync("${base}${path}", content);\n` +
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

/**
 * DMT has its own table of contents. It is a multi-step flow — a remitter is onboarded
 * and verified, a beneficiary is attached and verified, and only then does money move —
 * so the document is ordered the way an integrator actually builds it, and signing gets a
 * section of its own because it applies to every call.
 */
const DMT_SECTIONS = [
  { id: 'dmt-overview', label: 'Overview' },
  { id: 'dmt-auth', label: 'Authentication' },
  { id: 'dmt-signing', label: 'Request signing' },
  { id: 'dmt-envelope', label: 'Response envelope' },
  { id: 'dmt-sender', label: 'Sender registration' },
  { id: 'dmt-beneficiary', label: 'Beneficiary' },
  { id: 'dmt-transfer', label: 'Transfer' },
  { id: 'dmt-status', label: 'Status check' },
  { id: 'dmt-balance', label: 'Balance' },
  { id: 'dmt-reference', label: 'Banks & IFSC' },
  { id: 'dmt-callbacks', label: 'Callbacks' },
  { id: 'dmt-codes', label: 'Status & error codes' },
  { id: 'dmt-checklist', label: 'Go-live checklist' },
];

const PAYOUT_SECTIONS = [
  { id: 'payout-overview', label: 'Overview' },
  { id: 'payout-auth', label: 'Authentication & signing' },
  { id: 'payout-transfer', label: 'Transfer' },
  { id: 'payout-status', label: 'Status check' },
  { id: 'payout-balance', label: 'Balance' },
  { id: 'payout-ifsc', label: 'IFSC lookup' },
  { id: 'payout-callbacks', label: 'Callbacks' },
  { id: 'payout-codes', label: 'Status & error codes' },
  { id: 'payout-checklist', label: 'Go-live checklist' },
];

const PRODUCTS = [
  { key: 'recharge', label: 'Recharge & Bill Payment', icon: Smartphone, available: true },
  { key: 'aeps', label: 'AEPS', icon: Fingerprint, available: false },
  { key: 'dmt', label: 'DMT / Money Transfer', icon: Landmark, available: true },
  { key: 'payout', label: 'Payout / Bank Transfer', icon: Send, available: true },
  { key: 'microatm', label: 'Micro ATM', icon: Banknote, available: false },
];

/** The table of contents for whichever product is being read. */
function sectionsFor(product: string) {
  if (product === 'dmt') return DMT_SECTIONS;
  if (product === 'payout') return PAYOUT_SECTIONS;
  return SECTIONS;
}

const STATUS_VALUES: string[][] = [
  ['SUCCESS', '0', 'Final. Consumer served, wallet debited.'],
  ['PENDING', '1', 'Not final. Accepted and debited; settles later.'],
  ['FAILED', '2', 'Final. Declined — amount refunded to your wallet.'],
  ['ERROR', '3 · 4 · 5 · 9', 'No transaction exists. Auth, validation or system failure.'],
];

const ERROR_CODES: string[][] = [
  ['AUTH_MOBILE_REQUIRED', 'mobile missing, or not the account’s registered 10-digit number.'],
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
        ['mobile', '9000012345'],
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
        ['mobile', '9000012345'],
        ['token', 'YOUR_API_TOKEN'],
        ['requestid', 'REF001'],
      ]),
    [v1],
  );

  const balanceSamples = useMemo(
    () =>
      requestSamples(`${v1}/balance`, [
        ['mobile', '9000012345'],
        ['token', 'YOUR_API_TOKEN'],
      ]),
    [v1],
  );

  const operatorSamples = useMemo(
    () =>
      requestSamples(`${v1}/operators`, [
        ['mobile', '9000012345'],
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
          flow, no SDK. Send <C>mobile</C> and <C>token</C> as ordinary parameters and receive JSON.
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
              { k: 'Auth', v: 'mobile + token · optional IP allowlist' },
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
                Collect your <C>mobile</C> and <C>token</C> from{' '}
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
            { name: 'mobile', type: 'string', required: 'Required', desc: 'Your account’s registered 10-digit mobile number.' },
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
            { name: 'mobile', type: 'string', required: 'Required', desc: 'Your registered mobile number.' },
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
            { name: 'mobile', type: 'string', required: 'Required', desc: 'Your registered mobile number.' },
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

/* ─── DMT document ───────────────────────────────────────────────────────── */

/**
 * A DMT endpoint header. Unlike the recharge one this shows a SINGLE method, because the
 * distinction is load-bearing here: everything that moves money or reads customer data is
 * POST, and a GET to it is refused rather than honoured.
 */
function DmtEndpoint({ path, v1, method = 'POST' }: { path: string; v1: string; method?: 'GET' | 'POST' }) {
  const full = `${v1}${path}`;
  return (
    <div className="flex flex-wrap items-center gap-2 rounded-xl border border-slate-200 bg-slate-50/70 px-3 py-2.5">
      <MethodChip method={method} />
      <code className="min-w-0 flex-1 truncate font-mono text-[13px] font-medium text-slate-700">{full}</code>
      <CopyButton value={full} />
    </div>
  );
}

/** One beneficiary/sender step in the onboarding sequence. */
function FlowStep({ n, title, children }: { n: number; title: string; children: ReactNode }) {
  return (
    <li className="flex gap-3">
      <span className="mt-px grid size-5 shrink-0 place-items-center rounded-full bg-brand-50 font-mono text-[10px] font-bold text-brand-600">
        {n}
      </span>
      <span className="text-[13.5px] leading-relaxed text-slate-600">
        <span className="font-medium text-slate-700">{title}</span> — {children}
      </span>
    </li>
  );
}

const DMT_STATUS_VALUES: string[][] = [
  ['SUCCESS', '0', 'Final. Money reached the beneficiary; a UTR is present.'],
  ['PENDING', '1', 'Not final. Accepted and debited; settles later.'],
  ['FAILED', '2', 'Final. Declined — the amount is refunded to your wallet.'],
  ['ERROR', '3 · 4 · 5 · 9', 'No transaction was created. Nothing was debited.'],
];

const DMT_SECURITY_CODES: string[][] = [
  ['SEC_SIGNATURE_REQUIRED', 'Send the three signing headers.'],
  ['SEC_SIGNATURE_INVALID', 'Re-check the canonical string; hash the bytes you actually send.'],
  ['SEC_TIMESTAMP_INVALID', 'Use epoch seconds, not milliseconds. Sync your clock (NTP).'],
  ['SEC_NONCE_INVALID', 'Generate a fresh nonce for every request.'],
  ['SEC_METHOD_NOT_ALLOWED', 'Use POST. Only /dmt/balance accepts GET.'],
];

const DMT_FLOW_CODES: string[][] = [
  ['VAL_REQUESTID_REQUIRED', 'Send a unique requestid on /dmt/transfer.'],
  ['VAL_SENDER_MOBILE_REQUIRED', 'sendermobile must be exactly 10 digits.'],
  ['VAL_SENDER_NAME_REQUIRED', 'sendername is missing.'],
  ['VAL_BENEFICIARY_REQUIRED', 'beneficiaryid / beneficiaryname is missing.'],
  ['VAL_ACCOUNT_REQUIRED', 'accountno is missing.'],
  ['VAL_IFSC_REQUIRED', 'ifsc is missing.'],
  ['VAL_IFSC_INVALID', 'Must be 11 characters, e.g. HDFC0000123.'],
  ['VAL_PAYMENT_TYPE_INVALID', 'paymenttype must be IMPS or NEFT.'],
  ['VAL_LOCATION_INVALID', 'Send latitude and longitude together, and as real coordinates.'],
  ['VAL_OTP_REQUIRED', 'otp is missing.'],
  ['VAL_AMOUNT_INVALID', 'amount must be a number greater than zero.'],
  ['VAL_AMOUNT_BELOW_LIMIT', 'Below the per-transfer floor.'],
  ['VAL_AMOUNT_ABOVE_LIMIT', 'Above the per-transfer ceiling, or past the sender’s monthly headroom.'],
  ['DMT_SENDER_NOT_FOUND', 'Register the sender first.'],
  ['DMT_SENDER_NOT_VERIFIED', 'Clear the sender OTP.'],
  ['DMT_BENEFICIARY_NOT_FOUND', 'Unknown id, or not registered under this sender.'],
  ['DMT_BENEFICIARY_NOT_VERIFIED', 'Clear the beneficiary OTP.'],
  ['DMT_REJECTED', 'The flow declined the step — message carries the reason.'],
  ['TXN_REJECTED', 'Declined before a transaction existed, usually balance or a limit.'],
  ['TXN_NOT_FOUND', 'No transfer for that requestid / txnid.'],
  ['SYS_ERROR', 'Our fault. Do not re-send — call /dmt/status with the same requestid.'],
];

function DmtDoc({ appName, v1 }: { appName: string; v1: string }) {
  const transferSamples = useMemo(
    () =>
      signedSamples(v1.replace(/\/v1$/, ''), '/api/v1/dmt/transfer', [
        ['requestid', 'DMT-1001'],
        ['sendermobile', '9876543210'],
        ['beneficiaryid', 'b4f1c2a0-7e21-4a19-9f3d-2c8e5d6a1b77'],
        ['amount', '5000'],
        ['paymenttype', 'IMPS'],
        ['latitude', '22.7196'],
        ['longitude', '75.8577'],
      ]),
    [v1],
  );

  const senderSamples = useMemo(
    () =>
      signedSamples(v1.replace(/\/v1$/, ''), '/api/v1/dmt/sender/register', [
        ['sendermobile', '9876543210'],
        ['sendername', 'Ramesh Kumar'],
        ['pincode', '452001'],
      ]),
    [v1],
  );

  const beneSamples = useMemo(
    () =>
      signedSamples(v1.replace(/\/v1$/, ''), '/api/v1/dmt/beneficiary/register', [
        ['sendermobile', '9876543210'],
        ['beneficiaryname', 'Sunita Devi'],
        ['accountno', '50100234567890'],
        ['ifsc', 'HDFC0000123'],
      ]),
    [v1],
  );

  const statusSamples = useMemo(
    () =>
      signedSamples(v1.replace(/\/v1$/, ''), '/api/v1/dmt/status', [['requestid', 'DMT-1001']]),
    [v1],
  );

  return (
    <div className="space-y-10">
      {/* ── 01 Overview ───────────────────────────────────────────────── */}
      <Section id="dmt-overview" n={1} title="Overview">
        <p>
          Domestic Money Transfer for {appName}. Send money from a walk-in customer to any Indian bank account over
          IMPS or NEFT. Plain HTTP and JSON — no SDK — but unlike the recharge API, every request is{' '}
          <span className="font-medium text-slate-700">POST-only and signed</span>, because DMT tickets are large
          enough that a leaked or replayed request is a material loss.
        </p>

        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          <div className="flex items-center gap-3 px-4 py-2.5">
            <span className="shrink-0 text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
              Base URL
            </span>
            <code className="min-w-0 flex-1 truncate font-mono text-[12.5px] text-slate-700">{v1}/dmt/</code>
            <CopyButton value={`${v1}/dmt/`} />
          </div>
          <div className="grid gap-px border-t border-slate-100 bg-slate-100 sm:grid-cols-2">
            {[
              { k: 'Auth', v: 'mobile + token + HMAC signature · optional IP allowlist' },
              { k: 'Methods', v: 'POST everywhere · GET only for /dmt/balance' },
              { k: 'Rails', v: 'IMPS · NEFT' },
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
          The reply to <C>/dmt/transfer</C>, the reply to <C>/dmt/status</C> and the callback we push are the
          identical object — only <C>event</C> differs. It also extends the recharge transaction shape, so if you
          already consume <C>/recharge</C> the same parser works here; DMT only adds a beneficiary block.
        </Callout>

        <div className="rounded-xl border border-slate-200 bg-white p-5">
          <div className="mb-3 text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
            The flow — steps 1–4 are once per customer, step 5 repeats
          </div>
          <ol className="space-y-2.5">
            <FlowStep n={1} title="Register the sender">
              <C>/dmt/sender/register</C> with the customer’s mobile and name. Replies <C>otprequired: true</C> when
              an OTP has gone to them.
            </FlowStep>
            <FlowStep n={2} title="Verify the sender">
              <C>/dmt/sender/otp/verify</C>. Until this clears, the sender holds no beneficiaries and sends no money.
            </FlowStep>
            <FlowStep n={3} title="Attach a beneficiary">
              <C>/dmt/beneficiary/register</C> returns a <C>beneficiaryid</C> — store it against the customer.
            </FlowStep>
            <FlowStep n={4} title="Verify the beneficiary">
              <C>/dmt/beneficiary/otp/verify</C>. An unverified beneficiary cannot receive a transfer.
            </FlowStep>
            <FlowStep n={5} title="Send, then settle">
              <C>/dmt/transfer</C> with a fresh <C>requestid</C>, then wait for the callback or poll{' '}
              <C>/dmt/status</C> until <C>final: true</C>.
            </FlowStep>
          </ol>
        </div>

        <Callout tone="warn" title="Never retry a transfer with a new requestid.">
          If <C>/dmt/transfer</C> times out, the transfer may already be in flight. Resolve it by calling{' '}
          <C>/dmt/status</C> with the <span className="font-medium text-slate-700">same</span> requestid. A fresh
          requestid creates a second, independent transfer — and the money leaves twice.
        </Callout>

        <Callout tone="info" title="Two rails run this contract. Ask which one you are on.">
          <span className="font-medium text-slate-700">Pure DMT</span> is the five-step flow above: the remitter
          and each beneficiary are registered and OTP-verified before money moves.{' '}
          <span className="font-medium text-slate-700">Payout DMT</span> is the same contract with steps 1–4
          removed — that rail keeps no remitter, so nothing is pre-registered. On a payout platform:
          <ul className="mt-2 space-y-1 pl-4">
            <li>
              • <C>beneficiaryid</C> can be replaced by <C>accountno</C> + <C>ifsc</C> +{' '}
              <C>beneficiaryname</C> sent inline. One call, nothing to register or look up.{' '}
              <C>sendermobile</C> stays required, but the number does not have to be registered —
              send the customer’s.
            </li>
            <li>
              • The registration endpoints still work and still return a <C>beneficiaryid</C>, so an integration
              written for pure DMT keeps running unchanged — they simply reply <C>otprequired: false</C>, and the
              OTP-verify calls succeed without an OTP.
            </li>
            <li>
              • An inline destination on a <span className="font-medium text-slate-700">pure</span> platform is
              refused, not ignored — that rail can only send where it has registered.
            </li>
          </ul>
        </Callout>
      </Section>

      {/* ── 02 Authentication ─────────────────────────────────────────── */}
      <Section id="dmt-auth" n={2} title="Authentication">
        <p>
          Your <C>mobile</C> and <C>token</C> go on every request. Send them as headers — that keeps them out of
          bodies you might log wholesale, and out of the URL on the one GET endpoint.
        </p>
        <ParamTable
          rows={[
            { name: 'X-Api-Mobile', type: 'header', required: 'Required', desc: 'Your account’s registered 10-digit mobile number.' },
            {
              name: 'X-Api-Token',
              type: 'header',
              required: 'Required',
              desc: (
                <>
                  Your API token from{' '}
                  <Link to="/s/ApiAccess" className="font-medium text-brand-600 hover:underline">
                    API Access
                  </Link>
                  . It is also the HMAC signing key.
                </>
              ),
            },
            { name: 'X-Api-Timestamp', type: 'header', required: 'Required', desc: 'Unix epoch SECONDS, UTC.' },
            {
              name: 'X-Api-Nonce',
              type: 'header',
              required: 'Required',
              desc: 'Unique per request, 8–128 characters. A UUID is a good choice.',
            },
            {
              name: 'X-Api-Signature',
              type: 'header',
              required: 'Required',
              desc: 'Lowercase hex HMAC-SHA256 over the canonical string — see the next section.',
            },
          ]}
        />
        <Callout tone="info" title="No custom headers in your stack?">
          All five may instead be sent as body fields named <C>mobile</C>, <C>token</C>, <C>timestamp</C>,{' '}
          <C>nonce</C> and <C>signature</C>. Headers win where both are present, and the signature is computed the
          same way either way.
        </Callout>
        <div className="grid gap-3 sm:grid-cols-2">
          <Callout tone="good" icon={ShieldCheck} title="IP allowlist.">
            Turn on IP Security and list only your server IPs. A call from anywhere else is rejected with{' '}
            <C>AUTH_IP_NOT_WHITELISTED</C>, even with a valid token and a valid signature.
          </Callout>
          <Callout tone="warn" title="Keep the token server-side.">
            It authorises spending from your wallet and signs your requests. Never ship it in a mobile app or browser
            bundle. If it leaks, regenerate immediately.
          </Callout>
        </div>
      </Section>

      {/* ── 03 Request signing ────────────────────────────────────────── */}
      <Section id="dmt-signing" n={3} title="Request signing">
        <p>
          A token on its own is a bearer credential: anything that can read one request can replay it. So each DMT
          request must also <span className="font-medium text-slate-700">prove</span> possession of the token. You
          build a canonical string, HMAC it with your token, and send the result.
        </p>

        <div className="grid gap-3 sm:grid-cols-3">
          {[
            { t: 'Tamper-evident', d: 'Change one rupee of the body and the signature no longer matches.' },
            { t: 'Endpoint-bound', d: 'A captured /beneficiary/list call cannot be re-pointed at /transfer.' },
            { t: 'Replay-resistant', d: 'The timestamp ages out and the nonce is single-use.' },
          ].map((x) => (
            <div key={x.t} className="rounded-xl border border-slate-200 bg-white p-4">
              <div className="flex items-center gap-2">
                <Lock className="size-3.5 shrink-0 text-brand-500" />
                <span className="text-[12.5px] font-semibold text-slate-800">{x.t}</span>
              </div>
              <p className="mt-1.5 text-[12.5px] leading-relaxed text-slate-500">{x.d}</p>
            </div>
          ))}
        </div>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            The canonical string — five lines joined by \n
          </div>
          <Code label="canonical">{`POST                                  ← HTTP method, uppercase
/api/v1/dmt/transfer                  ← path, lowercase, no query string
1757923200                            ← timestamp, exactly as sent
9f1c4a2e-6b8d-4f30-a1e7-5c9d2b3a8e64  ← nonce, exactly as sent
583280e9d8432e1c2840…                 ← SHA-256 of the RAW body, lowercase hex

signature = HMAC-SHA256(canonical, your_api_token)  →  lowercase hex`}</Code>
        </div>

        <Callout tone="warn" title="Hash the bytes you actually send.">
          Line 5 is a SHA-256 of your request body exactly as it goes on the wire — not of a re-serialised copy. You
          never have to match our key order, spacing or number formatting. Build the JSON string once, hash{' '}
          <span className="font-medium text-slate-700">that</span> string, and send the same string. Re-serialising
          before sending is the single most common cause of <C>SEC_SIGNATURE_INVALID</C>.
        </Callout>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Worked example · reproduce this before you go live
          </div>
          <Code label="verify your implementation">{`Token (signing key)   TESTTOKEN123456

Body, sent verbatim
{"mobile":"9000012345","requestid":"ORD-77001","sendermobile":"9876543210","beneficiaryid":"b4f1c2a0-7e21-4a19-9f3d-2c8e5d6a1b77","amount":"5000","paymenttype":"IMPS"}

SHA-256 of that body
583280e9d8432e1c2840853bcf29657ef94cb98a085afd42a738978598724944

Canonical string (\\n shown escaped)
POST\\n/api/v1/dmt/transfer\\n1757923200\\n9f1c4a2e-6b8d-4f30-a1e7-5c9d2b3a8e64\\n583280e9d8432e1c2840853bcf29657ef94cb98a085afd42a738978598724944

X-Api-Signature
19ca0fbfdf6cbe9363cc855aa85bdbd9bafbea407fc2150f401fa31d753876e6`}</Code>
        </div>

        <Callout tone="good" icon={ShieldCheck} title="Signing GET /dmt/balance.">
          There is no body, so line 5 is the SHA-256 of the empty string:{' '}
          <C>e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855</C>. Everything else is unchanged.
        </Callout>

        <ParamTable
          rows={[
            {
              name: 'Clock window',
              type: 'rule',
              required: 'Required',
              desc: '±300 seconds from server time. Run NTP — a drifted clock fails every request.',
            },
            {
              name: 'Units',
              type: 'rule',
              required: 'Required',
              desc: 'Epoch SECONDS. Sending milliseconds is the most common integration error — it reads as ~55,000 years in the future.',
            },
            {
              name: 'Nonce reuse',
              type: 'rule',
              required: 'Required',
              desc: 'Single-use inside the replay window. Generate one per request, never per session.',
            },
            {
              name: 'Retries',
              type: 'rule',
              required: 'Required',
              desc: 'A retry needs a NEW timestamp, nonce and signature — but the SAME requestid. That combination is what makes a retry safe instead of a second transfer.',
            },
          ]}
        />
      </Section>

      {/* ── 04 Envelope ───────────────────────────────────────────────── */}
      <Section id="dmt-envelope" n={4} title="Response envelope">
        <p>
          Every reply — and every callback — opens with the same six fields. Parse them once; only the payload block
          below them changes per endpoint. Every endpoint replies <C>200 OK</C>, declines included.
        </p>
        <Code label="every response">{`{
  "status":     "PENDING",   // SUCCESS | PENDING | FAILED | ERROR
  "statuscode": 1,           // numeric mirror of status
  "errorcode":  "",          // stable machine code, "" when there is no error
  "message":    "...",       // human text — display it, never parse it
  "requestid":  "DMT-1001",  // your reference, echoed on EVERY reply

  ...                        // the endpoint's own payload

  "timestamp":  "2026-09-15T14:20:11+05:30",
  "version":    "1.0"
}`}</Code>
        <RefTable head={['status', 'code', 'Meaning']} rows={DMT_STATUS_VALUES} />
        <Callout tone="warn" title="Never branch on the HTTP status.">
          A bad signature, a declined transfer and a successful one all return <C>200 OK</C>. Branch on{' '}
          <C>status</C> and <C>errorcode</C> in the body.
        </Callout>
      </Section>

      {/* ── 05 Sender ─────────────────────────────────────────────────── */}
      <Section id="dmt-sender" n={5} title="Sender registration">
        <p>
          The sender is the walk-in customer remitting the money, identified by their mobile number. Register them
          once, verify once.
        </p>

        <DmtEndpoint path="/dmt/sender/register" v1={v1} />
        <ParamTable
          rows={[
            { name: 'sendermobile', type: 'string', required: 'Required', desc: 'Exactly 10 digits. This is the sender’s identity.' },
            { name: 'sendername', type: 'string', required: 'Required', desc: 'Name as per the customer’s KYC.' },
            { name: 'address', type: 'string', required: 'Optional', desc: 'Customer address.' },
            { name: 'pincode', type: 'string', required: 'Optional', desc: '6-digit postal code.' },
          ]}
        />

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Request</div>
          <CodeTabs samples={senderSamples} />
        </div>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Response</div>
          <Code label="200 OK">{`{
  "status":      "SUCCESS",
  "statuscode":  0,
  "errorcode":   "",
  "message":     "Sender registered. An OTP has been sent to the sender's mobile — verify it to activate.",
  "otprequired": true,
  "reference":   ""
}`}</Code>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <DmtEndpoint path="/dmt/sender/otp/verify" v1={v1} />
            <p className="mt-2 text-[13px] text-slate-500">
              Takes <C>sendermobile</C> and <C>otp</C>. Activates the sender.
            </p>
          </div>
          <div>
            <DmtEndpoint path="/dmt/sender/otp/resend" v1={v1} />
            <p className="mt-2 text-[13px] text-slate-500">
              Takes <C>sendermobile</C>. Sends a fresh OTP.
            </p>
          </div>
        </div>

        <DmtEndpoint path="/dmt/sender/fetch" v1={v1} />
        <p>
          The sender’s profile and — the reason to call it before quoting a price — how much they may still send
          this month.
        </p>
        <Code label="200 OK">{`{
  "status":           "SUCCESS",
  "statuscode":       0,
  "sendermobile":     "9876543210",
  "sendername":       "Ramesh Kumar",
  "address":          "12 MG Road, Indore",
  "verified":         true,
  "consumedlimit":    18000.00,   // already sent this calendar month
  "remaininglimit":   7000.00,    // what is still available
  "pertransferlimit": 5000.00     // 0 means no extra per-transfer cap
}`}</Code>
        <Callout tone="info" title="The limit follows the customer, not your outlet.">
          <C>consumedlimit</C> counts every transfer that sender made across the platform this calendar month, so the
          same customer cannot be spread across two retailers to exceed the cap. Check{' '}
          <C>remaininglimit</C> before you quote.
        </Callout>
      </Section>

      {/* ── 06 Beneficiary ────────────────────────────────────────────── */}
      <Section id="dmt-beneficiary" n={6} title="Beneficiary">
        <p>A beneficiary is a destination bank account attached to one verified sender.</p>

        <DmtEndpoint path="/dmt/beneficiary/register" v1={v1} />
        <ParamTable
          rows={[
            { name: 'sendermobile', type: 'string', required: 'Required', desc: 'Must be a verified sender.' },
            { name: 'beneficiaryname', type: 'string', required: 'Required', desc: 'Account holder’s name.' },
            { name: 'accountno', type: 'string', required: 'Required', desc: 'Destination account number.' },
            { name: 'ifsc', type: 'string', required: 'Required', desc: '11 characters, e.g. HDFC0000123.' },
            { name: 'bankname', type: 'string', required: 'Optional', desc: 'Resolved from the IFSC when omitted.' },
          ]}
        />

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Request</div>
          <CodeTabs samples={beneSamples} />
        </div>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Response</div>
          <Code label="200 OK">{`{
  "status":      "SUCCESS",
  "statuscode":  0,
  "message":     "Verify OTP to activate beneficiary.",
  "otprequired": true,
  "beneficiary": {
    "beneficiaryid":   "b4f1c2a0-7e21-4a19-9f3d-2c8e5d6a1b77",
    "beneficiaryname": "Sunita Devi",
    "accountno":       "50100234567890",
    "ifsc":            "HDFC0000123",
    "bankname":        "HDFC Bank",
    "verified":        false,
    "nameverified":    false
  }
}`}</Code>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <DmtEndpoint path="/dmt/beneficiary/otp/verify" v1={v1} />
            <p className="mt-2 text-[13px] text-slate-500">
              Takes <C>beneficiaryid</C> and <C>otp</C>.
            </p>
          </div>
          <div>
            <DmtEndpoint path="/dmt/beneficiary/otp/resend" v1={v1} />
            <p className="mt-2 text-[13px] text-slate-500">
              Takes <C>beneficiaryid</C>.
            </p>
          </div>
          <div>
            <DmtEndpoint path="/dmt/beneficiary/list" v1={v1} />
            <p className="mt-2 text-[13px] text-slate-500">
              Takes <C>sendermobile</C>. Returns <C>count</C> and a <C>beneficiaries</C> array.
            </p>
          </div>
          <div>
            <DmtEndpoint path="/dmt/beneficiary/delete" v1={v1} />
            <p className="mt-2 text-[13px] text-slate-500">
              Takes <C>beneficiaryid</C>. Past transfers stay queryable.
            </p>
          </div>
        </div>

        <DmtEndpoint path="/dmt/beneficiary/verify" v1={v1} />
        <p>
          Penny-drop name enquiry: asks the destination bank who actually holds the account, so your customer can
          confirm before sending a large amount to a mistyped digit. Takes <C>sendermobile</C>, <C>accountno</C>,{' '}
          <C>ifsc</C> and optionally <C>bankname</C>.
        </p>
        <Code label="200 OK">{`{
  "status":       "SUCCESS",
  "verifiedname": "SUNITA DEVI",   // the name the BANK holds
  "bankname":     "HDFC Bank",
  "charge":       4.00,
  "balance":      18420.50
}`}</Code>
        <Callout tone="warn" title="This call costs money.">
          A fee is debited from your DMT wallet for the enquiry, including when the bank declines to confirm a name
          (<C>status: "FAILED"</C>, with the bank’s reason in <C>message</C>). Call it deliberately — never on every
          page load.
        </Callout>
      </Section>

      {/* ── 07 Transfer ───────────────────────────────────────────────── */}
      <Section id="dmt-transfer" n={7} title="Transfer">
        <p>The money-moving call. Both the sender and the beneficiary must already be verified.</p>
        <DmtEndpoint path="/dmt/transfer" v1={v1} />

        <ParamTable
          rows={[
            {
              name: 'requestid',
              type: 'string',
              required: 'Required',
              desc: 'Your own unique reference. This is the idempotency key — see the note below.',
            },
            {
              name: 'sendermobile',
              type: 'string',
              required: 'Required',
              desc: 'Customer’s 10-digit mobile. On pure DMT it must be a verified sender; on a payout-DMT platform any valid number is accepted, but it is still required.',
            },
            {
              name: 'beneficiaryid',
              type: 'string',
              required: 'Required',
              desc: 'Verified beneficiary, registered under this sender. On a payout-DMT platform you may instead send accountno + ifsc + beneficiaryname inline and skip registration.',
            },
            {
              name: 'accountno · ifsc · beneficiaryname',
              type: 'string',
              required: 'Conditional',
              desc: 'The destination named inline, in place of beneficiaryid. Accepted only on a payout-DMT platform; a pure DMT platform refuses them.',
            },
            { name: 'amount', type: 'decimal', required: 'Required', desc: 'Rupees. Default bounds ₹100 – ₹25,000 per transfer.' },
            { name: 'paymenttype', type: 'string', required: 'Optional', desc: 'IMPS (default) or NEFT.' },
            { name: 'remark', type: 'string', required: 'Optional', desc: 'Narration, where the bank carries it.' },
            { name: 'latitude / longitude', type: 'string', required: 'Optional', desc: 'Outlet coordinates, stored against the transaction for audit. Send both or neither; each is validated as a real coordinate.' },
          ]}
        />

        <Callout tone="good" icon={ShieldCheck} title="Idempotent by requestid.">
          A <C>requestid</C> you have already used is never debited twice — we return the original transfer’s current
          result with <C>"duplicate": true</C>. So a timeout is safe to resolve:{' '}
          <span className="font-medium text-slate-700">retry with the same requestid</span>, never a new one.
        </Callout>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Request</div>
          <CodeTabs samples={transferSamples} />
        </div>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Response · shared by /dmt/transfer, /dmt/status and callbacks
          </div>
          <Code label="200 OK">{`{
  "status":       "PENDING",
  "statuscode":   1,
  "errorcode":    "",
  "message":      "Transfer initiated.",
  "requestid":    "DMT-1001",
  "event":        "INITIAL",     // INITIAL | STATUS | CALLBACK
  "final":        false,         // true once status can no longer change
  "duplicate":    false,         // true when replaying a used requestid
  "txnid":        884512,        // our transaction id
  "providerref":  "IMP1234567",
  "amount":       5000.00,
  "debitamount":  5015.00,       // principal + charges
  "balance":      13405.50,      // your DMT wallet afterwards
  "date":         "2026-09-15 14:20:11",

  "sendermobile":    "9876543210",
  "beneficiaryid":   "b4f1c2a0-7e21-4a19-9f3d-2c8e5d6a1b77",
  "beneficiaryname": "Sunita Devi",
  "accountno":       "50100234567890",
  "ifsc":            "HDFC0000123",
  "bankname":        "HDFC Bank",
  "paymenttype":     "IMPS",
  "utr":             "",          // bank UTR — populated once SUCCESS
  "charge":          15.00
}`}</Code>
        </div>

        <Callout tone="info" title="utr is the number that settles disputes.">
          It is the reference the beneficiary’s own bank recognises, and it appears only once the transfer reaches{' '}
          <C>SUCCESS</C>. While <C>PENDING</C> it is deliberately empty — quoting <C>providerref</C> to a customer’s
          bank will not help them.
        </Callout>

        <div className="rounded-xl border border-slate-200 bg-white p-5">
          <div className="mb-3 text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
            What is checked before anything is debited
          </div>
          <RefTable
            head={['Check', 'On failure']}
            rows={[
              ['Credentials, account active, IP allowed', 'AUTH_*'],
              ['Signature, timestamp, nonce', 'SEC_*'],
              ['requestid present', 'VAL_REQUESTID_REQUIRED'],
              ['requestid already used', 'Original result, duplicate: true'],
              ['Amount within bounds', 'VAL_AMOUNT_BELOW_LIMIT / VAL_AMOUNT_ABOVE_LIMIT'],
              ['Sender exists, is yours, is verified', 'DMT_SENDER_NOT_FOUND / DMT_SENDER_NOT_VERIFIED'],
              ['Beneficiary is yours and belongs to this sender', 'DMT_BENEFICIARY_NOT_FOUND'],
              ['Beneficiary verified', 'DMT_BENEFICIARY_NOT_VERIFIED'],
              ['Sender’s monthly headroom covers the amount', 'VAL_AMOUNT_ABOVE_LIMIT'],
              ['Wallet covers principal + charges', 'TXN_REJECTED'],
            ]}
          />
          <p className="mt-3 text-[13px] leading-relaxed text-slate-500">
            All of it happens before a rupee moves. Any failure returns <C>ERROR</C> or <C>FAILED</C> with nothing
            debited and no transaction to reconcile.
          </p>
        </div>
      </Section>

      {/* ── 08 Status ─────────────────────────────────────────────────── */}
      <Section id="dmt-status" n={8} title="Status check">
        <p>
          Resolve a transfer by your own <C>requestid</C>, or by our <C>txnid</C>. The reply is byte-for-byte the
          same shape as <C>/dmt/transfer</C> with <C>event: "STATUS"</C> — one parser handles both.
        </p>
        <DmtEndpoint path="/dmt/status" v1={v1} />
        <ParamTable
          rows={[
            {
              name: 'requestid',
              type: 'string',
              required: 'Conditional',
              desc: 'Your reference. Send this or txnid.',
            },
            { name: 'txnid', type: 'integer', required: 'Conditional', desc: 'Our transaction id. Send this or requestid.' },
          ]}
        />
        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Request</div>
          <CodeTabs samples={statusSamples} />
        </div>
        <Callout tone="info" title="Poll sensibly.">
          A still-pending transfer is re-enquired with the bank before we answer, so the result reflects the rail and
          not just our record. Wait ~60 seconds after initiating, then poll at most every 30 seconds, and stop as
          soon as <C>final: true</C>. If you consume callbacks, polling is your fallback for missed deliveries — not
          the primary path.
        </Callout>
      </Section>

      {/* ── 09 Balance ────────────────────────────────────────────────── */}
      <Section id="dmt-balance" n={9} title="Balance">
        <p>
          The only <C>GET</C> in the DMT contract — it exposes no customer data and changes nothing. It is still
          signed; sign the empty body.
        </p>
        <DmtEndpoint path="/dmt/balance" v1={v1} method="GET" />
        <Code label="200 OK">{`{
  "status":     "SUCCESS",
  "statuscode": 0,
  "balance":    31826.00,          // total spendable across wallets
  "wallets": [
    { "walletid": 2, "wallet": "DMT Wallet", "balance": 13405.50, "hold": 0.00, "available": 13405.50 }
  ]
}`}</Code>
        <Callout tone="info" title="Watch available, not balance.">
          DMT transfers draw on the DMT wallet — <C>walletid: 2</C>. A held amount is included in{' '}
          <C>balance</C> but cannot be spent; <C>available</C> is what transfers actually draw on.
        </Callout>
      </Section>

      {/* ── 10 Reference ──────────────────────────────────────────────── */}
      <Section id="dmt-reference" n={10} title="Banks & IFSC">
        <DmtEndpoint path="/dmt/banks" v1={v1} />
        <p>
          The supported bank catalogue: <C>bankid</C>, <C>bankname</C>, <C>defaultifsc</C>, and <C>imps</C> — which
          is <C>false</C> for banks that accept NEFT only. Cache it; it changes rarely.
        </p>

        <DmtEndpoint path="/dmt/ifsc" v1={v1} />
        <p>
          Branch details behind an IFSC. Takes <C>ifsc</C>; returns <C>bankname</C>, <C>branch</C> and{' '}
          <C>address</C>. Use it to confirm a branch with the customer before registering the beneficiary.
        </p>
        <Code label="200 OK">{`{
  "status":   "SUCCESS",
  "ifsc":     "HDFC0000123",
  "bankname": "HDFC Bank",
  "branch":   "INDORE MG ROAD",
  "address":  "MG Road, Indore, Madhya Pradesh"
}`}</Code>
      </Section>

      {/* ── 11 Callbacks ──────────────────────────────────────────────── */}
      <Section id="dmt-callbacks" n={11} title="Callbacks">
        <p>
          When a transfer reaches a final status we POST the result to the callback URL configured on your account.
          The body is the same transaction payload with <C>event: "CALLBACK"</C>.
        </p>
        <Code label="POST to your callback URL">{`POST https://your-host/dmt-callback
Content-Type: application/json
X-Event: CALLBACK
X-Request-Id: DMT-1001
X-Transaction-Id: 884512
X-Signature-Algorithm: HMAC-SHA256
X-Signature: <HMAC-SHA256 of the raw body, keyed by your API token>

{ ...the same payload /dmt/transfer returns, with "event":"CALLBACK"... }`}</Code>
        <ParamTable
          rows={[
            {
              name: 'Verify X-Signature',
              type: 'rule',
              required: 'Required',
              desc: 'HMAC-SHA256 of the raw body with your API token — the same algorithm you use outbound. An unverified callback endpoint accepts anyone’s idea of “settled”.',
            },
            {
              name: 'Reply 2xx quickly',
              type: 'rule',
              required: 'Required',
              desc: 'A non-2xx or a timeout is retried with backoff. Acknowledge first, process after.',
            },
            {
              name: 'Be idempotent',
              type: 'rule',
              required: 'Required',
              desc: 'Retries mean the same callback can arrive more than once. Key your handler on requestid and ignore repeats.',
            },
            {
              name: 'Trust status, not arrival',
              type: 'rule',
              required: 'Required',
              desc: 'A callback can report FAILED. Its arrival is not confirmation of success.',
            },
          ]}
        />
      </Section>

      {/* ── 12 Codes ──────────────────────────────────────────────────── */}
      <Section id="dmt-codes" n={12} title="Status & error codes">
        <p>
          <C>errorcode</C> is stable and safe to switch on. <C>message</C> is human-readable and may be reworded at
          any time — never match on it.
        </p>

        <div>
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Request security
          </div>
          <RefTable head={['Code', 'Fix']} rows={DMT_SECURITY_CODES} />
        </div>

        <div>
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Authentication
          </div>
          <RefTable
            head={['Code', 'Fix']}
            rows={[
              ['AUTH_MOBILE_REQUIRED', 'Send X-Api-Mobile.'],
              ['AUTH_TOKEN_REQUIRED', 'Send X-Api-Token.'],
              ['AUTH_USER_NOT_FOUND', 'No account is registered with that mobile number.'],
              ['AUTH_NOT_API_USER', 'The account is not an API-user account.'],
              ['AUTH_ACCOUNT_INACTIVE', 'Contact support.'],
              ['AUTH_INVALID_TOKEN', 'Token is wrong or was regenerated.'],
              ['AUTH_IP_NOT_WHITELISTED', 'Add the calling IP on API Access.'],
            ]}
          />
        </div>

        <div>
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Validation, flow & transaction
          </div>
          <RefTable head={['Code', 'Fix']} rows={DMT_FLOW_CODES} filterable filterLabel="Filter error codes" />
        </div>
      </Section>

      {/* ── 13 Checklist ──────────────────────────────────────────────── */}
      <Section id="dmt-checklist" n={13} title="Go-live checklist">
        <RefTable
          head={['Item', 'Confirm']}
          rows={[
            ['Signature reproduced', 'Your code produces 19ca0fbf…76e6 for the worked example.'],
            ['Clock synced', 'NTP running; timestamps in SECONDS.'],
            ['Nonce per request', 'Fresh UUID every call, never cached.'],
            ['requestid persisted', 'Written to your database BEFORE the transfer call.'],
            ['Timeout path', 'On timeout you call /dmt/status with the same requestid — never re-send.'],
            ['Callback verified', 'You check X-Signature and handle repeats.'],
            ['PENDING handled', 'Your UI shows “processing”, not “failed”.'],
            ['Limits surfaced', 'You call /dmt/sender/fetch and show remaininglimit before quoting.'],
            ['IPs whitelisted', 'Every production egress IP is registered.'],
            ['Token stored safely', 'Environment or secret store — never in source control.'],
          ]}
        />
        <Callout tone="good" icon={ShieldCheck} title="Ready.">
          Once every row above is true, ask your account manager to enable DMT on your API user. Amount bounds and
          the clock-skew window are deployment settings — confirm the values in force before go-live.
        </Callout>
      </Section>
    </div>
  );
}

const PAYOUT_FLOW_CODES: string[][] = [
  ['VAL_REQUESTID_REQUIRED', 'Send a unique requestid on /payout/transfer.'],
  ['VAL_ACCOUNT_REQUIRED', 'accountno is missing, or is not 5–25 alphanumeric characters.'],
  ['VAL_IFSC_REQUIRED', 'ifsc is missing.'],
  ['VAL_IFSC_INVALID', 'Must be 11 characters, e.g. HDFC0000123.'],
  ['VAL_BENEFICIARY_NAME_REQUIRED', 'beneficiaryname is missing — payout names the holder inline.'],
  ['VAL_AMOUNT_INVALID', 'amount must be a number greater than zero.'],
  ['VAL_AMOUNT_BELOW_LIMIT', 'Below the per-transfer floor.'],
  ['VAL_AMOUNT_ABOVE_LIMIT', 'Above the per-transfer ceiling.'],
  ['VAL_LOCATION_INVALID', 'Send latitude and longitude together, and as real coordinates.'],
  ['PAYOUT_UNAVAILABLE', 'Payout is not enabled on this platform. Contact support — not a parameter error.'],
  ['TXN_REJECTED', 'Declined before a transaction existed, usually balance or a limit.'],
  ['TXN_NOT_FOUND', 'No payout for that requestid / txnid.'],
  ['SYS_ERROR', 'Our fault. Do not re-send — call /payout/status with the same requestid.'],
];

function PayoutDoc({ appName, v1 }: { appName: string; v1: string }) {
  const transferSamples = useMemo(
    () =>
      signedSamples(v1.replace(/\/v1$/, ''), '/api/v1/payout/transfer', [
        ['requestid', 'PO-1001'],
        ['accountno', '50100234567890'],
        ['ifsc', 'HDFC0000123'],
        ['beneficiaryname', 'Sunita Devi'],
        ['amount', '5000'],
        ['paymenttype', 'IMPS'],
        ['latitude', '22.7196'],
        ['longitude', '75.8577'],
      ]),
    [v1],
  );

  const statusSamples = useMemo(
    () => signedSamples(v1.replace(/\/v1$/, ''), '/api/v1/payout/status', [['requestid', 'PO-1001']]),
    [v1],
  );

  return (
    <div className="space-y-10">
      {/* ── 01 Overview ───────────────────────────────────────────────── */}
      <Section id="payout-overview" n={1} title="Overview">
        <p>
          Payout moves money out of <span className="font-medium text-slate-700">your own</span> {appName} wallet to
          any Indian bank account over IMPS, NEFT or RTGS. It is{' '}
          <span className="font-medium text-slate-700">one request</span> — the destination account travels on the
          transfer call itself.
        </p>

        <Callout tone="warn" title="Payout does not take a beneficiaryid, and never will.">
          <C>beneficiaryid</C> belongs to <C>/api/v1/dmt</C>, where the money is a walk-in customer’s cash and the
          regulator requires a registered, OTP-verified remitter and beneficiary. Payout moves your own funds, so
          there is nothing to pre-register: send <C>accountno</C>, <C>ifsc</C> and <C>beneficiaryname</C> inline on{' '}
          <C>/payout/transfer</C>. There is no sender, no OTP, and no id to fetch from anywhere.
        </Callout>

        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          <div className="flex items-center gap-3 px-4 py-2.5">
            <span className="shrink-0 text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
              Base URL
            </span>
            <code className="min-w-0 flex-1 truncate font-mono text-[12.5px] text-slate-700">{v1}/payout/</code>
            <CopyButton value={`${v1}/payout/`} />
          </div>
          <div className="grid gap-px border-t border-slate-100 bg-slate-100 sm:grid-cols-2">
            {[
              { k: 'Auth', v: 'mobile + token + HMAC signature · optional IP allowlist' },
              { k: 'Methods', v: 'POST everywhere · GET only for /payout/balance' },
              { k: 'Rails', v: 'IMPS · NEFT · RTGS' },
              { k: 'Steps', v: 'One call. No registration, no OTP' },
            ].map((x) => (
              <div key={x.k} className="bg-white px-4 py-3">
                <div className="text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">{x.k}</div>
                <div className="mt-1 text-[13px] text-slate-700">{x.v}</div>
              </div>
            ))}
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5">
          <div className="mb-3 text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
            The whole flow
          </div>
          <ol className="space-y-2.5">
            <FlowStep n={1} title="Optional: confirm the IFSC">
              <C>/payout/ifsc</C> returns the bank and branch. Free, and it catches a mistyped IFSC before money
              moves.
            </FlowStep>
            <FlowStep n={2} title="Send">
              <C>/payout/transfer</C> with a fresh <C>requestid</C> and the destination account.
            </FlowStep>
            <FlowStep n={3} title="Settle">
              Wait for the callback, or poll <C>/payout/status</C> until <C>final: true</C>.
            </FlowStep>
          </ol>
        </div>

        <Callout tone="warn" title="Never retry a payout with a new requestid.">
          If <C>/payout/transfer</C> times out, the transfer may already be in flight. Resolve it by calling{' '}
          <C>/payout/status</C> with the <span className="font-medium text-slate-700">same</span> requestid. A fresh
          requestid creates a second, independent payout — and the money leaves twice.
        </Callout>
      </Section>

      {/* ── 02 Auth & signing ─────────────────────────────────────────── */}
      <Section id="payout-auth" n={2} title="Authentication & signing">
        <p>
          Identical to the DMT contract: five headers, and an HMAC-SHA256 signature over the raw body. If you have
          already integrated DMT, your signing code works here unchanged — only the path on line 2 of the canonical
          string differs.
        </p>
        <ParamTable
          rows={[
            {
              name: 'X-Api-Mobile',
              type: 'header',
              required: 'Required',
              desc: (
                <>
                  Your account’s registered 10-digit mobile number — shown as{' '}
                  <span className="font-medium text-slate-700">API Mobile Number</span> on{' '}
                  <Link to="/s/ApiAccess" className="font-medium text-brand-600 hover:underline">
                    API Access
                  </Link>
                  , next to the token. It is not secret.
                </>
              ),
            },
            {
              name: 'X-Api-Token',
              type: 'header',
              required: 'Required',
              desc: 'Your API token from the same page. It is also the HMAC signing key.',
            },
            { name: 'X-Api-Timestamp', type: 'header', required: 'Required', desc: 'Unix epoch SECONDS, UTC.' },
            {
              name: 'X-Api-Nonce',
              type: 'header',
              required: 'Required',
              desc: 'Unique per request, 8–128 characters. A UUID is a good choice.',
            },
            {
              name: 'X-Api-Signature',
              type: 'header',
              required: 'Required',
              desc: 'Lowercase hex HMAC-SHA256 over the canonical string below.',
            },
          ]}
        />
        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            The canonical string — five lines joined by \n
          </div>
          <Code label="canonical">{`POST                                  ← HTTP method, uppercase
/api/v1/payout/transfer               ← path, lowercase, no query string
1757923200                            ← timestamp, exactly as sent
9f1c4a2e-6b8d-4f30-a1e7-5c9d2b3a8e64  ← nonce, exactly as sent
583280e9d8432e1c2840…                 ← SHA-256 of the RAW body, lowercase hex

signature = HMAC-SHA256(canonical, your_api_token)  →  lowercase hex`}</Code>
        </div>
        <Callout tone="warn" title="Hash the bytes you actually send.">
          Line 5 is a SHA-256 of your request body exactly as it goes on the wire — not of a re-serialised copy.
          Build the JSON string once, hash <span className="font-medium text-slate-700">that</span> string, and send
          the same string. Re-serialising before sending is the most common cause of <C>SEC_SIGNATURE_INVALID</C>.
          The worked example in the DMT docs reproduces the whole calculation end to end.
        </Callout>
        <Callout tone="info" title="No custom headers in your stack?">
          All five may instead be sent as body fields named <C>mobile</C>, <C>token</C>, <C>timestamp</C>,{' '}
          <C>nonce</C> and <C>signature</C>. Headers win where both are present.
        </Callout>
      </Section>

      {/* ── 03 Transfer ───────────────────────────────────────────────── */}
      <Section id="payout-transfer" n={3} title="Transfer">
        <p>The money-moving call, and the entire contract in one request.</p>
        <DmtEndpoint path="/payout/transfer" v1={v1} />

        <ParamTable
          rows={[
            {
              name: 'requestid',
              type: 'string',
              required: 'Required',
              desc: 'Your own unique reference. This is the idempotency key — see the note below.',
            },
            { name: 'accountno', type: 'string', required: 'Required', desc: 'Destination account number. Digits and letters only — no spaces or dashes.' },
            { name: 'ifsc', type: 'string', required: 'Required', desc: 'Destination IFSC, 11 characters.' },
            { name: 'beneficiaryname', type: 'string', required: 'Required', desc: 'Account holder’s name.' },
            { name: 'amount', type: 'decimal', required: 'Required', desc: 'Rupees. Per-transfer bounds are a deployment setting — confirm yours.' },
            { name: 'paymenttype', type: 'string', required: 'Optional', desc: 'IMPS (default), NEFT or RTGS.' },
            { name: 'bankname', type: 'string', required: 'Optional', desc: 'Resolved from the IFSC when you omit it.' },
            { name: 'remark', type: 'string', required: 'Optional', desc: 'Narration, where the bank carries it.' },
            { name: 'mobile', type: 'string', required: 'Optional', desc: 'A contact number stored against the payout for your own reconciliation.' },
            { name: 'latitude / longitude', type: 'string', required: 'Optional', desc: 'Outlet coordinates, stored against the transaction for audit. Send both or neither; each is validated as a real coordinate.' },
          ]}
        />

        <Callout tone="good" icon={ShieldCheck} title="Idempotent by requestid.">
          A <C>requestid</C> you have already used is never debited twice — we return the original payout’s current
          result with <C>"duplicate": true</C>. So a timeout is safe to resolve:{' '}
          <span className="font-medium text-slate-700">retry with the same requestid</span>, never a new one.
        </Callout>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Request</div>
          <CodeTabs samples={transferSamples} />
        </div>

        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Response · shared by /payout/transfer, /payout/status and callbacks
          </div>
          <Code label="200 OK">{`{
  "status":       "PENDING",
  "statuscode":   1,
  "errorcode":    "",
  "message":      "Payout initiated.",
  "requestid":    "PO-1001",
  "event":        "INITIAL",     // INITIAL | STATUS | CALLBACK
  "final":        false,         // true once status can no longer change
  "duplicate":    false,         // true when replaying a used requestid
  "txnid":        884512,        // our transaction id
  "providerref":  "IMP1234567",
  "amount":       5000.00,
  "debitamount":  5015.00,       // principal + charges
  "balance":      13405.50,      // your wallet afterwards
  "date":         "2026-10-12 14:20:11",

  "accountno":       "50100234567890",
  "ifsc":            "HDFC0000123",
  "beneficiaryname": "Sunita Devi",
  "bankname":        "HDFC Bank",
  "paymenttype":     "IMPS",
  "utr":             "",          // bank UTR — populated once SUCCESS
  "charge":          15.00
}`}</Code>
        </div>

        <Callout tone="info" title="utr is the number that settles disputes.">
          It is the reference the beneficiary’s own bank recognises, and it appears only once the payout reaches{' '}
          <C>SUCCESS</C>. While <C>PENDING</C> it is deliberately empty.
        </Callout>

        <div className="rounded-xl border border-slate-200 bg-white p-5">
          <div className="mb-3 text-[10.5px] font-semibold uppercase tracking-wide text-slate-400">
            What is checked before anything is debited
          </div>
          <RefTable
            head={['Check', 'On failure']}
            rows={[
              ['Credentials, account active, IP allowed', 'AUTH_*'],
              ['Signature, timestamp, nonce', 'SEC_*'],
              ['requestid present', 'VAL_REQUESTID_REQUIRED'],
              ['requestid already used', 'Original result, duplicate: true'],
              ['Account number shape', 'VAL_ACCOUNT_REQUIRED'],
              ['IFSC shape', 'VAL_IFSC_REQUIRED / VAL_IFSC_INVALID'],
              ['Holder name present', 'VAL_BENEFICIARY_NAME_REQUIRED'],
              ['Amount within bounds', 'VAL_AMOUNT_BELOW_LIMIT / VAL_AMOUNT_ABOVE_LIMIT'],
              ['Payout service enabled on the platform', 'PAYOUT_UNAVAILABLE'],
              ['Wallet covers principal + charges', 'TXN_REJECTED'],
            ]}
          />
          <p className="mt-3 text-[13px] leading-relaxed text-slate-500">
            All of it happens before a rupee moves. Any failure returns <C>ERROR</C> or <C>FAILED</C> with nothing
            debited and no transaction to reconcile.
          </p>
        </div>
      </Section>

      {/* ── 04 Status ─────────────────────────────────────────────────── */}
      <Section id="payout-status" n={4} title="Status check">
        <p>
          Resolve a payout by your own <C>requestid</C>, or by our <C>txnid</C>. The reply is the same shape as{' '}
          <C>/payout/transfer</C> with <C>event: "STATUS"</C> — one parser handles both.
        </p>
        <DmtEndpoint path="/payout/status" v1={v1} />
        <ParamTable
          rows={[
            { name: 'requestid', type: 'string', required: 'Conditional', desc: 'Your reference. Send this or txnid.' },
            { name: 'txnid', type: 'integer', required: 'Conditional', desc: 'Our transaction id. Send this or requestid.' },
          ]}
        />
        <div className="pt-1">
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">Request</div>
          <CodeTabs samples={statusSamples} />
        </div>
        <Callout tone="info" title="Poll sensibly.">
          A still-pending payout is re-enquired with the bank before we answer. Wait ~60 seconds after initiating,
          then poll at most every 30 seconds, and stop as soon as <C>final: true</C>.
        </Callout>
      </Section>

      {/* ── 05 Balance ────────────────────────────────────────────────── */}
      <Section id="payout-balance" n={5} title="Balance">
        <p>
          The only <C>GET</C> in the payout contract — it exposes no customer data and changes nothing. It is still
          signed; sign the empty body.
        </p>
        <DmtEndpoint path="/payout/balance" v1={v1} method="GET" />
        <Callout tone="good" icon={ShieldCheck} title="Signing a GET.">
          There is no body, so line 5 of the canonical string is the SHA-256 of the empty string:{' '}
          <C>e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855</C>.
        </Callout>
        <Callout tone="info" title="Watch available, not balance.">
          A held amount is included in <C>balance</C> but cannot be spent; <C>available</C> is what a payout
          actually draws on.
        </Callout>
      </Section>

      {/* ── 06 IFSC ───────────────────────────────────────────────────── */}
      <Section id="payout-ifsc" n={6} title="IFSC lookup">
        <DmtEndpoint path="/payout/ifsc" v1={v1} />
        <p>
          Branch details behind an IFSC. Takes <C>ifsc</C>; returns <C>bankname</C>, <C>branch</C> and{' '}
          <C>address</C>. Free — call it to confirm the destination before you move money.
        </p>
        <Code label="200 OK">{`{
  "status":   "SUCCESS",
  "ifsc":     "HDFC0000123",
  "bankname": "HDFC Bank",
  "branch":   "INDORE MG ROAD",
  "address":  "MG Road, Indore, Madhya Pradesh"
}`}</Code>
      </Section>

      {/* ── 07 Callbacks ──────────────────────────────────────────────── */}
      <Section id="payout-callbacks" n={7} title="Callbacks">
        <p>
          When a payout reaches a final status we POST the result to the callback URL configured on your account for
          the payout service. The body is the same transaction payload with <C>event: "CALLBACK"</C>.
        </p>
        <Code label="POST to your callback URL">{`POST https://your-host/payout-callback
Content-Type: application/json
X-Event: CALLBACK
X-Request-Id: PO-1001
X-Transaction-Id: 884512
X-Signature-Algorithm: HMAC-SHA256
X-Signature: <HMAC-SHA256 of the raw body, keyed by your API token>

{ ...the same payload /payout/transfer returns, with "event":"CALLBACK"... }`}</Code>
        <ParamTable
          rows={[
            {
              name: 'Verify X-Signature',
              type: 'rule',
              required: 'Required',
              desc: 'HMAC-SHA256 of the raw body with your API token. An unverified callback endpoint accepts anyone’s idea of “settled”.',
            },
            {
              name: 'Reply 2xx quickly',
              type: 'rule',
              required: 'Required',
              desc: 'A non-2xx or a timeout is retried with backoff. Acknowledge first, process after.',
            },
            {
              name: 'Be idempotent',
              type: 'rule',
              required: 'Required',
              desc: 'The same callback can arrive more than once. Key your handler on requestid and ignore repeats.',
            },
            {
              name: 'Trust status, not arrival',
              type: 'rule',
              required: 'Required',
              desc: 'A callback can report FAILED. Its arrival is not confirmation of success.',
            },
          ]}
        />
      </Section>

      {/* ── 08 Codes ──────────────────────────────────────────────────── */}
      <Section id="payout-codes" n={8} title="Status & error codes">
        <p>
          <C>errorcode</C> is stable and safe to switch on. <C>message</C> is human-readable and may be reworded at
          any time — never match on it.
        </p>
        <RefTable head={['status', 'code', 'Meaning']} rows={DMT_STATUS_VALUES} />
        <div>
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Request security
          </div>
          <RefTable
            head={['Code', 'Fix']}
            rows={[
              ['SEC_SIGNATURE_REQUIRED', 'Send the three signing headers.'],
              ['SEC_SIGNATURE_INVALID', 'Re-check the canonical string; hash the bytes you actually send.'],
              ['SEC_TIMESTAMP_INVALID', 'Use epoch seconds, not milliseconds. Sync your clock (NTP).'],
              ['SEC_NONCE_INVALID', 'Generate a fresh nonce for every request.'],
              ['SEC_METHOD_NOT_ALLOWED', 'Use POST. Only /payout/balance accepts GET.'],
            ]}
          />
        </div>
        <div>
          <div className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-slate-400">
            Validation & transaction
          </div>
          <RefTable head={['Code', 'Fix']} rows={PAYOUT_FLOW_CODES} filterable filterLabel="Filter error codes" />
        </div>
      </Section>

      {/* ── 09 Checklist ──────────────────────────────────────────────── */}
      <Section id="payout-checklist" n={9} title="Go-live checklist">
        <RefTable
          head={['Item', 'Confirm']}
          rows={[
            ['Signature reproduced', 'Your code reproduces the worked example in the DMT signing section.'],
            ['Clock synced', 'NTP running; timestamps in SECONDS.'],
            ['Nonce per request', 'Fresh UUID every call, never cached.'],
            ['requestid persisted', 'Written to your database BEFORE the transfer call.'],
            ['Timeout path', 'On timeout you call /payout/status with the same requestid — never re-send.'],
            ['Callback verified', 'You check X-Signature and handle repeats.'],
            ['PENDING handled', 'Your UI shows “processing”, not “failed”.'],
            ['Account validated', 'You confirm the IFSC via /payout/ifsc and strip spaces from account numbers.'],
            ['IPs whitelisted', 'Every production egress IP is registered.'],
            ['Token stored safely', 'Environment or secret store — never in source control.'],
          ]}
        />
        <Callout tone="good" icon={ShieldCheck} title="Ready.">
          Once every row above is true, ask your account manager to enable payout on your API user. Amount bounds
          and the clock-skew window are deployment settings — confirm the values in force before go-live.
        </Callout>
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
  const current = PRODUCTS.find((d) => d.key === product) ?? PRODUCTS[0];
  const sections = sectionsFor(product);
  const [active, setActive] = useState(sections[0].id);
  const documented = product === 'recharge' || product === 'dmt' || product === 'payout';

  // Highlight the section the reader is actually looking at. Only a narrow band near the
  // top of the viewport counts as "reading here". The observer reports CHANGES, not the
  // full picture, so visibility is accumulated in a set — otherwise scrolling past several
  // sections at once leaves the highlight on whichever one last fired an event.
  useEffect(() => {
    if (!documented) return;

    // Switching products swaps the whole table of contents, so reset the highlight to the
    // new document's first section rather than leaving it on an id that no longer exists.
    setActive(sections[0].id);

    const visible = new Set<string>();
    const observer = new IntersectionObserver(
      (entries) => {
        for (const e of entries) {
          if (e.isIntersecting) visible.add(e.target.id);
          else visible.delete(e.target.id);
        }
        const first = sections.find((s) => visible.has(s.id));
        if (first) setActive(first.id);
      },
      { rootMargin: '-8% 0px -75% 0px', threshold: 0 },
    );

    sections.forEach((s) => {
      const el = document.getElementById(s.id);
      if (el) observer.observe(el);
    });
    return () => observer.disconnect();
  }, [documented, sections]);

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

          {documented && (
            <div className="mt-4 hidden lg:block">
              <div className="px-3 pb-2 text-[10px] font-semibold uppercase tracking-wider text-slate-400">
                On this page
              </div>
              <nav className="space-y-px border-l border-slate-200">
                {sections.map((s) => (
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
              {product === 'dmt' && (
                <p className="mt-2 max-w-2xl text-[14px] leading-relaxed text-slate-500">
                  Sender, beneficiary, transfer — one response shape across all three, plus the callback. Every
                  endpoint is POST-only and signed; only the balance read is a GET.
                </p>
              )}
              {product === 'payout' && (
                <p className="mt-2 max-w-2xl text-[14px] leading-relaxed text-slate-500">
                  One signed call moves money from your wallet to any bank account. No sender, no beneficiary
                  registration, no OTP — and no beneficiaryid.
                </p>
              )}
            </header>

            {product === 'recharge' ? (
              <RechargeDoc appName={appName} v1={v1} />
            ) : product === 'dmt' ? (
              <DmtDoc appName={appName} v1={v1} />
            ) : product === 'payout' ? (
              <PayoutDoc appName={appName} v1={v1} />
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
