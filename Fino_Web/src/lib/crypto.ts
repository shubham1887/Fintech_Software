import CryptoJS from 'crypto-js';
import { getConfig, cfgValue } from './runtimeConfig';

/**
 * AES-256-CBC + PKCS7, mirroring Fino_API `Helpers/EncryptDecrypt.cs`.
 * Key/IV are parsed as raw UTF-8 bytes; ciphertext is Base64. They MUST match the
 * backend `Encryption:Key` / `Encryption:IV`. Resolved at run time so a resale copy is
 * retargeted by editing config.json only (no rebuild):
 *   config.json "encKey"/"encIv"  →  build-time VITE_ENC_KEY/VITE_ENC_IV  →  default.
 */
const KEY = CryptoJS.enc.Utf8.parse(
  cfgValue(getConfig().encKey) ?? import.meta.env.VITE_ENC_KEY ?? 'Nx7pQ2sVbY8mK4wR9tE1uH6zC3jL5dA0',
);
const IV = CryptoJS.enc.Utf8.parse(
  cfgValue(getConfig().encIv) ?? import.meta.env.VITE_ENC_IV ?? 'Fp3Kd9Xm2Qs7Vb1L',
);

const CFG = {
  iv: IV,
  mode: CryptoJS.mode.CBC,
  padding: CryptoJS.pad.Pkcs7,
} as const;

/** Encrypt a plaintext string → Base64 ciphertext (no salt prefix). */
export function encrypt(plain: string): string {
  if (plain === '' || plain == null) return '';
  return CryptoJS.AES.encrypt(plain, KEY, CFG).toString();
}

/**
 * Decrypt Base64 ciphertext → plaintext. Returns '' on any failure (e.g. the
 * value was actually plain text, not ciphertext) so callers can fall back.
 */
export function decrypt(cipher: string): string {
  if (!cipher) return '';
  try {
    const bytes = CryptoJS.AES.decrypt(cipher, KEY, CFG);
    return bytes.toString(CryptoJS.enc.Utf8);
  } catch {
    return '';
  }
}
