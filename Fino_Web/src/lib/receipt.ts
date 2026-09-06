import { getField } from '@/lib/row';
import { inr } from '@/lib/format';

export interface ReceiptBrand {
  appName: string;
  logoUrl?: string;
}

export interface ReceiptLine { label: string; value: string; }
export interface ReceiptData { id: string; status: string; lines: ReceiptLine[]; appName: string; }

/**
 * Dynamic transaction receipt — reusable from any transaction report row (recharge / DMT / BBPS /
 * AEPS / payout / MicroATM). Reads whatever fields the row happens to carry (first present key per
 * line wins), so one implementation serves every product. Supports print, download (PNG) and share.
 */
const LINES: Array<{ label: string; keys: string[]; money?: boolean; date?: boolean }> = [
  { label: 'Receipt No.', keys: ['TransactionMasterID', 'TransactionID', 'ReferenceNo'] },
  { label: 'Date & Time', keys: ['DOC', 'TransactionDate'], date: true },
  { label: 'Entity', keys: ['FirmName', 'SenderName', 'PersonName'] },
  { label: 'Mobile', keys: ['UserMobile', 'SenderNo'] },
  { label: 'Operator / Service', keys: ['OperatorName', 'BankName', 'Bank'] },
  { label: 'Number / Account', keys: ['ConsumerNumber'] },
  { label: 'Beneficiary', keys: ['BeneName', 'BeneficiaryName'] },
  { label: 'Amount', keys: ['Amount'], money: true },
  { label: 'Charge', keys: ['Charge'], money: true },
  { label: 'Commission', keys: ['RetailerCommission', 'RetailerComm', 'NetCommission'], money: true },
  { label: 'Opening Balance', keys: ['OpeningBalance'], money: true },
  { label: 'Closing Balance', keys: ['ClosingBalance'], money: true },
  { label: 'Operator Ref.', keys: ['ReferenceNo'] },
  { label: 'Transaction Ref.', keys: ['ProviderRequestID', 'ProviderRefNo'] },
  { label: 'Remark', keys: ['Remark', 'Description'] },
];

function firstPresent(row: Record<string, unknown>, keys: string[]): unknown {
  for (const k of keys) {
    const v = getField(row, k);
    if (v !== null && v !== undefined && v !== '') return v;
  }
  return undefined;
}

export function getReceiptData(row: Record<string, unknown>, brand: ReceiptBrand): ReceiptData {
  const lines: ReceiptLine[] = [];
  for (const f of LINES) {
    const raw = firstPresent(row, f.keys);
    if (raw === undefined) continue;
    let value: string;
    if (f.money) value = inr(Number(raw) || 0);
    else if (f.date) { const d = new Date(String(raw)); value = isNaN(d.getTime()) ? String(raw) : d.toLocaleString(); }
    else value = String(raw);
    lines.push({ label: f.label, value });
  }
  return {
    id: String(getField(row, 'TransactionMasterID', 'TransactionID') ?? 'txn'),
    status: (String(getField(row, 'Status') ?? '').toUpperCase()) || 'PENDING',
    lines,
    appName: brand.appName || 'Receipt',
  };
}

export function statusColor(status: string): string {
  const s = status.toUpperCase();
  return s.startsWith('SUCCESS') ? '#16a34a' : s.startsWith('FAIL') ? '#dc2626' : '#d97706';
}

function esc(s: unknown): string {
  return String(s ?? '').replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c] as string));
}

// ─── Print (opens a print-ready window) ───────────────────────────────────
export function printReceipt(row: Record<string, unknown>, brand: ReceiptBrand): boolean {
  const d = getReceiptData(row, brand);
  const rows = d.lines.map((l) => `<tr><td class="l">${esc(l.label)}</td><td class="v">${esc(l.value)}</td></tr>`).join('');
  const html = `<!doctype html><html><head><meta charset="utf-8"><title>Receipt ${esc(d.id)}</title>
<style>
  *{box-sizing:border-box;margin:0;padding:0}
  body{font-family:'Segoe UI',system-ui,Arial,sans-serif;color:#1e293b;background:#f1f5f9;padding:16px}
  .receipt{max-width:360px;margin:0 auto;background:#fff;border:1px solid #e2e8f0;border-radius:12px;overflow:hidden}
  .head{text-align:center;padding:18px 16px 10px}
  .app{font-size:18px;font-weight:800}.sub{font-size:11px;color:#64748b;margin-top:2px}
  .status{display:inline-block;margin:10px auto 0;padding:3px 12px;border-radius:999px;font-size:12px;font-weight:700;color:#fff;background:${statusColor(d.status)}}
  table{width:100%;border-collapse:collapse;margin:8px 0}
  td{padding:7px 16px;font-size:12.5px;border-bottom:1px dashed #e2e8f0;vertical-align:top}
  td.l{color:#64748b;white-space:nowrap}td.v{text-align:right;font-weight:600;word-break:break-word}
  .foot{text-align:center;padding:12px 16px 18px;font-size:10.5px;color:#94a3b8}
  @media print{body{background:#fff;padding:0}.receipt{border:none}}
</style></head><body>
  <div class="receipt"><div class="head"><div class="app">${esc(d.appName)}</div>
  <div class="sub">Transaction Receipt</div><div class="status">${esc(d.status)}</div></div>
  <table><tbody>${rows}</tbody></table>
  <div class="foot">This is a system-generated receipt.<br/>Printed on ${new Date().toLocaleString()}</div></div>
</body></html>`;
  const w = window.open('', '_blank', 'width=420,height=680');
  if (!w) return false;
  w.document.write(html); w.document.close(); w.focus();
  setTimeout(() => { try { w.print(); } catch { /* ignore */ } }, 350);
  return true;
}

// ─── Render to a canvas (for download / share as PNG) ─────────────────────
function roundRect(ctx: CanvasRenderingContext2D, x: number, y: number, w: number, h: number, r: number) {
  if (typeof ctx.roundRect === 'function') { ctx.beginPath(); ctx.roundRect(x, y, w, h, r); return; }
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.arcTo(x + w, y, x + w, y + h, r);
  ctx.arcTo(x + w, y + h, x, y + h, r);
  ctx.arcTo(x, y + h, x, y, r);
  ctx.arcTo(x, y, x + w, y, r);
  ctx.closePath();
}

function fit(ctx: CanvasRenderingContext2D, text: string, maxW: number): string {
  if (ctx.measureText(text).width <= maxW) return text;
  let t = text;
  while (t.length > 1 && ctx.measureText(t + '…').width > maxW) t = t.slice(0, -1);
  return t + '…';
}

function renderCanvas(row: Record<string, unknown>, brand: ReceiptBrand): HTMLCanvasElement {
  const d = getReceiptData(row, brand);
  const scale = 2, W = 360, padX = 20, headerH = 96, rowH = 30, footerH = 48;
  const H = headerH + d.lines.length * rowH + footerH;
  const canvas = document.createElement('canvas');
  canvas.width = W * scale; canvas.height = H * scale;
  const ctx = canvas.getContext('2d');
  if (!ctx) return canvas;
  ctx.scale(scale, scale);
  ctx.fillStyle = '#ffffff'; ctx.fillRect(0, 0, W, H);

  ctx.textAlign = 'center';
  ctx.fillStyle = '#0f172a'; ctx.font = '800 20px Segoe UI, system-ui, Arial';
  ctx.fillText(d.appName, W / 2, 34);
  ctx.fillStyle = '#64748b'; ctx.font = '400 11px Segoe UI, system-ui, Arial';
  ctx.fillText('Transaction Receipt', W / 2, 50);

  ctx.font = '700 12px Segoe UI, system-ui, Arial';
  const pillW = ctx.measureText(d.status).width + 26;
  const pillX = (W - pillW) / 2, pillY = 62, pillH = 22;
  roundRect(ctx, pillX, pillY, pillW, pillH, 11); ctx.fillStyle = statusColor(d.status); ctx.fill();
  ctx.fillStyle = '#ffffff'; ctx.fillText(d.status, W / 2, pillY + 15);

  let y = headerH;
  for (const ln of d.lines) {
    ctx.strokeStyle = '#e2e8f0'; ctx.setLineDash([2, 2]);
    ctx.beginPath(); ctx.moveTo(padX, y); ctx.lineTo(W - padX, y); ctx.stroke(); ctx.setLineDash([]);
    ctx.textAlign = 'left'; ctx.fillStyle = '#64748b'; ctx.font = '400 12.5px Segoe UI, system-ui, Arial';
    ctx.fillText(ln.label, padX, y + 20);
    ctx.textAlign = 'right'; ctx.fillStyle = '#1e293b'; ctx.font = '600 12.5px Segoe UI, system-ui, Arial';
    const labelW = (ctx.font = '400 12.5px Segoe UI, system-ui, Arial', ctx.measureText(ln.label).width);
    ctx.font = '600 12.5px Segoe UI, system-ui, Arial';
    ctx.fillText(fit(ctx, ln.value, W - padX * 2 - labelW - 16), W - padX, y + 20);
    y += rowH;
  }

  ctx.textAlign = 'center'; ctx.fillStyle = '#94a3b8'; ctx.font = '400 10.5px Segoe UI, system-ui, Arial';
  ctx.fillText('This is a system-generated receipt.', W / 2, y + 22);
  ctx.fillText(new Date().toLocaleString(), W / 2, y + 36);
  return canvas;
}

function canvasToBlob(canvas: HTMLCanvasElement): Promise<Blob | null> {
  return new Promise((res) => canvas.toBlob((b) => res(b), 'image/png'));
}

export async function downloadReceipt(row: Record<string, unknown>, brand: ReceiptBrand): Promise<boolean> {
  const blob = await canvasToBlob(renderCanvas(row, brand));
  if (!blob) return false;
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `receipt-${getReceiptData(row, brand).id}.png`;
  document.body.appendChild(a); a.click(); a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1500);
  return true;
}

export type ShareResult = { ok: true } | { ok: false; reason: 'unsupported' | 'cancelled' | 'error' };

export async function shareReceipt(row: Record<string, unknown>, brand: ReceiptBrand): Promise<ShareResult> {
  const blob = await canvasToBlob(renderCanvas(row, brand));
  if (!blob) return { ok: false, reason: 'error' };
  const d = getReceiptData(row, brand);
  const file = new File([blob], `receipt-${d.id}.png`, { type: 'image/png' });
  const nav = navigator as Navigator & { canShare?: (data?: unknown) => boolean };
  if (nav.share && nav.canShare && nav.canShare({ files: [file] })) {
    try {
      await nav.share({ files: [file], title: `${d.appName} receipt`, text: `${d.appName} transaction receipt · Txn ${d.id}` });
      return { ok: true };
    } catch (e) {
      return { ok: false, reason: (e as Error)?.name === 'AbortError' ? 'cancelled' : 'error' };
    }
  }
  return { ok: false, reason: 'unsupported' };
}
