import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Banknote,
  CheckCircle2,
  Clock3,
  Landmark,
  Plus,
  RotateCcw,
  Search,
  Send,
  ShieldCheck,
  Trash2,
  UserPlus,
  XCircle,
} from 'lucide-react';
import {
  addBeneficiary,
  addSender,
  checkBranch,
  deleteBeneficiary,
  fetchDmtBanks,
  fetchRecentTransactions,
  moneyTransfer,
  resendBeneficiaryOtp,
  resendSenderOtp,
  selectBeneficiaries,
  selectSender,
  verifyBeneficiaryAccount,
  verifyBeneficiaryOtp,
  verifySenderOtp,
  type BeneficiaryDetail,
  type DmtBankOption,
  type PaymentType,
  type SenderDetails,
  type TransferResult,
  DMT_SERVICE_ID,
} from '@/api/dmt.api';
import { getField } from '@/lib/row';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useIsMobileApp } from '@/lib/native';
import { txnWhen } from '@/lib/txn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Autocomplete, type AutocompleteOption } from '@/components/ui/Autocomplete';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { StatusBadge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';

/** Where the sender lookup left us — drives which panel the page shows. */
type Stage =
  | { kind: 'idle' }
  | { kind: 'register'; mobile: string } // not registered → show the registration form
  | { kind: 'verify-sender'; mobile: string } // registered, OTP pending → verify to continue
  | { kind: 'ready'; sender: SenderDetails }; // verified → beneficiaries + transfer

export default function DmtPage() {
  const toast = useToast();
  const qc = useQueryClient();

  const [mobile, setMobile] = useState('');
  const [stage, setStage] = useState<Stage>({ kind: 'idle' });

  const recentQ = useQuery({ queryKey: ['dmt', 'recent'], queryFn: () => fetchRecentTransactions(15) });

  // --- Sender lookup ---------------------------------------------------------
  const lookupMut = useMutation({
    mutationFn: async (m: string) => selectSender(m),
    onSuccess: (res, m) => {
      if (res.ok && res.data) {
        setStage({ kind: 'ready', sender: res.data });
        return;
      }
      const msg = (res.message || '').toLowerCase();
      if (msg.includes('not registered')) {
        setStage({ kind: 'register', mobile: m });
      } else if (msg.includes('otp not verified')) {
        setStage({ kind: 'verify-sender', mobile: m });
        toast.info('This sender is registered but not verified. Enter the OTP to continue.');
      } else {
        toast.error(res.message || 'Sender lookup failed.');
        setStage({ kind: 'idle' });
      }
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const search = () => {
    if (mobile.length !== 10) return toast.error('Enter a valid 10-digit mobile number.');
    setStage({ kind: 'idle' });
    lookupMut.mutate(mobile);
  };

  const reset = () => {
    setMobile('');
    setStage({ kind: 'idle' });
  };

  return (
    <>
      <PageHeader title="Money Transfer" subtitle="Domestic Money Transfer — send to any bank account" />

      {/* Sender search — the entry point for the whole flow. Once a sender is loaded, the
          sender-details strip below replaces this card entirely; "Change sender" there is
          what comes back to it. */}
      {stage.kind !== 'ready' && (
      <Card className="mb-4">
        <CardBody>
          <form
            className="flex flex-col gap-3 sm:flex-row sm:items-end"
            onSubmit={(e) => {
              e.preventDefault();
              search();
            }}
          >
            <div className="flex-1">
              <Input
                label="Sender mobile number"
                placeholder="10-digit mobile number"
                leftIcon={<Search className="size-4" />}
                inputMode="numeric"
                maxLength={10}
                value={mobile}
                onChange={(e) => setMobile(e.target.value.replace(/\D/g, ''))}
              />
            </div>
            <div className="flex gap-2 [&>button]:flex-1 md:[&>button]:flex-none">
              <Button type="submit" loading={lookupMut.isPending} disabled={mobile.length !== 10}>
                <Search className="size-4" />
                Search
              </Button>
              {stage.kind !== 'idle' && (
                <Button type="button" variant="outline" onClick={reset}>
                  <RotateCcw className="size-4" />
                  Reset
                </Button>
              )}
            </div>
          </form>
          <p className="mt-2 text-xs text-slate-400">
            Enter the customer's mobile number to check their DMT registration.
          </p>
        </CardBody>
      </Card>
      )}

      {/* Stage-specific panel */}
      {stage.kind === 'register' && (
        <RegisterSender
          mobile={stage.mobile}
          onRegistered={(needsOtp) =>
            setStage(needsOtp ? { kind: 'verify-sender', mobile: stage.mobile } : { kind: 'idle' })
          }
          onVerified={() => lookupMut.mutate(stage.mobile)}
        />
      )}

      {stage.kind === 'verify-sender' && (
        <VerifySender mobile={stage.mobile} onVerified={() => lookupMut.mutate(stage.mobile)} />
      )}

      {stage.kind === 'ready' && (
        <SenderWorkspace
          sender={stage.sender}
          onChangeSender={reset}
          onTransferDone={() => qc.invalidateQueries({ queryKey: ['dmt', 'recent'] })}
        />
      )}

      {/* Recent DMT transactions */}
      <Card className="mt-4">
        <CardHeader>
          <CardTitle>Recent transfers</CardTitle>
        </CardHeader>
        <CardBody className="p-0">
          <RecentTransfers rows={recentQ.data ?? []} loading={recentQ.isFetching} />
        </CardBody>
      </Card>
    </>
  );
}

/* -------------------------------------------------------------------------- */
/* Sender registration                                                         */
/* -------------------------------------------------------------------------- */

function RegisterSender({
  mobile,
  onRegistered,
  onVerified,
}: {
  mobile: string;
  onRegistered: (needsOtp: boolean) => void;
  onVerified: () => void;
}) {
  const toast = useToast();
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [pinCode, setPinCode] = useState('');
  const [otpFor, setOtpFor] = useState<string | null>(null);

  const addMut = useMutation({
    mutationFn: async () => {
      if (!name.trim()) throw new Error('Enter the sender name.');
      if (!address.trim()) throw new Error('Enter the sender address.');
      return addSender({ name: name.trim(), mobile, address: address.trim(), pinCode });
    },
    onSuccess: (res) => {
      if (!res.ok) return toast.error(res.message || 'Registration failed.');
      const marker = String(res.data ?? '');
      if (marker.toUpperCase().includes('VERIFY_OTP')) {
        setOtpFor(mobile);
        onRegistered(true);
        toast.info('OTP sent to the sender. Enter it to complete registration.');
      } else {
        toast.success(marker || 'Sender registered.');
        onRegistered(false);
        onVerified();
      }
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>Register new sender</CardTitle>
        </CardHeader>
        <CardBody>
          <p className="mb-4 text-sm text-slate-500">
            No sender is registered against <span className="font-semibold text-slate-700">{mobile}</span>. Add the
            details below to register.
          </p>
          <form
            className="grid grid-cols-1 gap-4 sm:grid-cols-2"
            onSubmit={(e) => {
              e.preventDefault();
              addMut.mutate();
            }}
          >
            <Input label="Sender name" placeholder="Full name" value={name} onChange={(e) => setName(e.target.value)} />
            <Input label="Mobile number" value={mobile} readOnly disabled />
            <Input
              label="Pincode"
              placeholder="6-digit pincode"
              inputMode="numeric"
              maxLength={6}
              value={pinCode}
              onChange={(e) => setPinCode(e.target.value.replace(/\D/g, ''))}
            />
            <Input
              label="Address"
              placeholder="Sender address"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
            />
            <div className="sm:col-span-2 flex justify-end [&>button]:w-full md:[&>button]:w-auto">
              <Button type="submit" loading={addMut.isPending}>
                <UserPlus className="size-4" />
                Register sender
              </Button>
            </div>
          </form>
        </CardBody>
      </Card>

      {otpFor && (
        <OtpModal
          title="Verify sender"
          subtitle={`Enter the OTP sent to ${otpFor}`}
          onResend={() => resendSenderOtp(otpFor)}
          onSubmit={(otp) => verifySenderOtp(otpFor, otp)}
          onClose={() => setOtpFor(null)}
          onVerified={() => {
            setOtpFor(null);
            toast.success('Sender verified.');
            onVerified();
          }}
        />
      )}
    </>
  );
}

/** Sender exists but was never OTP-verified — verify it straight away. */
function VerifySender({ mobile, onVerified }: { mobile: string; onVerified: () => void }) {
  const toast = useToast();
  return (
    <Card>
      <CardHeader>
        <CardTitle>Verify sender</CardTitle>
      </CardHeader>
      <CardBody>
        <OtpInline
          subtitle={`Enter the OTP sent to ${mobile}`}
          onResend={() => resendSenderOtp(mobile)}
          onSubmit={(otp) => verifySenderOtp(mobile, otp)}
          onVerified={() => {
            toast.success('Sender verified.');
            onVerified();
          }}
        />
      </CardBody>
    </Card>
  );
}

/* -------------------------------------------------------------------------- */
/* Verified-sender workspace: info strip + beneficiaries + add + transfer       */
/* -------------------------------------------------------------------------- */

function SenderWorkspace({
  sender,
  onChangeSender,
  onTransferDone,
}: {
  sender: SenderDetails;
  onChangeSender: () => void;
  onTransferDone: () => void;
}) {
  const isMobile = useIsMobileApp();
  const toast = useToast();
  const qc = useQueryClient();
  const [showAdd, setShowAdd] = useState(false);
  const [transferFor, setTransferFor] = useState<BeneficiaryDetail | null>(null);
  const [deleteFor, setDeleteFor] = useState<BeneficiaryDetail | null>(null);
  const [verifyFor, setVerifyFor] = useState<BeneficiaryDetail | null>(null);
  const [result, setResult] = useState<TransferResult | null>(null);

  const beneQ = useQuery({
    queryKey: ['dmt', 'bene', sender.SenderGUID],
    queryFn: async () => {
      const res = await selectBeneficiaries(sender.SenderGUID);
      return res.ok && Array.isArray(res.data) ? res.data : [];
    },
  });
  const beneficiaries = beneQ.data ?? [];

  const delMut = useMutation({
    mutationFn: async (b: BeneficiaryDetail) => {
      const res = await deleteBeneficiary(b.BeneficiaryGUID);
      if (!res.ok) throw new Error(res.message || 'Delete failed.');
      return res;
    },
    onSuccess: () => {
      toast.success('Beneficiary removed.');
      setDeleteFor(null);
      qc.invalidateQueries({ queryKey: ['dmt', 'bene', sender.SenderGUID] });
    },
    onError: (e: Error) => {
      toast.error(e.message);
      setDeleteFor(null);
    },
  });

  // Kick off verification: dispatch a fresh OTP to the sender, then open the OTP modal.
  const startVerifyMut = useMutation({
    mutationFn: async (b: BeneficiaryDetail) => {
      const res = await resendBeneficiaryOtp(b.BeneficiaryGUID);
      if (!res.ok) throw new Error(res.message || 'Could not send OTP.');
      return b;
    },
    onSuccess: (b) => {
      setVerifyFor(b);
      toast.info(`OTP sent to ${sender.Mobile}. Enter it to verify the beneficiary.`);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      {/* Sender info strip — flat, plain, no colourful tiles */}
      <Card className="mb-4">
        <CardBody className="flex flex-wrap items-center gap-x-8 gap-y-3">
          <InfoField label="Sender" value={sender.Name} />
          <InfoField label="Mobile" value={sender.Mobile} mono />
          <InfoField label="Remaining limit" value={inr(sender.RemainingLimit)} mono />
          {sender.DisplayLimit > 0 && (
            <InfoField label="Max per transfer" value={inr(sender.DisplayLimit)} mono />
          )}
          <InfoField label="Consumed" value={inr(sender.Consumed)} mono />
          <div className="ml-auto flex items-center gap-2">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-success-50 px-3 py-1 text-xs font-semibold text-success-700">
              <ShieldCheck className="size-3.5" />
              Verified sender
            </span>
            {/* The only way back to the lookup form, which is hidden while a sender is loaded. */}
            <Button size="sm" variant="outline" onClick={onChangeSender}>
              <RotateCcw className="size-4" />
              Change sender
            </Button>
          </div>
        </CardBody>
      </Card>

      {/* Beneficiaries */}
      <Card>
        <CardHeader className="flex items-center justify-between">
          <CardTitle>Beneficiaries</CardTitle>
          <Button size="sm" onClick={() => setShowAdd(true)}>
            <Plus className="size-4" />
            Add beneficiary
          </Button>
        </CardHeader>
        <CardBody className="p-0">
          {/* Beneficiaries carry the page's main action (Send), so on a phone they become
              cards with the action attached rather than a seven-column scroll. */}
          {isMobile ? (
            beneQ.isFetching && beneficiaries.length === 0 ? (
              <p className="px-4 py-8 text-center text-sm text-slate-400">Loading…</p>
            ) : beneficiaries.length === 0 ? (
              <EmptyState title="No beneficiaries yet" description="Add a beneficiary to start sending money." />
            ) : (
              beneficiaries.map((b, i) => (
                <div
                  key={b.BeneficiaryGUID}
                  className={cn('px-4 py-3', i > 0 && 'border-t border-slate-100')}
                >
                  <div className="flex items-start justify-between gap-3">
                    <span className="min-w-0">
                      <span className="block truncate text-sm font-semibold text-slate-800">{b.Name}</span>
                      <span className="block truncate text-xs text-slate-400">{b.BankName || '—'}</span>
                      <span className="block truncate text-xs tabular-nums text-slate-500">
                        {[b.AccountNo, b.IFSC].filter(Boolean).join(' · ')}
                      </span>
                    </span>
                    {b.IsVerified && (
                      <span className="inline-flex shrink-0 items-center gap-1 rounded-full bg-success-50 px-2.5 py-0.5 text-[11px] font-semibold text-success-700">
                        <ShieldCheck className="size-3.5" />
                        Verified
                      </span>
                    )}
                  </div>

                  <div className="mt-3 flex items-center gap-2">
                    {b.IsVerified ? (
                      <Button size="sm" className="flex-1" onClick={() => setTransferFor(b)}>
                        <Send className="size-4" />
                        Send money
                      </Button>
                    ) : (
                      <Button
                        size="sm"
                        variant="outline"
                        className="flex-1"
                        loading={startVerifyMut.isPending && startVerifyMut.variables?.BeneficiaryGUID === b.BeneficiaryGUID}
                        onClick={() => startVerifyMut.mutate(b)}
                      >
                        <ShieldCheck className="size-4" />
                        Verify
                      </Button>
                    )}
                    <Button
                      size="sm"
                      variant="ghost"
                      aria-label="Remove beneficiary"
                      className="shrink-0 text-danger-600 hover:bg-danger-50"
                      onClick={() => setDeleteFor(b)}
                    >
                      <Trash2 className="size-4" />
                    </Button>
                  </div>
                </div>
              ))
            )
          ) : (
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                  <th className="px-4 py-2.5">#</th>
                  <th className="px-4 py-2.5">Name</th>
                  <th className="px-4 py-2.5">Bank</th>
                  <th className="px-4 py-2.5">Account number</th>
                  <th className="px-4 py-2.5">IFSC</th>
                  <th className="px-4 py-2.5">Status</th>
                  <th className="px-4 py-2.5 text-right">Action</th>
                </tr>
              </thead>
              <tbody>
                {beneQ.isFetching && beneficiaries.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-4 py-8 text-center text-sm text-slate-400">
                      Loading…
                    </td>
                  </tr>
                ) : beneficiaries.length === 0 ? (
                  <tr>
                    <td colSpan={7}>
                      <EmptyState
                        title="No beneficiaries yet"
                        description="Add a beneficiary to start sending money."
                      />
                    </td>
                  </tr>
                ) : (
                  beneficiaries.map((b, i) => (
                    <tr key={b.BeneficiaryGUID} className="border-b border-slate-100 last:border-0 even:bg-slate-50/40">
                      <td className="px-4 py-2.5 text-slate-500">{i + 1}</td>
                      <td className="px-4 py-2.5 font-medium text-slate-800">{b.Name}</td>
                      <td className="px-4 py-2.5 text-slate-600">{b.BankName || '—'}</td>
                      <td className="px-4 py-2.5 text-slate-700 tabular-nums">{b.AccountNo}</td>
                      <td className="px-4 py-2.5 text-slate-600 tabular-nums">{b.IFSC}</td>
                      <td className="px-4 py-2.5">
                        {b.IsVerified ? (
                          <span className="inline-flex items-center gap-1 rounded-full bg-success-50 px-2.5 py-0.5 text-xs font-semibold text-success-700">
                            <ShieldCheck className="size-3.5" />
                            Verified
                          </span>
                        ) : (
                          <Button
                            size="sm"
                            variant="outline"
                            loading={startVerifyMut.isPending && startVerifyMut.variables?.BeneficiaryGUID === b.BeneficiaryGUID}
                            onClick={() => startVerifyMut.mutate(b)}
                          >
                            <ShieldCheck className="size-4" />
                            Verify
                          </Button>
                        )}
                      </td>
                      <td className="px-4 py-2.5">
                        <div className="flex items-center justify-end gap-2">
                          <Button size="sm" onClick={() => setTransferFor(b)} disabled={!b.IsVerified}>
                            <Send className="size-4" />
                            Send
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            className="text-danger-600 hover:bg-danger-50"
                            onClick={() => setDeleteFor(b)}
                          >
                            <Trash2 className="size-4" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
          )}
        </CardBody>
      </Card>

      {showAdd && (
        <AddBeneficiaryModal
          sender={sender}
          onClose={() => setShowAdd(false)}
          onAdded={() => {
            setShowAdd(false);
            qc.invalidateQueries({ queryKey: ['dmt', 'bene', sender.SenderGUID] });
          }}
        />
      )}

      {transferFor && (
        <TransferModal
          sender={sender}
          beneficiary={transferFor}
          onClose={() => setTransferFor(null)}
          onDone={(res) => {
            setTransferFor(null);
            setResult(res);
            onTransferDone();
            qc.invalidateQueries({ queryKey: ['my-wallets'] });
          }}
        />
      )}

      {deleteFor && (
        <Modal open onClose={() => setDeleteFor(null)} title="Remove beneficiary" size="sm">
          <p className="text-sm text-slate-600">
            Remove <span className="font-semibold text-slate-800">{deleteFor.Name}</span> ({deleteFor.AccountNo}) from
            the beneficiary list?
          </p>
          <div className="mt-5 flex justify-end gap-2">
            <Button variant="outline" onClick={() => setDeleteFor(null)}>
              Cancel
            </Button>
            <Button variant="danger" loading={delMut.isPending} onClick={() => delMut.mutate(deleteFor)}>
              <Trash2 className="size-4" />
              Remove
            </Button>
          </div>
        </Modal>
      )}

      {verifyFor && (
        <OtpModal
          title="Verify beneficiary"
          subtitle={`Enter the OTP sent to ${sender.Mobile}`}
          onResend={() => resendBeneficiaryOtp(verifyFor.BeneficiaryGUID)}
          onSubmit={(otp) => verifyBeneficiaryOtp(verifyFor.BeneficiaryGUID, otp)}
          onClose={() => setVerifyFor(null)}
          onVerified={() => {
            setVerifyFor(null);
            toast.success('Beneficiary verified.');
            qc.invalidateQueries({ queryKey: ['dmt', 'bene', sender.SenderGUID] });
          }}
        />
      )}

      {result && <TransferResultModal result={result} onClose={() => setResult(null)} />}
    </>
  );
}

function InfoField({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div>
      <p className="text-xs font-medium uppercase tracking-wide text-slate-400">{label}</p>
      <p className={cn('mt-0.5 text-sm font-semibold text-slate-800', mono && 'tabular-nums')}>{value || '—'}</p>
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/* Add beneficiary                                                             */
/* -------------------------------------------------------------------------- */

function AddBeneficiaryModal({
  sender,
  onClose,
  onAdded,
}: {
  sender: SenderDetails;
  onClose: () => void;
  onAdded: () => void;
}) {
  const toast = useToast();
  const [name, setName] = useState('');
  const [bankId, setBankId] = useState('');
  const [ifsc, setIfsc] = useState('');
  const [bankName, setBankName] = useState('');
  const [branch, setBranch] = useState('');
  const [accountNo, setAccountNo] = useState('');
  const [confirmAccountNo, setConfirmAccountNo] = useState('');
  const [otpFor, setOtpFor] = useState<string | null>(null);
  // Once the account is penny-drop verified, the bank-returned name is authoritative: it fills and
  // locks the name field. Cleared whenever the account/IFSC/bank changes (a new account = re-verify).
  const [verifiedName, setVerifiedName] = useState<string | null>(null);

  // Bank master for the dropdown — pick a bank to prefill its name + default IFSC.
  const banksQ = useQuery({
    queryKey: ['dmt', 'banks'],
    queryFn: async () => {
      const res = await fetchDmtBanks();
      return res.ok && Array.isArray(res.data) ? res.data : [];
    },
  });
  const banks: DmtBankOption[] = banksQ.data ?? [];
  const bankOptions: AutocompleteOption[] = useMemo(
    () => banks.map((b) => ({ value: b.DMTBankID, label: b.Name, sublabel: b.DefaultIFSC })),
    [banks],
  );

  const onBankChange = (id: string) => {
    setBankId(id);
    setVerifiedName(null); // bank/IFSC changed → previous verification no longer applies
    const bank = banks.find((b) => String(b.DMTBankID) === id);
    if (bank) {
      setBankName(bank.Name);
      setIfsc(bank.DefaultIFSC.toUpperCase().replace(/[^A-Z0-9]/g, ''));
      setBranch('');
    } else {
      setBankName('');
    }
  };

  const branchMut = useMutation({
    mutationFn: async () => {
      if (ifsc.length !== 11) throw new Error('Enter a valid 11-character IFSC.');
      const res = await checkBranch(ifsc.toUpperCase());
      if (!res.ok || !res.data) throw new Error(res.message || 'IFSC details not found.');
      return res.data;
    },
    onSuccess: (d) => {
      setBankName(d.BankName || '');
      setBranch(d.Branch ? `${d.Branch}${d.Address ? ` · ${d.Address}` : ''}` : '');
      toast.success('Bank details found.');
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // Penny-drop account verification: fetch the bank's account-holder name (charges a small fee).
  const verifyMut = useMutation({
    mutationFn: async () => {
      if (accountNo.length < 7) throw new Error('Enter a valid account number first.');
      if (ifsc.length !== 11) throw new Error('Enter a valid 11-character IFSC first.');
      const res = await verifyBeneficiaryAccount({
        senderGuid: sender.SenderGUID,
        senderMobile: sender.Mobile,
        accountNo,
        ifsc: ifsc.toUpperCase(),
        bankName,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'Account could not be verified.');
      return res.data;
    },
    onSuccess: (d) => {
      setName(d.VerifiedName);
      setVerifiedName(d.VerifiedName);
      if (d.BankName) setBankName(d.BankName);
      toast.success(`Verified: ${d.VerifiedName}${d.Charge ? ` (₹${d.Charge} charged)` : ''}`);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const addMut = useMutation({
    mutationFn: async () => {
      if (!name.trim()) throw new Error('Enter the beneficiary name.');
      if (ifsc.length !== 11) throw new Error('Enter a valid 11-character IFSC.');
      if (accountNo.length < 7) throw new Error('Enter a valid account number.');
      if (accountNo !== confirmAccountNo) throw new Error('Account numbers do not match.');
      const res = await addBeneficiary({
        senderGuid: sender.SenderGUID,
        senderMobile: sender.Mobile,
        name: name.trim(),
        accountNo,
        ifsc: ifsc.toUpperCase(),
        bankName,
        isNameVerified: !!verifiedName,
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'Could not add beneficiary.');
      return res.data;
    },
    onSuccess: (data) => {
      if (data.Message.toLowerCase().includes('verify otp')) {
        setOtpFor(data.BeneficiaryGUID);
        toast.info('OTP sent to the sender. Verify to activate the beneficiary.');
      } else {
        toast.success(data.Message || 'Beneficiary added.');
        onAdded();
      }
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Modal open onClose={onClose} title="Add beneficiary" size="md">
      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault();
          addMut.mutate();
        }}
      >
        <div className="sm:col-span-2">
          <Input
            label={
              <span className="flex items-center gap-1.5">
                Beneficiary name
                {verifiedName && (
                  <span className="inline-flex items-center gap-1 rounded-full bg-success-50 px-1.5 py-0.5 text-[11px] font-semibold text-success-700">
                    <CheckCircle2 className="size-3" /> Verified
                  </span>
                )}
              </span>
            }
            placeholder="As per bank account"
            value={name}
            readOnly={!!verifiedName}
            className={verifiedName ? 'bg-success-50/40' : undefined}
            hint={verifiedName ? 'Name confirmed by the bank — locked.' : undefined}
            onChange={(e) => setName(e.target.value)}
          />
        </div>
        <div className="sm:col-span-2">
          <Autocomplete
            label="Bank"
            placeholder="Type to search bank…"
            options={bankOptions}
            value={bankId}
            loading={banksQ.isFetching}
            onChange={(v) => onBankChange(v === '' ? '' : String(v))}
          />
        </div>
        <div className="sm:col-span-2 grid grid-cols-[1fr_auto] items-end gap-2">
          <Input
            label="IFSC"
            placeholder="11-character IFSC"
            leftIcon={<Landmark className="size-4" />}
            maxLength={11}
            value={ifsc}
            onChange={(e) => {
              setIfsc(e.target.value.toUpperCase().replace(/[^A-Z0-9]/g, ''));
              setBankId('');
              setBankName('');
              setBranch('');
              setVerifiedName(null);
            }}
          />
          <Button type="button" variant="secondary" loading={branchMut.isPending} onClick={() => branchMut.mutate()}>
            <Search className="size-4" />
            Check
          </Button>
        </div>
        {(bankName || branch) && (
          <div className="sm:col-span-2 rounded-lg border border-slate-200 bg-slate-50/60 px-3 py-2 text-sm">
            <p className="font-semibold text-slate-800">{bankName || 'Bank'}</p>
            {branch && <p className="mt-0.5 text-xs text-slate-500">{branch}</p>}
          </div>
        )}
        <Input
          label="Account number"
          placeholder="Account number"
          inputMode="numeric"
          maxLength={20}
          value={accountNo}
          onChange={(e) => { setAccountNo(e.target.value.replace(/\D/g, '')); setVerifiedName(null); }}
        />
        <Input
          label="Confirm account number"
          placeholder="Re-enter account number"
          inputMode="numeric"
          maxLength={20}
          value={confirmAccountNo}
          onChange={(e) => setConfirmAccountNo(e.target.value.replace(/\D/g, ''))}
          error={confirmAccountNo && accountNo !== confirmAccountNo ? 'Does not match' : undefined}
        />

        {/* Penny-drop account verification — fetches & locks the bank's account-holder name. */}
        <div className="sm:col-span-2 flex flex-wrap items-center justify-between gap-2 rounded-lg border border-slate-200 bg-slate-50/60 px-3 py-2.5">
          <div className="min-w-0 text-sm">
            {verifiedName ? (
              <p className="flex items-center gap-1.5 font-semibold text-success-700">
                <CheckCircle2 className="size-4 shrink-0" /> {verifiedName}
              </p>
            ) : (
              <p className="text-slate-500">Verify the account to confirm the holder's name (a small fee applies).</p>
            )}
          </div>
          <Button
            type="button"
            variant={verifiedName ? 'outline' : 'secondary'}
            loading={verifyMut.isPending}
            disabled={accountNo.length < 7 || ifsc.length !== 11 || !!verifiedName}
            onClick={() => verifyMut.mutate()}
          >
            <ShieldCheck className="size-4" />
            {verifiedName ? 'Verified' : 'Verify account'}
          </Button>
        </div>

        <div className="sm:col-span-2 flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={addMut.isPending}>
            <Plus className="size-4" />
            Add beneficiary
          </Button>
        </div>
      </form>

      {otpFor && (
        <OtpModal
          title="Verify beneficiary"
          subtitle={`Enter the OTP sent to ${sender.Mobile}`}
          onSubmit={(otp) => verifyBeneficiaryOtp(otpFor, otp)}
          onClose={() => setOtpFor(null)}
          onVerified={() => {
            setOtpFor(null);
            toast.success('Beneficiary verified.');
            onAdded();
          }}
        />
      )}
    </Modal>
  );
}

/* -------------------------------------------------------------------------- */
/* Transfer                                                                     */
/* -------------------------------------------------------------------------- */

function TransferModal({
  sender,
  beneficiary,
  onClose,
  onDone,
}: {
  sender: SenderDetails;
  beneficiary: BeneficiaryDetail;
  onClose: () => void;
  onDone: (res: TransferResult) => void;
}) {
  const toast = useToast();
  const [amount, setAmount] = useState('');
  const [paymentType, setPaymentType] = useState<PaymentType>('IMPS');
  const [remark, setRemark] = useState('');

  const amt = Number(amount) || 0;
  // Admin per-transfer cap from Key Setting 'ShowLimitInDMT' (0 ⇒ no extra cap).
  const perTxnCap = sender.DisplayLimit > 0 ? sender.DisplayLimit : Infinity;
  const overCap = amt > perTxnCap;
  const overLimit = amt > sender.RemainingLimit || overCap;

  const transferMut = useMutation({
    mutationFn: async () => {
      if (amt <= 0) throw new Error('Enter a valid amount.');
      if (overCap) throw new Error(`Amount exceeds the maximum per transfer (${inr(sender.DisplayLimit)}).`);
      if (overLimit) throw new Error('Amount exceeds the sender limit.');
      const res = await moneyTransfer({ sender, beneficiary, amount: amt, paymentType, remark });
      if (!res.ok || !res.data) throw new Error(res.message || 'Transfer failed.');
      return res.data;
    },
    onSuccess: (data) => onDone(data),
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <Modal open onClose={onClose} title="Send money" size="md">
      <div className="mb-4 rounded-lg border border-slate-200 bg-slate-50/60 px-4 py-3">
        <p className="text-sm font-semibold text-slate-800">{beneficiary.Name}</p>
        <p className="mt-0.5 text-xs text-slate-500 tabular-nums">
          {beneficiary.BankName ? `${beneficiary.BankName} · ` : ''}
          {beneficiary.AccountNo} · {beneficiary.IFSC}
        </p>
      </div>

      <form
        className="space-y-4"
        onSubmit={(e) => {
          e.preventDefault();
          transferMut.mutate();
        }}
      >
        <Input
          label="Amount (₹)"
          type="number"
          step="1"
          placeholder="0"
          leftIcon={<Banknote className="size-4" />}
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          error={
            overCap
              ? `Exceeds max per transfer (${inr(sender.DisplayLimit)})`
              : overLimit
                ? `Exceeds remaining limit (${inr(sender.RemainingLimit)})`
                : undefined
          }
          hint={
            sender.DisplayLimit > 0
              ? `Remaining limit: ${inr(sender.RemainingLimit)} · Max per transfer: ${inr(sender.DisplayLimit)}`
              : `Remaining limit: ${inr(sender.RemainingLimit)}`
          }
        />

        <div>
          <span className="mb-1.5 block text-sm font-medium text-slate-700">Transfer mode</span>
          <div className="flex gap-2">
            {(['IMPS', 'NEFT'] as PaymentType[]).map((m) => (
              <button
                key={m}
                type="button"
                onClick={() => setPaymentType(m)}
                className={cn(
                  'flex-1 rounded-lg border px-4 py-2 text-sm font-semibold transition-colors',
                  paymentType === m
                    ? 'border-brand-400 bg-brand-50 text-brand-700'
                    : 'border-slate-300 bg-white text-slate-600 hover:bg-slate-50',
                )}
              >
                {m}
              </button>
            ))}
          </div>
        </div>

        <Input
          label="Remark (optional)"
          placeholder="Note for this transfer"
          value={remark}
          onChange={(e) => setRemark(e.target.value)}
        />

        <div className="flex items-center justify-between border-t border-slate-100 pt-4">
          <p className="text-sm text-slate-500">
            Payable: <span className="font-semibold text-slate-800">{inr(amt)}</span>
          </p>
          <div className="flex gap-2">
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" loading={transferMut.isPending} disabled={amt <= 0 || overLimit}>
              <Send className="size-4" />
              Send {amt > 0 ? inr(amt) : ''}
            </Button>
          </div>
        </div>
      </form>
    </Modal>
  );
}

function TransferResultModal({ result, onClose }: { result: TransferResult; onClose: () => void }) {
  const st = result.Status.toUpperCase();
  const kind = st === 'SUCCESS' ? 'SUCCESS' : st === 'PENDING' || st === 'PROCESS' ? 'PENDING' : 'FAILED';
  return (
    <Modal open onClose={onClose} title="Transfer status" size="sm">
      <div className="flex flex-col items-center px-2 pb-2 text-center">
        {kind === 'SUCCESS' ? (
          <CheckCircle2 className="size-16 text-success-500" />
        ) : kind === 'PENDING' ? (
          <Clock3 className="size-16 text-amber-500" />
        ) : (
          <XCircle className="size-16 text-danger-500" />
        )}
        <p
          className={cn(
            'mt-3 font-display text-xl font-extrabold',
            kind === 'SUCCESS' && 'text-success-600',
            kind === 'PENDING' && 'text-amber-600',
            kind === 'FAILED' && 'text-danger-600',
          )}
        >
          {kind === 'SUCCESS' ? 'Transfer Successful' : kind === 'PENDING' ? 'Transfer In Process' : 'Transfer Failed'}
        </p>
        <p className="mt-1 font-display text-3xl font-extrabold text-slate-800 tabular-nums">{inr(result.Amount)}</p>
        {result.Message && <p className="mt-2 max-w-sm text-sm text-slate-500">{result.Message}</p>}

        <div className="mt-4 w-full divide-y divide-slate-100 rounded-xl border border-slate-200 text-left text-sm">
          <ResultRow label="Account" value={result.AccountNo} mono />
          <ResultRow label="IFSC" value={result.IFSC} mono />
          {result.TransactionMasterID > 0 && (
            <ResultRow label="Transaction ID" value={String(result.TransactionMasterID)} mono />
          )}
          {result.ProviderRefNo && <ResultRow label="Reference" value={result.ProviderRefNo} mono />}
          {result.ClosingBalance > 0 && <ResultRow label="Closing balance" value={inr(result.ClosingBalance)} mono />}
        </div>

        <Button className="mt-5 w-full" onClick={onClose}>
          Done
        </Button>
      </div>
    </Modal>
  );
}

function ResultRow({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="flex items-center justify-between px-4 py-2.5">
      <span className="text-slate-500">{label}</span>
      <span className={cn('font-medium text-slate-800', mono && 'tabular-nums')}>{value || '—'}</span>
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/* OTP entry — modal + inline variants                                          */
/* -------------------------------------------------------------------------- */

interface OtpCoreProps {
  subtitle: string;
  onSubmit: (otp: string) => Promise<{ ok: boolean; message: string }>;
  onVerified: () => void;
  onResend?: () => Promise<{ ok: boolean; message: string }>;
}

function useOtp({ onSubmit, onVerified, onResend }: OtpCoreProps) {
  const toast = useToast();
  const [otp, setOtp] = useState('');
  const verifyMut = useMutation({
    mutationFn: async () => {
      if (otp.length < 4) throw new Error('Enter the OTP.');
      const res = await onSubmit(otp);
      if (!res.ok) throw new Error(res.message || 'Invalid OTP.');
      return res;
    },
    onSuccess: () => onVerified(),
    onError: (e: Error) => toast.error(e.message),
  });
  const resendMut = useMutation({
    mutationFn: async () => {
      if (!onResend) return;
      const res = await onResend();
      if (!res.ok) throw new Error(res.message || 'Could not resend OTP.');
      return res;
    },
    onSuccess: () => toast.info('OTP resent.'),
    onError: (e: Error) => toast.error(e.message),
  });
  return { otp, setOtp, verifyMut, resendMut };
}

function OtpInline(props: OtpCoreProps) {
  const { otp, setOtp, verifyMut, resendMut } = useOtp(props);
  return (
    <>
      <form
        className="flex flex-col gap-3 sm:flex-row sm:items-end"
        onSubmit={(e) => {
          e.preventDefault();
          verifyMut.mutate();
        }}
      >
        <div className="flex-1">
          <Input
            label="OTP"
            placeholder="Enter OTP"
            inputMode="numeric"
            maxLength={6}
            value={otp}
            onChange={(e) => setOtp(e.target.value.replace(/\D/g, ''))}
          />
        </div>
        <div className="flex gap-2">
          <Button type="submit" loading={verifyMut.isPending}>
            <ShieldCheck className="size-4" />
            Verify
          </Button>
          {props.onResend && (
            <Button type="button" variant="outline" loading={resendMut.isPending} onClick={() => resendMut.mutate()}>
              Resend
            </Button>
          )}
        </div>
      </form>
      <p className="mt-2 text-xs text-slate-400">{props.subtitle}</p>
    </>
  );
}

function OtpModal(props: OtpCoreProps & { title: string; onClose: () => void }) {
  return (
    <Modal open onClose={props.onClose} title={props.title} size="sm">
      <OtpInline {...props} />
    </Modal>
  );
}

/* -------------------------------------------------------------------------- */
/* Recent transfers                                                             */
/* -------------------------------------------------------------------------- */

function RecentTransfers({ rows, loading }: { rows: Record<string, unknown>[]; loading: boolean }) {
  const isMobile = useIsMobileApp();
  const dmtRows = useMemo(
    () =>
      rows
        .filter((r) => Number(getField(r, 'ServiceID') ?? 0) === DMT_SERVICE_ID)
        .map((r) => ({
          id: Number(getField(r, 'TransactionMasterID') ?? 0),
          date: String(getField(r, 'DateTime') ?? ''),
          beneficiary: String(getField(r, 'DMT_BeneficiaryName', 'BeneficiaryName') ?? ''),
          account: String(getField(r, 'ConsumerNumber') ?? ''),
          bank: String(getField(r, 'DMT_Bank', 'BankName') ?? ''),
          status: String(getField(r, 'Status') ?? ''),
          amount: Number(getField(r, 'Amount') ?? 0),
          debit: Number(getField(r, 'DebitAmount') ?? 0),
          clBal: Number(getField(r, 'ClosingBalance') ?? 0),
        })),
    [rows],
  );

  // A seven-column table is unreadable at 375px — the same rows become a list there.
  if (isMobile) {
    if (loading && dmtRows.length === 0) {
      return <p className="px-4 py-8 text-center text-sm text-slate-400">Loading…</p>;
    }
    if (dmtRows.length === 0) {
      return <EmptyState title="No transfers yet" description="Your recent money transfers will show up here." />;
    }
    return (
      <>
        {dmtRows.map((r, i) => (
          <div
            key={`${r.id}-${i}`}
            className={cn('flex items-start gap-3 px-4 py-3', i > 0 && 'border-t border-slate-100')}
          >
            <span className="min-w-0 flex-1">
              <span className="block truncate text-sm font-semibold text-slate-800">
                {r.beneficiary || r.account || '—'}
              </span>
              <span className="block truncate text-xs text-slate-400">
                {/* Don't repeat whatever already became the title. */}
                {[r.bank, r.beneficiary ? r.account : '', txnWhen(r.date)].filter(Boolean).join(' · ')}
              </span>
            </span>
            <span className="shrink-0 text-right">
              <span className="block font-display text-sm font-bold tabular-nums text-slate-900">
                {inr(r.amount)}
              </span>
              <StatusBadge status={r.status} />
            </span>
          </div>
        ))}
      </>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
            <th className="px-4 py-2.5">#</th>
            <th className="px-4 py-2.5">Date &amp; time</th>
            <th className="px-4 py-2.5">Beneficiary</th>
            <th className="px-4 py-2.5">Account</th>
            <th className="px-4 py-2.5">Status</th>
            <th className="px-4 py-2.5 text-right">Amount</th>
            <th className="px-4 py-2.5 text-right">Cl bal</th>
          </tr>
        </thead>
        <tbody>
          {loading && dmtRows.length === 0 ? (
            <tr>
              <td colSpan={7} className="px-4 py-8 text-center text-sm text-slate-400">
                Loading…
              </td>
            </tr>
          ) : dmtRows.length === 0 ? (
            <tr>
              <td colSpan={7}>
                <EmptyState title="No transfers yet" description="Your recent money transfers will show up here." />
              </td>
            </tr>
          ) : (
            dmtRows.map((r, i) => (
              <tr key={`${r.id}-${i}`} className="border-b border-slate-100 last:border-0 even:bg-slate-50/40">
                <td className="px-4 py-2.5 text-slate-500">{i + 1}</td>
                <td className="whitespace-nowrap px-4 py-2.5 text-slate-600">
                  {r.date ? new Date(r.date).toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' }) : '—'}
                </td>
                <td className="px-4 py-2.5">
                  <span className="font-medium text-slate-800">{r.beneficiary || '—'}</span>
                  {r.bank && <span className="block text-xs text-slate-400">{r.bank}</span>}
                </td>
                <td className="px-4 py-2.5 text-slate-600 tabular-nums">{r.account}</td>
                <td className="px-4 py-2.5">
                  <StatusBadge status={r.status} />
                </td>
                <td className="px-4 py-2.5 text-right tabular-nums">{inr(r.amount)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums">{inr(r.clBal)}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
