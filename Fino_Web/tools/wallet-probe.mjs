/**
 * Wallet balance probe — talks to Fino_API directly, with the same AES envelope the web app
 * uses, and prints the RAW answer for the two calls the balance card depends on.
 *
 * Use it when a user says "balance nahi dikh raha". The UI now shows the server's reason, but
 * this tells you the same thing without needing that user's phone.
 *
 *   node tools/wallet-probe.mjs <mobile> <password> [apiBase]
 *
 * Example:
 *   node tools/wallet-probe.mjs 8888888888 'Secret@123'
 *   node tools/wallet-probe.mjs 8888888888 'Secret@123' https://api.ripay.co.in/api/
 *
 * Key/IV come from Fino_Web/.env (VITE_ENC_KEY / VITE_ENC_IV) and MUST match the API's
 * Encryption:Key / Encryption:IV.
 */
import crypto from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.dirname(path.dirname(fileURLToPath(import.meta.url)));

function fromEnvFile(name, fallback) {
  for (const file of ['.env.local', '.env']) {
    const p = path.join(root, file);
    if (!fs.existsSync(p)) continue;
    const line = fs
      .readFileSync(p, 'utf8')
      .split(/\r?\n/)
      .find((l) => l.trim().startsWith(`${name}=`));
    if (line) return line.slice(line.indexOf('=') + 1).split('#')[0].trim();
  }
  return fallback;
}

const [, , mobile, password, baseArg] = process.argv;
if (!mobile || !password) {
  console.error('usage: node tools/wallet-probe.mjs <mobile> <password> [apiBase]');
  process.exit(2);
}

const BASE = (baseArg || fromEnvFile('VITE_API_URL', 'http://127.0.0.1:5266/api/'))
  .split('#')[0]
  .trim()
  .replace(/\/?$/, '/');
const KEY = Buffer.from(fromEnvFile('VITE_ENC_KEY', ''), 'utf8');
const IV = Buffer.from(fromEnvFile('VITE_ENC_IV', ''), 'utf8');

if (KEY.length !== 32 || IV.length !== 16) {
  console.error(`Bad key/IV: key=${KEY.length}B (need 32), iv=${IV.length}B (need 16). Check .env`);
  process.exit(2);
}

const enc = (plain) => {
  const c = crypto.createCipheriv('aes-256-cbc', KEY, IV);
  return Buffer.concat([c.update(plain, 'utf8'), c.final()]).toString('base64');
};

const dec = (b64) => {
  try {
    const d = crypto.createDecipheriv('aes-256-cbc', KEY, IV);
    return Buffer.concat([d.update(Buffer.from(b64, 'base64')), d.final()]).toString('utf8');
  } catch {
    return '';
  }
};

const pick = (o, n) => o?.[n] ?? o?.[n[0].toUpperCase() + n.slice(1)];

async function call(endpoint, payload, { token, keyData } = {}) {
  const form = new FormData();
  form.append('Request', enc(JSON.stringify(payload ?? {})));
  form.append('KeyData', keyData ? enc(JSON.stringify(keyData)) : enc('{}'));

  const res = await fetch(BASE + endpoint, {
    method: 'POST',
    body: form,
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });

  const env = await res.json().catch(() => ({}));
  const raw = pick(env, 'data') ?? '';
  const plain = raw ? dec(raw) || raw : '';
  let data = null;
  try {
    data = plain ? JSON.parse(plain) : null;
  } catch {
    data = plain;
  }
  return { http: res.status, status: String(pick(env, 'status') ?? ''), message: String(pick(env, 'message') ?? ''), data };
}

console.log(`API: ${BASE}\n`);

const login = await call('Auth/login', { MobileNumber: mobile, Password: password, DeviceType: 'Web', IPAddress: '' });
console.log(`login              -> HTTP ${login.http} ${login.status} | ${login.message}`);
if (!login.data) process.exit(1);

const user = login.data.user ?? login.data.User;
const tokens = login.data.tokens ?? login.data.Tokens;
const token = tokens.AccessToken ?? tokens.accessToken;
console.log(`  UserID=${user.UserID} UserTypeID=${user.UserTypeID} (${user.FirmName || '-'})\n`);

const keyData = { UserID: user.UserID, UserTypeID: user.UserTypeID, SessionToken: token, DeviceType: 'Web', IPAddress: '' };

const bal = await call('UserWalletMaster/GetByUserID', { UserID: user.UserID }, { token, keyData });
console.log(`UserWalletMaster/GetByUserID -> HTTP ${bal.http} ${bal.status} | ${bal.message}`);
console.log(`  ${JSON.stringify(bal.data)}\n`);

const defs = await call('Wallet/Select', {}, { token, keyData });
console.log(`Wallet/Select                -> HTTP ${defs.http} ${defs.status} | ${defs.message}`);
console.log(`  ${Array.isArray(defs.data) ? `${defs.data.length} wallet(s)` : JSON.stringify(defs.data)}\n`);

// What the UI will do with those two answers.
if (bal.status.toUpperCase() !== 'SUCCESS' && !/no\s*record/i.test(bal.message)) {
  console.log('VERDICT: balance card will show the ERROR state with the message above.');
} else if (!Array.isArray(bal.data) || bal.data.length === 0) {
  console.log('VERDICT: user genuinely has no wallet rows — every wallet shows a real Rs 0.00.');
} else {
  const total = bal.data.reduce((s, r) => s + Number(r.Balance ?? 0), 0);
  console.log(`VERDICT: balance loads fine. Total = ${total}`);
}
