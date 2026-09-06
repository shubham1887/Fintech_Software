import { useState, type ReactNode } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ArrowLeft, Building2, CalendarDays, CheckCircle2, Hash, IdCard, KeyRound, Landmark,
  Lock, Mail, MapPin, Pencil, Phone, Power, ShieldCheck, Smartphone, UserRound,
  Wallet, XCircle,
} from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { roleLabel } from '@/lib/roles';
import { getField as get } from '@/lib/row';
import { inr, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { Spinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/EmptyState';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;

/** A label/value row inside an info card — flat, no nested boxes. */
function Field({ icon: Icon, label, value }: { icon?: typeof Wallet; label: string; value: ReactNode }) {
  const empty = value === null || value === undefined || value === '' || value === '—';
  return (
    <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-2.5 last:border-0">
      <span className="flex shrink-0 items-center gap-2 text-sm text-slate-500">
        {Icon && <Icon className="size-4 text-slate-400" />}
        {label}
      </span>
      <span className={cn('text-right text-sm font-medium', empty ? 'text-slate-300' : 'text-slate-800')}>
        {empty ? '—' : value}
      </span>
    </div>
  );
}

/** A titled section card holding Field rows. */
function InfoCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Card className="p-5">
      <h3 className="mb-1 text-sm font-semibold text-slate-700">{title}</h3>
      <div>{children}</div>
    </Card>
  );
}

/** A yes/no capability pill. */
function Flag({ label, on }: { label: string; on: boolean }) {
  return (
    <div className="flex items-center justify-between border-b border-slate-100 py-2.5 last:border-0">
      <span className="text-sm text-slate-500">{label}</span>
      {on ? (
        <span className="flex items-center gap-1 text-sm font-medium text-success-600">
          <CheckCircle2 className="size-4" /> Enabled
        </span>
      ) : (
        <span className="flex items-center gap-1 text-sm font-medium text-slate-400">
          <XCircle className="size-4" /> Off
        </span>
      )}
    </div>
  );
}

const AVATAR_TONES = [
  'bg-brand-100 text-brand-700',
  'bg-emerald-100 text-emerald-700',
  'bg-amber-100 text-amber-700',
  'bg-rose-100 text-rose-700',
  'bg-violet-100 text-violet-700',
  'bg-sky-100 text-sky-700',
];
function avatarTone(seed: string) {
  let h = 0;
  for (let i = 0; i < seed.length; i++) h = (h * 31 + seed.charCodeAt(i)) >>> 0;
  return AVATAR_TONES[h % AVATAR_TONES.length];
}

export function UserProfileScreen(_props: CustomScreenProps) {
  const navigate = useNavigate();
  const toast = useToast();
  const qc = useQueryClient();
  const targetId = new URLSearchParams(useLocation().search).get('id') ?? '';

  const [holdOpen, setHoldOpen] = useState(false);
  const [holdAmount, setHoldAmount] = useState('');
  const [holdRemark, setHoldRemark] = useState('');

  const { data: res, isLoading, refetch } = useQuery({
    queryKey: ['user-profile', targetId],
    queryFn: () => apiPost<Row>('Users/SelectByID', { TargetUserID: Number(targetId) }),
    enabled: Boolean(targetId),
  });
  const u = res?.ok ? res.data : undefined;

  const invalidate = () => {
    refetch();
    qc.invalidateQueries({ queryKey: ['users-screen'] });
  };

  const activateMut = useMutation({
    mutationFn: (next: boolean) => apiPost('Users/Activate', { TargetUserID: Number(targetId), IsActive: next }),
    onSuccess: (r) => (r.ok ? (toast.success('User status updated.'), invalidate()) : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const pwdMut = useMutation({
    mutationFn: () => apiPost('Users/SendPassword', { TargetUserID: Number(targetId) }),
    onSuccess: (r) => (r.ok ? toast.success('New password sent by SMS.') : toast.error(r.message)),
    onError: (e: Error) => toast.error(e.message),
  });

  const holdMut = useMutation({
    mutationFn: (v: { amount: number; remark: string }) =>
      apiPost('Users/AddHoldBalance', { TargetUserID: Number(targetId), Amount: v.amount, Remark: v.remark }),
    onSuccess: (r) => {
      if (!r.ok) return toast.error(r.message);
      toast.success('Hold balance updated.');
      setHoldOpen(false);
      setHoldAmount('');
      setHoldRemark('');
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const submitHold = () => {
    const amount = Number(holdAmount);
    if (!amount || amount <= 0) return toast.error('Enter a hold amount greater than zero.');
    holdMut.mutate({ amount, remark: holdRemark.trim() });
  };

  if (!targetId) {
    return (
      <>
        <PageHeader title="User Profile" icon={UserRound} />
        <EmptyState icon={UserRound} title="No user selected" description="Open a profile from the users list." />
      </>
    );
  }

  if (isLoading) {
    return (
      <>
        <PageHeader title="User Profile" icon={UserRound} />
        <div className="flex min-h-[40vh] items-center justify-center">
          <Spinner className="size-6 text-brand-500" />
        </div>
      </>
    );
  }

  if (!u) {
    return (
      <>
        <PageHeader
          title="User Profile"
          icon={UserRound}
          actions={
            <Button variant="outline" onClick={() => navigate('/s/Users')}>
              <ArrowLeft className="size-4" /> Back to users
            </Button>
          }
        />
        <EmptyState icon={UserRound} title="User not found" description={res?.message || 'This user could not be loaded.'} />
      </>
    );
  }

  const name = String(get(u, 'PersonName') || get(u, 'FirmName') || '—');
  const firm = String(get(u, 'FirmName') ?? '');
  const roleName = String(get(u, 'UserType') ?? '') || roleLabel(Number(get(u, 'UserTypeID') ?? 0)) || '—';
  const active = Boolean(get(u, 'IsActive'));
  const deleted = Boolean(get(u, 'IsDeleted'));
  const kyc = Boolean(get(u, 'IsKYCApproved'));
  const commHold = Boolean(get(u, 'IsCommissionHold'));
  const initials = name.trim().slice(0, 1).toUpperCase() || 'U';
  const balance = Number(get(u, 'Balance')) || 0;
  const hold = Number(get(u, 'HoldBalance')) || 0;
  const available = balance - hold;

  const parentName = String(get(u, 'ParentName') ?? '');
  const parentMobile = String(get(u, 'ParentMobile') ?? '');

  return (
    <>
      <PageHeader
        title="User Profile"
        subtitle={roleName}
        icon={UserRound}
        actions={
          <div className="flex flex-wrap items-center gap-2">
            <Button variant="outline" onClick={() => navigate('/s/Users')}>
              <ArrowLeft className="size-4" /> Back
            </Button>
            {!deleted && (
              <Button variant="outline" onClick={() => navigate(`/s/Users/Edit?id=${targetId}`)}>
                <Pencil className="size-4" /> Edit
              </Button>
            )}
          </div>
        }
      />

      {/* Identity header */}
      <Card className="mb-5 p-6">
        <div className="flex flex-col gap-5 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-center gap-4">
            <span className={cn('grid size-14 shrink-0 place-items-center rounded-full text-xl font-semibold', avatarTone(name))}>
              {initials}
            </span>
            <div className="min-w-0">
              <p className="truncate text-lg font-semibold text-slate-900">{name}</p>
              {firm && firm !== name && <p className="truncate text-sm text-slate-500">{firm}</p>}
              <div className="mt-2 flex flex-wrap gap-1.5">
                <Badge tone="brand">{roleName}</Badge>
                {deleted ? (
                  <Badge tone="danger">Deleted</Badge>
                ) : (
                  <Badge tone={active ? 'success' : 'danger'}>{active ? 'Active' : 'Inactive'}</Badge>
                )}
                <Badge tone={kyc ? 'success' : 'neutral'}>{kyc ? 'KYC approved' : 'KYC pending'}</Badge>
                {commHold && <Badge tone="warning">Commission hold</Badge>}
              </div>
            </div>
          </div>

          {!deleted && (
            <div className="flex flex-wrap gap-2">
              <Button
                variant="outline"
                size="sm"
                loading={activateMut.isPending}
                onClick={() => activateMut.mutate(!active)}
              >
                <Power className={cn('size-4', active ? 'text-danger-500' : 'text-success-600')} />
                {active ? 'Deactivate' : 'Activate'}
              </Button>
              <Button variant="outline" size="sm" onClick={() => setHoldOpen(true)}>
                <Lock className="size-4 text-slate-400" /> Add hold
              </Button>
              <Button variant="outline" size="sm" loading={pwdMut.isPending} onClick={() => pwdMut.mutate()}>
                <KeyRound className="size-4 text-slate-400" /> Send password
              </Button>
            </div>
          )}
        </div>
      </Card>

      {/* Balance strip — flat, three columns */}
      <Card className="mb-5">
        <div className="grid grid-cols-1 divide-y divide-slate-100 sm:grid-cols-3 sm:divide-x sm:divide-y-0">
          <div className="p-5">
            <p className="flex items-center gap-2 text-sm text-slate-500">
              <Wallet className="size-4 text-slate-400" /> Wallet balance
            </p>
            <p className="mt-1 font-display text-2xl font-bold tabular-nums text-slate-900">{inr(balance)}</p>
          </div>
          <div className="p-5">
            <p className="flex items-center gap-2 text-sm text-slate-500">
              <Lock className="size-4 text-slate-400" /> On hold
            </p>
            <p className="mt-1 font-display text-2xl font-bold tabular-nums text-slate-900">{inr(hold)}</p>
          </div>
          <div className="p-5">
            <p className="flex items-center gap-2 text-sm text-slate-500">
              <CheckCircle2 className="size-4 text-slate-400" /> Available
            </p>
            <p className={cn('mt-1 font-display text-2xl font-bold tabular-nums', available < 0 ? 'text-danger-600' : 'text-slate-900')}>
              {inr(available)}
            </p>
          </div>
        </div>
      </Card>

      {/* Info grid */}
      <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
        <InfoCard title="Contact">
          <Field icon={UserRound} label="Person name" value={String(get(u, 'PersonName') ?? '')} />
          <Field icon={Building2} label="Firm name" value={firm} />
          <Field icon={IdCard} label="Alias" value={String(get(u, 'AliasName') ?? '')} />
          <Field icon={Phone} label="Mobile" value={String(get(u, 'MobileNumber') ?? '')} />
          <Field icon={Smartphone} label="WhatsApp" value={String(get(u, 'WhatsAppNumber') ?? '')} />
          <Field icon={Mail} label="Email" value={String(get(u, 'EmailID') ?? '')} />
        </InfoCard>

        <InfoCard title="Mapping (upline)">
          <Field icon={UserRound} label="Parent" value={parentName} />
          <Field icon={Phone} label="Parent mobile" value={parentMobile} />
          <Field icon={ShieldCheck} label="Role" value={roleName} />
          <Field icon={Hash} label="Reference no." value={String(get(u, 'UniqueReferenceNo') ?? '')} />
          <Field icon={CalendarDays} label="Joined" value={dateOnly(get(u, 'DOC') as string)} />
        </InfoCard>

        <InfoCard title="Address">
          <Field icon={MapPin} label="Address" value={String(get(u, 'Address') ?? '')} />
          <Field icon={Building2} label="Shop address" value={String(get(u, 'ShopAddress') ?? '')} />
          <Field label="City" value={String(get(u, 'CityName') ?? '')} />
          <Field label="State" value={String(get(u, 'StateName') ?? '')} />
          <Field label="Pincode" value={String(get(u, 'Pincode') ?? '')} />
        </InfoCard>

        <InfoCard title="Tax & KYC">
          <Field icon={IdCard} label="PAN" value={String(get(u, 'PANNo') ?? '')} />
          <Field icon={Landmark} label="GST" value={String(get(u, 'GSTNo') ?? '')} />
          <Field icon={IdCard} label="Aadhaar" value={String(get(u, 'AadharNo') ?? '')} />
          <Field icon={ShieldCheck} label="KYC status" value={<Badge tone={kyc ? 'success' : 'neutral'}>{kyc ? 'Approved' : 'Pending'}</Badge>} />
          <Field label="NPCI agent ID" value={String(get(u, 'NPCIAgentID') ?? '')} />
        </InfoCard>

        <InfoCard title="Access & flags">
          <Flag label="Two-factor authentication" on={Boolean(get(u, 'IsTwoFA'))} />
          <Flag label="Login OTP" on={Boolean(get(u, 'IsLoginOTP'))} />
          <Flag label="Login PIN set" on={Boolean(get(u, 'IsPin'))} />
          <Flag label="Invoice applicable" on={Boolean(get(u, 'IsInvoiceApplicable'))} />
          <Flag label="Commission hold" on={commHold} />
        </InfoCard>
      </div>

      {/* Add hold balance */}
      <Modal open={holdOpen} onClose={() => setHoldOpen(false)} title="Add hold balance">
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            Freeze part of <span className="font-medium text-slate-700">{firm || name}</span>&rsquo;s wallet.
            Held funds stay in the balance but can&rsquo;t be spent.
          </p>
          <div className="flex gap-4 rounded-lg bg-slate-50 p-3 text-sm">
            <div>
              <p className="text-xs text-slate-400">Balance</p>
              <p className="font-medium tabular-nums text-slate-800">{inr(balance)}</p>
            </div>
            <div>
              <p className="text-xs text-slate-400">Currently on hold</p>
              <p className="font-medium tabular-nums text-slate-800">{inr(hold)}</p>
            </div>
          </div>
          <Input label="Hold amount (₹)" type="number" min="0" placeholder="0.00" value={holdAmount} onChange={(e) => setHoldAmount(e.target.value)} />
          <Input label="Remark (optional)" placeholder="Why is this being held?" value={holdRemark} onChange={(e) => setHoldRemark(e.target.value)} />
          <div className="flex justify-end gap-2">
            <Button variant="outline" size="sm" onClick={() => setHoldOpen(false)}>Cancel</Button>
            <Button size="sm" loading={holdMut.isPending} onClick={submitHold}>Add hold</Button>
          </div>
        </div>
      </Modal>
    </>
  );
}
