import { useEffect, useMemo, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, Power, ChevronRight, Upload, X, Building2, QrCode } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { uploadImage, imageUrl } from '@/api/catalog.api';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { Badge } from '@/components/ui/Badge';

type Row = Record<string, unknown>;

const num = (v: unknown) => (v === '' || v === undefined || v === null ? 0 : Number(v));
const str = (v: unknown) => String(v ?? '');

// ── Mode add/edit modal ──────────────────────────────────────────────────────
function ModeForm({ initial, onCancel, onSave, saving }: {
  initial?: Row; onCancel: () => void; onSave: (v: Row) => void; saving: boolean;
}) {
  const [name, setName] = useState(str(getField(initial ?? {}, 'ModeName')));
  const [order, setOrder] = useState(str(getField(initial ?? {}, 'DisplayOrder') ?? '0'));
  const [active, setActive] = useState(initial ? Boolean(getField(initial, 'IsActive')) : true);
  const [err, setErr] = useState('');

  const submit = () => {
    if (!name.trim()) { setErr('Mode name is required'); return; }
    onSave({ ModeName: name.trim(), DisplayOrder: num(order), IsActive: active });
  };
  return (
    <div className="space-y-4">
      <Input label={<>Mode name <span className="text-danger-500">*</span></>} value={name} error={err}
        placeholder="e.g. Bank, UPI, QR Payment" onChange={(e) => { setName(e.target.value); setErr(''); }} />
      <Input label="Display order" type="number" value={order} onChange={(e) => setOrder(e.target.value)} />
      <label className="flex cursor-pointer items-center gap-2.5 rounded-lg border border-slate-200 px-3 py-2.5">
        <input type="checkbox" checked={active} onChange={(e) => setActive(e.target.checked)}
          className="size-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
        <span className="text-sm font-medium text-slate-700">Active</span>
      </label>
      <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
        <Button variant="outline" onClick={onCancel} disabled={saving}>Cancel</Button>
        <Button onClick={submit} loading={saving}>{initial ? 'Save' : 'Add mode'}</Button>
      </div>
    </div>
  );
}

// ── Sub Mode add/edit modal (with bank + QR details) ─────────────────────────
function SubModeForm({ modeName, initial, onCancel, onSave, saving }: {
  modeName: string; initial?: Row; onCancel: () => void; onSave: (v: Row) => void; saving: boolean;
}) {
  const toast = useToast();
  const g = (k: string, d = '') => str(getField(initial ?? {}, k) ?? d);
  const [f, setF] = useState({
    SubModeName: g('SubModeName'),
    Balance: g('Balance', '0'),
    DisplayOrder: g('DisplayOrder', '0'),
    BankName: g('BankName'),
    IFSC: g('IFSC'),
    BeneficiaryName: g('BeneficiaryName'),
    AccountNumber: g('AccountNumber'),
    QRCode: g('QRCode'),
    QRImage: g('QRImage'),
  });
  const [active, setActive] = useState(initial ? Boolean(getField(initial, 'IsActive')) : true);
  const [err, setErr] = useState('');
  const [uploading, setUploading] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);
  const set = (k: string, v: string) => setF((s) => ({ ...s, [k]: v }));

  const pickImage = async (file?: File) => {
    if (!file) return;
    setUploading(true);
    try {
      const path = await uploadImage(file, 'misc');
      set('QRImage', path);
      toast.success('QR image uploaded.');
    } catch (e) { toast.error((e as Error).message); }
    finally { setUploading(false); }
  };

  const submit = () => {
    if (!f.SubModeName.trim()) { setErr('Sub mode name is required'); return; }
    onSave({
      SubModeName: f.SubModeName.trim(), Balance: num(f.Balance), DisplayOrder: num(f.DisplayOrder),
      BankName: f.BankName.trim(), IFSC: f.IFSC.trim(), BeneficiaryName: f.BeneficiaryName.trim(),
      AccountNumber: f.AccountNumber.trim(), QRCode: f.QRCode.trim(), QRImage: f.QRImage,
      IsActive: active,
    });
  };

  return (
    <div className="space-y-5">
      <p className="text-xs text-slate-500">Under mode <span className="font-semibold text-slate-700">{modeName}</span></p>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Input label={<>Sub mode name <span className="text-danger-500">*</span></>} value={f.SubModeName} error={err}
          placeholder="e.g. HDFC Bank, PhonePe QR" onChange={(e) => { set('SubModeName', e.target.value); setErr(''); }} />
        <Input label="Balance (₹)" type="number" value={f.Balance} onChange={(e) => set('Balance', e.target.value)} />
      </div>

      {/* Bank details */}
      <div className="rounded-xl border border-slate-200 p-4">
        <div className="mb-3 flex items-center gap-2 text-sm font-semibold text-slate-700">
          <Building2 className="size-4 text-brand-600" /> Bank / account details
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input label="Bank name" value={f.BankName} onChange={(e) => set('BankName', e.target.value)} />
          <Input label="IFSC code" value={f.IFSC} onChange={(e) => set('IFSC', e.target.value.toUpperCase())} />
          <Input label="Beneficiary name" value={f.BeneficiaryName} onChange={(e) => set('BeneficiaryName', e.target.value)} />
          <Input label="Account number" value={f.AccountNumber} onChange={(e) => set('AccountNumber', e.target.value)} />
        </div>
      </div>

      {/* QR details */}
      <div className="rounded-xl border border-slate-200 p-4">
        <div className="mb-3 flex items-center gap-2 text-sm font-semibold text-slate-700">
          <QrCode className="size-4 text-brand-600" /> QR / UPI details
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input label="QR code / UPI ID" value={f.QRCode} placeholder="name@upi" onChange={(e) => set('QRCode', e.target.value)} />
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700">QR image</label>
            <div className="flex items-center gap-3">
              {f.QRImage ? (
                <div className="relative">
                  <img src={imageUrl(f.QRImage)} alt="QR" className="size-16 rounded-lg border border-slate-200 object-cover" />
                  <button type="button" onClick={() => set('QRImage', '')}
                    className="absolute -right-1.5 -top-1.5 rounded-full bg-danger-500 p-0.5 text-white shadow"><X className="size-3" /></button>
                </div>
              ) : (
                <div className="flex size-16 items-center justify-center rounded-lg border border-dashed border-slate-300 text-slate-300"><QrCode className="size-6" /></div>
              )}
              <Button type="button" variant="outline" size="sm" loading={uploading} onClick={() => fileRef.current?.click()}>
                <Upload className="size-4" /> Upload
              </Button>
              <input ref={fileRef} type="file" accept="image/*" className="hidden"
                onChange={(e) => pickImage(e.target.files?.[0])} />
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Input label="Display order" type="number" value={f.DisplayOrder} onChange={(e) => set('DisplayOrder', e.target.value)} />
        <label className="flex cursor-pointer items-center gap-2.5 rounded-lg border border-slate-200 px-3 py-2.5">
          <input type="checkbox" checked={active} onChange={(e) => setActive(e.target.checked)}
            className="size-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
          <span className="text-sm font-medium text-slate-700">Active</span>
        </label>
      </div>

      <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
        <Button variant="outline" onClick={onCancel} disabled={saving}>Cancel</Button>
        <Button onClick={submit} loading={saving}>{initial ? 'Save' : 'Add sub mode'}</Button>
      </div>
    </div>
  );
}

// ── Main screen ──────────────────────────────────────────────────────────────
export function ModeMasterScreen() {
  const toast = useToast();
  const qc = useQueryClient();
  const [selectedMode, setSelectedMode] = useState<number | null>(null);
  const [modeModal, setModeModal] = useState<{ open: boolean; row?: Row }>({ open: false });
  const [subModal, setSubModal] = useState<{ open: boolean; row?: Row }>({ open: false });

  const { data: modes = [] } = useQuery({ queryKey: ['modes-all'], queryFn: () => fetchList('Mode/GetAll', {}) });
  const { data: subs = [] } = useQuery({ queryKey: ['submodes-all'], queryFn: () => fetchList('SubMode/GetAll', {}) });

  // Auto-select the first mode once loaded.
  useEffect(() => {
    if (selectedMode === null && modes.length > 0) setSelectedMode(Number(getField(modes[0], 'ModeID')));
  }, [modes, selectedMode]);

  const currentMode = useMemo(() => modes.find((m) => Number(getField(m, 'ModeID')) === selectedMode), [modes, selectedMode]);
  const currentModeName = str(getField(currentMode ?? {}, 'ModeName'));
  const modeSubs = useMemo(
    () => subs.filter((s) => Number(getField(s, 'ModeID')) === selectedMode)
      .sort((a, b) => num(getField(a, 'DisplayOrder')) - num(getField(b, 'DisplayOrder'))),
    [subs, selectedMode],
  );

  const refreshModes = () => qc.invalidateQueries({ queryKey: ['modes-all'] });
  const refreshSubs = () => qc.invalidateQueries({ queryKey: ['submodes-all'] });

  // ── Mode mutations ──
  const saveMode = useMutation({
    mutationFn: (v: Row) => modeModal.row
      ? apiPost('Mode/Update', { ModeID: getField(modeModal.row, 'ModeID'), ...v })
      : apiPost('Mode/Create', v),
    onSuccess: (r) => { if (r.ok) { toast.success('Mode saved.'); setModeModal({ open: false }); refreshModes(); } else toast.error(r.message); },
    onError: (e: Error) => toast.error(e.message),
  });
  const toggleMode = useMutation({
    mutationFn: (row: Row) => apiPost('Mode/Update', {
      ModeID: getField(row, 'ModeID'), ModeName: getField(row, 'ModeName'),
      DisplayOrder: getField(row, 'DisplayOrder'), IsActive: !getField(row, 'IsActive'),
    }),
    onSuccess: (r) => { if (r.ok) { toast.success('Status updated.'); refreshModes(); } else toast.error(r.message); },
  });
  const deleteMode = useMutation({
    mutationFn: (id: unknown) => apiPost('Mode/Delete', { ModeID: id }),
    onSuccess: (r) => { if (r.ok) { toast.success('Mode deleted.'); refreshModes(); } else toast.error(r.message); },
  });

  // ── SubMode mutations ──
  const saveSub = useMutation({
    mutationFn: (v: Row) => subModal.row
      ? apiPost('SubMode/Update', { SubModeID: getField(subModal.row, 'SubModeID'), ModeID: selectedMode, ...v })
      : apiPost('SubMode/Create', { ModeID: selectedMode, ...v }),
    onSuccess: (r) => { if (r.ok) { toast.success('Sub mode saved.'); setSubModal({ open: false }); refreshSubs(); } else toast.error(r.message); },
    onError: (e: Error) => toast.error(e.message),
  });
  const toggleSub = useMutation({
    mutationFn: (row: Row) => apiPost('SubMode/Update', {
      SubModeID: getField(row, 'SubModeID'), ModeID: getField(row, 'ModeID'), SubModeName: getField(row, 'SubModeName'),
      Balance: getField(row, 'Balance'), BankName: getField(row, 'BankName'), IFSC: getField(row, 'IFSC'),
      BeneficiaryName: getField(row, 'BeneficiaryName'), AccountNumber: getField(row, 'AccountNumber'),
      QRCode: getField(row, 'QRCode'), QRImage: getField(row, 'QRImage'),
      DisplayOrder: getField(row, 'DisplayOrder'), IsActive: !getField(row, 'IsActive'),
    }),
    onSuccess: (r) => { if (r.ok) { toast.success('Status updated.'); refreshSubs(); } else toast.error(r.message); },
  });
  const deleteSub = useMutation({
    mutationFn: (id: unknown) => apiPost('SubMode/Delete', { SubModeID: id }),
    onSuccess: (r) => { if (r.ok) { toast.success('Sub mode deleted.'); refreshSubs(); } else toast.error(r.message); },
  });

  return (
    <div className="space-y-5">
      <PageHeader title="Modes & Sub Modes" subtitle="Fund-request transfer methods and their sources / destinations" />
{/* 
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatTile label="Modes" value={String(modes.length)} icon={Layers} tone="bg-brand-50 text-brand-600" />
        <StatTile label="Active modes" value={String(activeModes)} icon={Power} tone="bg-success-50 text-success-600" />
        <StatTile label="Sub modes" value={String(subs.length)} icon={ChevronRight} tone="bg-slate-100 text-slate-600" />
      </div> */}

      <div className="grid grid-cols-1 gap-5 lg:grid-cols-[320px_1fr]">
        {/* Modes list */}
        <div className="rounded-card border border-slate-200 bg-white">
          <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
            <h3 className="text-sm font-semibold text-slate-700">Modes</h3>
            <Button size="sm" onClick={() => setModeModal({ open: true })}><Plus className="size-4" /> Add</Button>
          </div>
          <ul className="max-h-[560px] overflow-y-auto p-2">
            {modes.map((m) => {
              const id = Number(getField(m, 'ModeID'));
              const active = Boolean(getField(m, 'IsActive'));
              const count = subs.filter((s) => Number(getField(s, 'ModeID')) === id).length;
              return (
                <li key={id}>
                  <button onClick={() => setSelectedMode(id)}
                    className={cn('group flex w-full items-center gap-2 rounded-lg px-3 py-2.5 text-left transition-colors',
                      selectedMode === id ? 'bg-brand-50 ring-1 ring-brand-200' : 'hover:bg-slate-50')}>
                    <span className={cn('flex-1 truncate text-sm font-medium', active ? 'text-slate-800' : 'text-slate-400 line-through')}>
                      {str(getField(m, 'ModeName'))}
                    </span>
                    <span className="rounded-full bg-slate-100 px-1.5 py-0.5 text-[11px] font-semibold text-slate-500">{count}</span>
                    <ChevronRight className={cn('size-4', selectedMode === id ? 'text-brand-500' : 'text-slate-300')} />
                  </button>
                </li>
              );
            })}
            {modes.length === 0 && <li className="px-3 py-6 text-center text-sm text-slate-400">No modes yet.</li>}
          </ul>
        </div>

        {/* Sub modes of selected mode */}
        <div className="rounded-card border border-slate-200 bg-white">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-slate-100 px-4 py-3">
            <div>
              <h3 className="text-sm font-semibold text-slate-700">{currentModeName ? `Sub modes · ${currentModeName}` : 'Sub modes'}</h3>
              <p className="text-xs text-slate-400">Sources / destinations under this mode</p>
            </div>
            <div className="flex flex-wrap gap-2">
              {currentMode && (
                <>
                  <Button size="sm" variant="outline" onClick={() => toggleMode.mutate(currentMode)}><Power className="size-4" /> {getField(currentMode, 'IsActive') ? 'Disable' : 'Enable'}</Button>
                  <Button size="sm" variant="outline" onClick={() => setModeModal({ open: true, row: currentMode })}><Pencil className="size-4" /> Edit</Button>
                  <Button size="sm" variant="outline" onClick={() => { if (confirm(`Delete mode “${currentModeName}”? Its sub modes must be removed first.`)) deleteMode.mutate(selectedMode); }}><Trash2 className="size-4" /> Delete</Button>
                  <Button size="sm" onClick={() => setSubModal({ open: true })} disabled={!selectedMode}><Plus className="size-4" /> Add sub mode</Button>
                </>
              )}
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                  <th className="px-4 py-2.5 font-semibold">Sub mode</th>
                  <th className="px-4 py-2.5 font-semibold">Details</th>
                  <th className="px-4 py-2.5 font-semibold">Balance</th>
                  <th className="px-4 py-2.5 font-semibold">Status</th>
                  <th className="px-4 py-2.5 text-right font-semibold">Actions</th>
                </tr>
              </thead>
              <tbody>
                {modeSubs.map((s) => {
                  const id = Number(getField(s, 'SubModeID'));
                  const active = Boolean(getField(s, 'IsActive'));
                  const bank = str(getField(s, 'BankName'));
                  const acc = str(getField(s, 'AccountNumber'));
                  const ifsc = str(getField(s, 'IFSC'));
                  const qr = str(getField(s, 'QRCode'));
                  const qrImg = str(getField(s, 'QRImage'));
                  return (
                    <tr key={id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50/60">
                      <td className="px-4 py-3 font-medium text-slate-800">{str(getField(s, 'SubModeName'))}</td>
                      <td className="px-4 py-3 text-xs text-slate-500">
                        <div className="flex items-center gap-2">
                          {qrImg && <img src={imageUrl(qrImg)} alt="QR" className="size-8 rounded border border-slate-200 object-cover" />}
                          <div>
                            {bank && <div>{bank}{ifsc ? ` · ${ifsc}` : ''}</div>}
                            {acc && <div>A/C {acc}</div>}
                            {qr && <div>{qr}</div>}
                            {!bank && !acc && !qr && !qrImg && <span className="text-slate-300">—</span>}
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3 tabular-nums text-slate-600">₹{num(getField(s, 'Balance')).toFixed(2)}</td>
                      <td className="px-4 py-3">
                        <Badge tone={active ? 'success' : 'neutral'}>{active ? 'Active' : 'Inactive'}</Badge>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-1">
                          <button title="Toggle active" onClick={() => toggleSub.mutate(s)} className="rounded-md p-1.5 text-slate-400 hover:bg-slate-100 hover:text-brand-600"><Power className="size-4" /></button>
                          <button title="Edit" onClick={() => setSubModal({ open: true, row: s })} className="rounded-md p-1.5 text-slate-400 hover:bg-slate-100 hover:text-brand-600"><Pencil className="size-4" /></button>
                          <button title="Delete" onClick={() => { if (confirm('Delete this sub mode?')) deleteSub.mutate(id); }} className="rounded-md p-1.5 text-slate-400 hover:bg-slate-100 hover:text-danger-600"><Trash2 className="size-4" /></button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
                {modeSubs.length === 0 && (
                  <tr><td colSpan={5} className="px-4 py-10 text-center text-sm text-slate-400">
                    {currentMode ? 'No sub modes yet. Click “Add sub mode”.' : 'Select a mode to manage its sub modes.'}
                  </td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Mode modal */}
      <Modal open={modeModal.open} onClose={() => setModeModal({ open: false })} title={modeModal.row ? 'Edit mode' : 'Add mode'} size="sm">
        <ModeForm initial={modeModal.row} saving={saveMode.isPending} onCancel={() => setModeModal({ open: false })}
          onSave={(v) => saveMode.mutate(v)} />
      </Modal>

      {/* Sub mode modal */}
      <Modal open={subModal.open} onClose={() => setSubModal({ open: false })} title={subModal.row ? 'Edit sub mode' : 'Add sub mode'} size="lg">
        <SubModeForm modeName={currentModeName} initial={subModal.row} saving={saveSub.isPending}
          onCancel={() => setSubModal({ open: false })} onSave={(v) => saveSub.mutate(v)} />
      </Modal>
    </div>
  );
}
