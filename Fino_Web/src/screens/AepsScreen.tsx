import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Fingerprint, ShieldCheck, Banknote, Usb } from 'lucide-react';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

type TxnType = 'CW' | 'BE' | 'MS';

const TXN_TYPES: { value: TxnType; label: string }[] = [
  { value: 'CW', label: 'Cash Withdrawal' },
  { value: 'BE', label: 'Balance Enquiry' },
  { value: 'MS', label: 'Mini Statement' },
];

/**
 * AEPS (Aadhaar Enabled Payment System) — cash withdrawal screen. Mirrors legacy
 * UI/src/Pages/InnerPages/AEPS/AEPSIndex.js: pick the active AEPS provider/bank, enter the
 * customer's Aadhaar + mobile + amount, then capture a fingerprint via the device RD service.
 *
 * Web browsers cannot read a biometric scanner directly — the customer's fingerprint must be
 * captured through the bank's RD (Registered Device) service, so the on-screen capture step is
 * a designed placeholder until the RD bridge is wired. Bank list loads live from the backend.
 */
export function AepsScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const [bank, setBank] = useState('');
  const [aadhaar, setAadhaar] = useState('');
  const [mobile, setMobile] = useState('');
  const [amount, setAmount] = useState('');
  const [txnType, setTxnType] = useState<TxnType>('CW');
  const [captured, setCaptured] = useState(false);

  const { data: banks = [] } = useQuery({
    queryKey: ['aeps-banks'],
    queryFn: () => fetchList('AEPS/SelectActiveAEPS', { IsActive: true }),
  });

  const needsAmount = txnType === 'CW';

  const capture = () => {
    // Real capture requires the bank's RD service running on the operator's machine.
    toast.error('Connect your biometric (RD) device to capture the fingerprint.');
  };

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!bank) return toast.error('Select an AEPS bank / provider.');
    if (aadhaar.replace(/\s/g, '').length !== 12) return toast.error('Enter a valid 12-digit Aadhaar number.');
    if (mobile.length !== 10) return toast.error('Enter a valid 10-digit mobile number.');
    if (needsAmount && Number(amount) <= 0) return toast.error('Enter a valid amount.');
    if (!captured) return toast.error('Capture the customer fingerprint to proceed.');
    toast.success('Transaction submitted to the AEPS provider.');
  };

  return (
    <>
      <PageHeader title={def.title} subtitle={def.subtitle} icon={pickIcon('AEPS')} />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardBody>
            <form onSubmit={submit} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div className="sm:col-span-2 grid grid-cols-3 gap-2">
                {TXN_TYPES.map((t) => (
                  <button
                    key={t.value}
                    type="button"
                    onClick={() => setTxnType(t.value)}
                    className={`rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
                      txnType === t.value
                        ? 'border-brand-500 bg-brand-50 text-brand-700'
                        : 'border-slate-200 bg-white text-slate-600 hover:border-slate-300'
                    }`}
                  >
                    {t.label}
                  </button>
                ))}
              </div>

              <div className="sm:col-span-2">
                <Select label="AEPS Bank / Provider" value={bank} onChange={(e) => setBank(e.target.value)}>
                  <option value="">Select bank</option>
                  {banks.map((b, i) => {
                    const id = String(getField(b, 'IIN', 'BankID', 'AEPSID', 'ID') ?? i);
                    const name = String(getField(b, 'BankName', 'Name', 'ProviderName') ?? id);
                    return (
                      <option key={`${id}-${i}`} value={id}>
                        {name}
                      </option>
                    );
                  })}
                </Select>
              </div>

              <Input
                label="Aadhaar Number"
                inputMode="numeric"
                maxLength={12}
                placeholder="12-digit Aadhaar"
                value={aadhaar}
                onChange={(e) => setAadhaar(e.target.value.replace(/\D/g, ''))}
              />
              <Input
                label="Mobile Number"
                inputMode="numeric"
                maxLength={10}
                placeholder="10-digit mobile"
                value={mobile}
                onChange={(e) => setMobile(e.target.value.replace(/\D/g, ''))}
              />
              {needsAmount && (
                <Input
                  label="Amount (₹)"
                  type="number"
                  min={0}
                  placeholder="Amount to withdraw"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                />
              )}

              <div className="sm:col-span-2 flex flex-wrap items-center gap-3 pt-1">
                <Button type="button" variant={captured ? 'outline' : 'primary'} onClick={() => { capture(); setCaptured(false); }}>
                  <Fingerprint className="size-4" />
                  {captured ? 'Re-capture Fingerprint' : 'Capture Fingerprint'}
                </Button>
                <Button type="submit">
                  <Banknote className="size-4" />
                  {needsAmount ? 'Withdraw' : 'Submit'}
                </Button>
              </div>
            </form>
          </CardBody>
        </Card>

        <Card>
          <CardBody className="space-y-4">
            <div className="flex items-center gap-2 text-slate-700">
              <ShieldCheck className="size-5 text-brand-500" />
              <span className="text-sm font-semibold">Secure AEPS</span>
            </div>
            <p className="text-sm text-slate-500">
              AEPS lets a customer withdraw cash or check their balance using only their Aadhaar
              number and fingerprint — no card or PIN required.
            </p>
            <div className="flex items-start gap-2 rounded-lg bg-amber-50 p-3 text-xs text-amber-700">
              <Usb className="mt-0.5 size-4 shrink-0" />
              <span>
                A certified biometric scanner with its RD service running is required to capture the
                fingerprint. Plug in the device, then tap <b>Capture Fingerprint</b>.
              </span>
            </div>
          </CardBody>
        </Card>
      </div>
    </>
  );
}
