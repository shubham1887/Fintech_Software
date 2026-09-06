import { useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { BadgePercent, Check, CheckCircle2, ChevronDown, Clock3, FileSearch, MapPin, ReceiptText, RotateCcw, Search, Smartphone, UserSearch, XCircle, Zap } from 'lucide-react';
import {
  doRecharge,
  fetchBill,
  fetchBillerFields,
  fetchDthCustomerInfo,
  fetchOperators,
  fetchOffers,
  fetchOperatorByDthNumber,
  fetchOperatorByMobile,
  fetchPlans,
  fetchRecentRecharges,
  type BillDetails,
  type DthInfoDepth,
} from '@/api/recharge.api';
import { fetchCatalog, imageUrl } from '@/api/catalog.api';
import { fetchPlanCircles } from '@/api/planCatalog.api';
import { parseDthInfo, parseDthOperator, parsePlanResponse, type DthInfo, type PlanGroup } from '@/lib/planParse';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { useIsMobileApp } from '@/lib/native';
import { txnWhen } from '@/lib/txn';
import { inr } from '@/lib/format';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { StatusBadge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';

type Row = Record<string, unknown>;

interface OperatorItem {
  id: number;
  name: string;
  serviceId: number;
  serviceName: string;
  opcode: string;
  image: string;
  minAmount: number;
  maxAmount: number;
  minLength: number;
  maxLength: number;
  numberLength: number;
  startWith: string;
  active: boolean;
  billFetch: boolean;
}

function toOperator(r: Row): OperatorItem {
  return {
    active: Boolean(getField(r, 'IsActive')),
    billFetch: Boolean(getField(r, 'IsBillFetch')),
    id: Number(getField(r, 'OperatorID') ?? 0),
    name: String(getField(r, 'OperatorName') ?? ''),
    serviceId: Number(getField(r, 'ServiceID') ?? 0),
    serviceName: String(getField(r, 'ServiceName') ?? '').trim(),
    opcode: String(getField(r, 'Opcode') ?? '').trim(),
    image: String(getField(r, 'OperatorImagePath') ?? '').trim(),
    minAmount: Number(getField(r, 'MinimumRechargeAmount') ?? 0),
    maxAmount: Number(getField(r, 'MaximumRechargeAmount') ?? 0),
    minLength: Number(getField(r, 'MinLength') ?? 0),
    maxLength: Number(getField(r, 'MaxLength') ?? 0),
    numberLength: Number(getField(r, 'NumberLength') ?? 0),
    startWith: String(getField(r, 'AccountStartWith') ?? '').trim(),
  };
}

/** Operator logo — uploaded image when available, else a colour-stable initials medallion. */
function OperatorAvatar({ name, image, className }: { name: string; image?: string; className?: string }) {
  const img = imageUrl(image);
  if (img) {
    return <img src={img} alt="" className={cn('shrink-0 rounded-full border border-slate-100 bg-white object-contain', className)} />;
  }
  const hue = [...name].reduce((h, c) => (h * 31 + c.charCodeAt(0)) % 360, 7);
  const initials = name
    .split(/\s+/)
    .filter((w) => /^[a-z0-9]/i.test(w))
    .slice(0, 2)
    .map((w) => w[0]!.toUpperCase())
    .join('') || '?';
  return (
    <span
      className={cn('grid shrink-0 select-none place-items-center rounded-full font-display text-[0.6em] font-bold text-white', className)}
      style={{ background: `linear-gradient(135deg, hsl(${hue} 62% 52%), hsl(${(hue + 45) % 360} 62% 42%))` }}
    >
      {initials}
    </span>
  );
}

/**
 * Recharge — service-wise flow like the legacy portal: pick a service tab, pick the operator
 * from a dropdown (with its logo + limits), number with live digit count, PLAN / OFFER lookups,
 * and the recent recharges alongside.
 */
export default function RechargePage() {
  const isMobile = useIsMobileApp();
  const toast = useToast();
  const qc = useQueryClient();
  const [params] = useSearchParams();

  const [serviceId, setServiceId] = useState<number>(0);
  const [operatorId, setOperatorId] = useState<string>('');
  const [opOpen, setOpOpen] = useState(false);
  const [opSearch, setOpSearch] = useState('');
  const [consumerNumber, setConsumerNumber] = useState('');
  const [amount, setAmount] = useState('');
  const [billerParams, setBillerParams] = useState<string[]>(['', '', '']); // extra fields → Param1..3
  const [bill, setBill] = useState<BillDetails | null>(null);
  // Plans / offers modal — the provider groups them by category, so the modal gets one tab per group.
  const [planModal, setPlanModal] = useState<{ title: string; groups: PlanGroup[] } | null>(null);
  const [planTab, setPlanTab] = useState(0);
  // DTH customer details fetched on demand (never automatically — the provider bills per hit).
  const [dthInfo, setDthInfo] = useState<DthInfo | null>(null);
  // Circle picked by hand — the mobile plan lookup needs a circle code and auto-detection can
  // come back without one (unlisted number, provider miss).
  const [manualCircle, setManualCircle] = useState('');
  // Operator + circle/state auto-detected from the mobile number (planapi.in). circle = code
  // (fed to plan/offer lookups); circleName = human state shown to the user.
  const [detected, setDetected] = useState<{ opCode: string; opName: string; circle: string; circleName: string } | null>(null);
  const [detecting, setDetecting] = useState(false);
  const detectedForRef = useRef<string>('');
  const manualPickRef = useRef(false); // true once the user picks an operator by hand
  // Recharge outcome dialog — SUCCESS / PENDING / FAILED with the transaction details.
  const [result, setResult] = useState<{
    status: 'SUCCESS' | 'PENDING' | 'FAILED';
    message: string;
    amount: number;
    number: string;
    operatorName: string;
    txnId: number;
    refNo: string;
    closingBalance: number | null;
  } | null>(null);

  const operatorsQ = useQuery({ queryKey: ['recharge', 'operators'], queryFn: fetchOperators, staleTime: 60_000 });
  const recentQ = useQuery({ queryKey: ['recharge', 'recent'], queryFn: () => fetchRecentRecharges(10) });
  const catalogQ = useQuery({ queryKey: ['service-catalog'], queryFn: fetchCatalog, staleTime: 300_000 });
  // Provider circle codes (static reference data — fetched once per session).
  const circlesQ = useQuery({ queryKey: ['plan-circles'], queryFn: fetchPlanCircles, staleTime: Infinity });

  // All operators feed the recent-table name/logo lookup; only ACTIVE ones are offered for recharge.
  const operators = useMemo(() => (operatorsQ.data ?? []).map(toOperator).filter((o) => o.id > 0 && o.name && o.name !== 'x'), [operatorsQ.data]);
  const activeOperators = useMemo(() => operators.filter((o) => o.active), [operators]);

  // Catalog = which services may be shown (admin toggles Active/Display) + their icons + order.
  const catalogServices = useMemo(() => (catalogQ.data ?? []).flatMap((c) => c.Services), [catalogQ.data]);

  // Service tabs: catalog services (in admin-defined order) that have active operators.
  const services = useMemo(() => {
    const withOps = new Map<number, string>();
    for (const o of activeOperators) if (o.serviceId > 0 && o.serviceName) withOps.set(o.serviceId, o.serviceName);
    if (catalogServices.length === 0) {
      return [...withOps.entries()].map(([id, name]) => ({ id, name, icon: '' })).sort((a, b) => a.id - b.id);
    }
    return catalogServices
      // A locked service keeps its tile on the dashboard so the retailer knows it exists, but it
      // has no usable transaction form here - the API refuses it either way.
      .filter((s) => withOps.has(s.ServiceID) && !s.IsLocked)
      .map((s) => ({ id: s.ServiceID, name: s.Name, icon: imageUrl(s.ServiceImagePath) }));
  }, [activeOperators, catalogServices]);

  // Initial tab: ?service=<name> match, else the first service.
  useEffect(() => {
    if (serviceId !== 0 || services.length === 0) return;
    const wanted = (params.get('service') ?? '').toLowerCase();
    const match = wanted ? services.find((s) => s.name.toLowerCase().includes(wanted)) : undefined;
    setServiceId((match ?? services[0]).id);
  }, [services, serviceId, params]);

  const serviceOperators = useMemo(() => activeOperators.filter((o) => o.serviceId === serviceId), [activeOperators, serviceId]);
  const operator = serviceOperators.find((o) => String(o.id) === operatorId);
  const isDth = (operator?.serviceName ?? '').toUpperCase().includes('DTH');
  const isPrepaid = /prepaid/i.test(operator?.serviceName ?? services.find((s) => s.id === serviceId)?.name ?? '');
  // Bill-type operator (admin's IsBillFetch flag) → Fetch Bill replaces Plan/Offer.
  const isBill = Boolean(operator?.billFetch);

  // The operator's own input fields from BBPSConfiguration (FK OperatorID): the first field
  // replaces the generic consumer-number input, the rest map to Param1..3.
  const fieldsQ = useQuery({
    queryKey: ['biller-fields', operator?.id ?? 0],
    enabled: Boolean(operator),
    staleTime: 300_000,
    queryFn: () => fetchBillerFields(operator!.id),
  });
  const billerFields = operator ? (fieldsQ.data ?? []) : [];
  const primaryField = billerFields[0];
  const extraFields = billerFields.slice(1, 4);
  const missingExtras = extraFields.some((f, i) => f.required && !billerParams[i]?.trim());

  // Operator switch = fresh slate for its fields, any fetched bill and any DTH customer lookup.
  useEffect(() => { setBillerParams(['', '', '']); setBill(null); setDthInfo(null); }, [operatorId]);

  // Is the current tab a prepaid-mobile service? (Works before an operator is picked, so we can
  // auto-detect.) Postpaid / bill services are number-based lookups, not operator-by-number.
  const currentServiceName = services.find((s) => s.id === serviceId)?.name ?? '';
  const serviceIsPrepaid = /prepaid|mobile/i.test(operator?.serviceName ?? currentServiceName);
  // Same idea for DTH, which has its own operator-detection endpoint keyed on the VC number.
  const serviceIsDth = /dth|d2h/i.test(operator?.serviceName ?? currentServiceName);

  // The circle code sent with plan/offer lookups: whatever the number detection returned, else the
  // one the user picked by hand.
  const circle = detected?.circle || manualCircle;

  // Auto-detect operator + circle/state once a full 10-digit mobile number is entered on a prepaid
  // tab. Detection is number-based, so it runs regardless of the selected operator; the match effect
  // below then auto-selects the local operator.
  useEffect(() => {
    if (!serviceIsPrepaid || consumerNumber.length !== 10) {
      if (consumerNumber.length < 10) { setDetected(null); detectedForRef.current = ''; manualPickRef.current = false; }
      return;
    }
    if (detectedForRef.current === consumerNumber) return; // already looked this number up
    detectedForRef.current = consumerNumber;
    let alive = true;
    setDetecting(true);
    fetchOperatorByMobile(consumerNumber)
      .then((res) => {
        if (!alive || !res.ok || !res.data) return;
        const d = res.data as Record<string, unknown>;
        const opCode = String(getField(d, 'OpCode', 'Opcode', 'operator_code') ?? '').trim();
        const opName = String(getField(d, 'Operator', 'OperatorName') ?? '').trim();
        const circle = String(getField(d, 'CircleCode', 'circle_code') ?? '').trim();
        const circleName = String(getField(d, 'Circle') ?? '').trim();
        if (!opCode && !opName) return; // provider returned nothing useful
        setDetected({ opCode, opName, circle, circleName });
      })
      .finally(() => { if (alive) setDetecting(false); });
    return () => { alive = false; };
  }, [consumerNumber, serviceIsPrepaid]);

  // DTH operator auto-detect (planapi.in DthOperatorFetch). Customer/VC numbers run 10–12 digits
  // depending on the operator, so instead of firing at an exact length like the mobile lookup this
  // waits for a pause in typing — one provider hit per number, not one per keystroke.
  useEffect(() => {
    if (!serviceIsDth) return;
    if (consumerNumber.length < 10) {
      setDetected(null);
      detectedForRef.current = '';
      manualPickRef.current = false;
      return;
    }
    if (detectedForRef.current === consumerNumber) return; // already looked this number up
    let alive = true;
    const timer = setTimeout(() => {
      detectedForRef.current = consumerNumber;
      setDetecting(true);
      fetchOperatorByDthNumber(consumerNumber)
        .then((res) => {
          if (!alive || !res.ok || !res.data) return;
          const d = parseDthOperator(res.data);
          if (!d.opCode && !d.opName) return; // provider returned nothing useful
          setDetected({ opCode: d.opCode, opName: d.opName, circle: '', circleName: '' });
        })
        .finally(() => { if (alive) setDetecting(false); });
    }, 700);
    return () => { alive = false; clearTimeout(timer); };
  }, [consumerNumber, serviceIsDth]);

  // Auto-select the local operator that matches the detection (by planapi Opcode, else by name).
  // Kept separate so it also fires once the operator list finishes loading.
  useEffect(() => {
    if (!detected || manualPickRef.current) return; // never override a manual pick
    const firstWord = detected.opName.toLowerCase().split(' ')[0];
    const match =
      (detected.opCode && serviceOperators.find((o) => o.opcode && o.opcode === detected.opCode)) ||
      (firstWord && serviceOperators.find((o) => o.name.toLowerCase().includes(firstWord)));
    if (match && String(match.id) !== operatorId) setOperatorId(String(match.id));
  }, [detected, serviceOperators, operatorId]);

  const reset = () => { setOperatorId(''); setConsumerNumber(''); setAmount(''); setBillerParams(['', '', '']); setBill(null); setDthInfo(null); setDetected(null); detectedForRef.current = ''; };

  const billMut = useMutation({
    mutationFn: async () => {
      if (!operator) throw new Error('Select an operator first.');
      if (!consumerNumber) throw new Error(primaryField?.error || 'Enter the consumer number first.');
      if (missingExtras) throw new Error('Fill all the required bill fields first.');
      const res = await fetchBill({
        operatorId: operator.id,
        serviceId: operator.serviceId,
        consumerNumber,
        param1: billerParams[0],
        param2: billerParams[1],
        param3: billerParams[2],
      });
      if (!res.ok || !res.data) throw new Error(res.message || 'Bill details not available.');
      return res.data;
    },
    onSuccess: (data) => {
      setBill(data);
      if (String(data.Status ?? '').toUpperCase() === 'SUCCESS') {
        if (data.BillAmount && Number(data.BillAmount) > 0) setAmount(String(data.BillAmount));
      } else {
        toast.error(data.Message || 'Bill details not available.');
      }
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const rechargeMut = useMutation({
    mutationFn: () =>
      doRecharge({
        operatorId: Number(operatorId),
        serviceId: operator?.serviceId ?? serviceId,
        consumerNumber,
        amount: Number(amount),
        mobileNo: consumerNumber,
        param1: billerParams[0],
        param2: billerParams[1],
        param3: billerParams[2],
        // A DTH lookup gives us the subscriber's name for the same field the bill fetch fills.
        customerName: bill?.CustomerName || dthInfo?.name || '',
      }),
    onSuccess: (res) => {
      const submittedAmount = Number(amount) || 0;
      const submittedNumber = consumerNumber;
      const operatorName = operator?.name ?? '';
      if (res.ok) {
        const st = String(res.data?.Status ?? '').toUpperCase();
        setResult({
          status: st === 'SUCCESS' ? 'SUCCESS' : st === 'PENDING' || st === 'PROCESS' ? 'PENDING' : 'FAILED',
          message: String(res.data?.Message ?? ''),
          amount: Number(res.data?.Amount ?? submittedAmount),
          number: String(res.data?.ConsumerNumber ?? submittedNumber),
          operatorName,
          txnId: Number(res.data?.TransactionMasterID ?? 0),
          refNo: String(res.data?.ProviderRefNo ?? ''),
          closingBalance: res.data?.ClosingBalance != null ? Number(res.data.ClosingBalance) : null,
        });
        // Fully clear the form (operator, number, amount, bill/DTH lookups) so the next
        // recharge starts fresh — the outcome stays visible in the result card above.
        reset();
        qc.invalidateQueries({ queryKey: ['recharge', 'recent'] });
        // Wallet was debited the moment the txn went to PROCESS — refresh the header balance
        // immediately so it reflects the deduction (both SUCCESS and PENDING outcomes).
        qc.invalidateQueries({ queryKey: ['my-wallets'] });
      } else {
        setResult({
          status: 'FAILED',
          message: res.message || 'Recharge failed.',
          amount: submittedAmount,
          number: submittedNumber,
          operatorName,
          txnId: 0,
          refNo: '',
          closingBalance: null,
        });
      }
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const planMut = useMutation({
    mutationFn: async () => {
      if (!operator) throw new Error('Select an operator first.');
      if (isPrepaid && !circle) throw new Error('Select the circle to see plans.');
      const res = await fetchPlans(operator.id, isDth, circle, detected?.opCode ?? '');
      if (!res.ok) throw new Error(res.message || 'Plans not available.');
      return parsePlanResponse(res.data, 'Plans');
    },
    onSuccess: (parsed) => {
      // The provider answers HTTP 200 even when it rejects the call (ERROR != "0"), so an empty
      // result is only "no plans" when it reported success.
      if (parsed.groups.length === 0) {
        return parsed.error && parsed.error !== '0'
          ? toast.error(parsed.message || 'Plans not available.')
          : toast.info(parsed.message || 'No plans found for this operator.');
      }
      setPlanTab(0);
      setPlanModal({ title: `${operator?.name} — Plans`, groups: parsed.groups });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const offerMut = useMutation({
    mutationFn: async () => {
      if (!operator) throw new Error('Select an operator first.');
      if (!consumerNumber) throw new Error('Enter the number first.');
      const res = await fetchOffers(operator.id, consumerNumber, circle);
      if (!res.ok) throw new Error(res.message || 'Offers not available.');
      return parsePlanResponse(res.data, 'Offers');
    },
    onSuccess: (parsed) => {
      if (parsed.groups.length === 0) {
        return parsed.error && parsed.error !== '0'
          ? toast.error(parsed.message || 'Offers not available.')
          : toast.info(parsed.message || 'No offers found for this number.');
      }
      setPlanTab(0);
      setPlanModal({ title: `${operator?.name} — Offers for ${consumerNumber}`, groups: parsed.groups });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // DTH customer lookup. `basic` (name / VC / RMN / address) is the default because the provider
  // bills the full call as 3 hits; the modal lets the user escalate when they need balance,
  // plan and next/last recharge date.
  const dthInfoMut = useMutation({
    mutationFn: async (depth: DthInfoDepth) => {
      if (!consumerNumber) throw new Error('Enter the DTH customer / VC number first.');
      if (!operator && !detected?.opCode) throw new Error('Select the DTH operator first.');
      const res = await fetchDthCustomerInfo(operator?.id ?? 0, consumerNumber, depth, detected?.opCode ?? '');
      if (!res.ok) throw new Error(res.message || 'Customer details not available.');
      return parseDthInfo(res.data);
    },
    onSuccess: (info) => {
      if (info.fields.length === 0) return toast.error(info.message || 'No customer details found for this number.');
      setDthInfo(info);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // Number validation hints from the operator master.
  const minLen = operator ? (operator.minLength || operator.numberLength) : 0;
  const maxLen = operator ? (operator.maxLength || operator.numberLength) : 0;
  const numberProblem =
    operator && consumerNumber.length > 0
      ? operator.startWith && !operator.startWith.split(',').some((p) => p.trim() && consumerNumber.startsWith(p.trim()))
        ? `Number should start with ${operator.startWith}`
        : minLen > 0 && consumerNumber.length < minLen
          ? `Needs at least ${minLen} digits`
          : maxLen > 0 && consumerNumber.length > maxLen
            ? `Maximum ${maxLen} digits`
            : ''
      : '';

  const amountProblem =
    operator && amount !== ''
      ? operator.minAmount > 0 && Number(amount) < operator.minAmount
        ? `Minimum ${inr(operator.minAmount)}`
        : operator.maxAmount > 0 && Number(amount) > operator.maxAmount
          ? `Maximum ${inr(operator.maxAmount)}`
          : ''
      : '';

  const canRecharge =
    operatorId !== '' && consumerNumber.length > 0 && Number(amount) > 0 &&
    !numberProblem && !amountProblem && !missingExtras && !rechargeMut.isPending;

  const recentRows = useMemo(() => {
    const opName = new Map(operators.map((o) => [o.id, o.name]));
    const opImage = new Map(operators.map((o) => [o.id, o.image]));
    return (recentQ.data ?? []).map((r) => ({
      id: Number(getField(r, 'TransactionMasterID') ?? 0),
      date: String(getField(r, 'DateTime') ?? ''),
      operatorId: Number(getField(r, 'OperatorID') ?? 0),
      operator: opName.get(Number(getField(r, 'OperatorID') ?? 0)) ?? `#${getField(r, 'OperatorID')}`,
      image: opImage.get(Number(getField(r, 'OperatorID') ?? 0)) ?? '',
      number: String(getField(r, 'ConsumerNumber') ?? ''),
      status: String(getField(r, 'Status') ?? ''),
      amount: Number(getField(r, 'Amount') ?? 0),
      debit: Number(getField(r, 'DebitAmount') ?? 0),
      clBal: Number(getField(r, 'ClosingBalance') ?? 0),
    }));
  }, [recentQ.data, operators]);

  return (
    <>
      {/* On a phone the shell header and the Services tab already say where you are. */}
      {!isMobile && <PageHeader title="Recharge" subtitle="Mobile, DTH and utility top-ups" />}

      {/* Service tabs — one scrollable row keeps the catalog tidy no matter how many services */}
      <div className="mb-4 flex gap-2 overflow-x-auto pb-2 [-ms-overflow-style:none] [scrollbar-width:thin]">
        {services.map((s) => (
          <button
            key={s.id}
            type="button"
            onClick={() => { setServiceId(s.id); setOperatorId(''); setOpOpen(false); }}
            className={cn(
              'flex shrink-0 items-center gap-1.5 whitespace-nowrap rounded-full px-4 py-1.5 text-sm font-semibold transition-colors',
              s.id === serviceId
                ? 'bg-brand-600 text-white shadow-sm'
                : 'border border-slate-200 bg-white text-slate-600 hover:bg-slate-50',
            )}
          >
            {s.icon ? <img src={s.icon} alt="" className="size-4 rounded-full bg-white object-contain" /> : null}
            {s.name}
          </button>
        ))}
        {operatorsQ.isLoading && <span className="py-1.5 text-sm text-slate-400">Loading services…</span>}
      </div>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-5">
        {/* New recharge */}
        <Card className="xl:col-span-2">
          <CardHeader>
            <CardTitle>New recharge</CardTitle>
          </CardHeader>
          <CardBody>
            <form
              className="space-y-4"
              onSubmit={(e) => { e.preventDefault(); if (canRecharge) rechargeMut.mutate(); }}
            >
              {/* Operator picker — searchable dropdown with operator logos */}
              <div className="relative">
                <span className="mb-1.5 block text-sm font-medium text-slate-700">Operator</span>
                <button
                  type="button"
                  onClick={() => { setOpOpen((v) => !v); setOpSearch(''); }}
                  className={cn(
                    'flex w-full items-center gap-3 rounded-lg border bg-white px-3 py-2 text-left text-sm transition-colors',
                    opOpen ? 'border-brand-400 ring-2 ring-brand-100' : 'border-slate-300 hover:border-slate-400',
                  )}
                >
                  {operator ? (
                    <>
                      <OperatorAvatar name={operator.name} image={operator.image} className="size-8 text-sm" />
                      <span className="min-w-0 flex-1 truncate py-1 font-semibold text-slate-800">{operator.name}</span>
                    </>
                  ) : (
                    <span className="flex-1 py-2 text-slate-400">
                      {operatorsQ.isLoading ? 'Loading operators…' : 'Select operator'}
                    </span>
                  )}
                  <ChevronDown className={cn('size-4 shrink-0 text-slate-400 transition-transform', opOpen && 'rotate-180')} />
                </button>

                {opOpen && (
                  <>
                    <div className="fixed inset-0 z-10" onClick={() => setOpOpen(false)} />
                    <div className="absolute z-20 mt-1.5 w-full overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xl">
                      {serviceOperators.length > 6 && (
                        <div className="flex items-center gap-2 border-b border-slate-100 px-3 py-2">
                          <Search className="size-4 text-slate-400" />
                          <input
                            autoFocus
                            value={opSearch}
                            onChange={(e) => setOpSearch(e.target.value)}
                            placeholder="Search operator…"
                            className="w-full bg-transparent text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none"
                          />
                        </div>
                      )}
                      <ul className="max-h-64 overflow-y-auto py-1">
                        {serviceOperators
                          .filter((o) => o.name.toLowerCase().includes(opSearch.toLowerCase()))
                          .map((o) => (
                            <li key={o.id}>
                              <button
                                type="button"
                                onClick={() => { manualPickRef.current = true; setOperatorId(String(o.id)); setOpOpen(false); }}
                                className={cn(
                                  'flex w-full items-center gap-3 px-3 py-2 text-left transition-colors hover:bg-brand-50/60',
                                  String(o.id) === operatorId && 'bg-brand-50',
                                )}
                              >
                                <OperatorAvatar name={o.name} image={o.image} className="size-8 text-sm" />
                                <span className="min-w-0 flex-1 truncate text-sm font-medium text-slate-800">{o.name}</span>
                                {String(o.id) === operatorId && <Check className="size-4 shrink-0 text-brand-600" />}
                              </button>
                            </li>
                          ))}
                        {serviceOperators.filter((o) => o.name.toLowerCase().includes(opSearch.toLowerCase())).length === 0 && (
                          <li className="px-3 py-6 text-center text-sm text-slate-400">No operators found.</li>
                        )}
                      </ul>
                    </div>
                  </>
                )}
              </div>

              {/* Primary number field — label/placeholder/length come from the operator's own
                  BBPS field configuration when the admin has defined it. */}
              <div>
                <Input
                  label={primaryField?.label || 'Consumer / mobile number'}
                  placeholder={primaryField?.placeholder || (isDth ? 'Customer ID' : 'Number to recharge')}
                  leftIcon={<Smartphone className="size-4" />}
                  value={consumerNumber}
                  inputMode={primaryField?.type === 'text' ? undefined : 'numeric'}
                  maxLength={primaryField?.length ? primaryField.length : undefined}
                  onChange={(e) => {
                    const raw = primaryField?.type === 'text' ? e.target.value : e.target.value.replace(/\D/g, '');
                    setConsumerNumber(raw);
                    setBill(null);
                    setDthInfo(null);
                  }}
                  error={numberProblem || undefined}
                />
                <div className="mt-1 flex items-center justify-between gap-2 text-xs">
                  {/* Auto-detected operator + state (prepaid mobile and DTH). */}
                  <span className="min-h-4">
                    {(serviceIsPrepaid || serviceIsDth) && detecting && (
                      <span className="text-slate-400">Detecting operator…</span>
                    )}
                    {(serviceIsPrepaid || serviceIsDth) && !detecting && detected && (detected.opName || detected.circleName) && (
                      <span className="inline-flex items-center gap-1.5 rounded-full bg-brand-50 px-2 py-0.5 font-medium text-brand-700">
                        <MapPin className="size-3" />
                        {detected.opName}
                        {detected.circleName ? ` · ${detected.circleName}` : ''}
                      </span>
                    )}
                  </span>
                  <span className="text-right text-slate-400">
                    Count : <span className="font-semibold text-slate-600 tabular-nums">{consumerNumber.length}</span>
                    {primaryField?.length
                      ? ` / ${primaryField.length}`
                      : minLen > 0 && maxLen > 0 ? ` / ${minLen === maxLen ? minLen : `${minLen}–${maxLen}`}` : ''}
                  </span>
                </div>

                {/* The mobile plan lookup returns nothing without a circle code, so when detection
                    did not supply one the user picks the state here. */}
                {serviceIsPrepaid && !detected?.circle && (
                  <div className="mt-3">
                    <span className="mb-1.5 block text-sm font-medium text-slate-700">Circle</span>
                    <select
                      value={manualCircle}
                      onChange={(e) => setManualCircle(e.target.value)}
                      className="h-12 w-full rounded-xl border border-slate-300 bg-white px-3.5 text-[15px] text-slate-700 focus:border-brand-400 focus:outline-none md:h-10 md:rounded-lg md:px-3 md:text-sm"
                    >
                      <option value="">Select circle</option>
                      {(circlesQ.data ?? []).map((c) => (
                        <option key={c.Code} value={c.Code}>{c.Name}</option>
                      ))}
                    </select>
                  </div>
                )}
              </div>

              {/* Extra biller parameters (BBPSConfiguration rows 2..4 → Param1..3) */}
              {extraFields.length > 0 && (
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                  {extraFields.map((f, i) =>
                    f.type === 'list' && f.options.length > 0 ? (
                      <div key={f.id}>
                        <span className="mb-1.5 block text-sm font-medium text-slate-700">
                          {f.label} {f.required && <span className="text-danger-500">*</span>}
                        </span>
                        <select
                          value={billerParams[i] ?? ''}
                          onChange={(e) => setBillerParams((p) => p.map((v, j) => (j === i ? e.target.value : v)))}
                          className="h-12 w-full rounded-xl border border-slate-300 bg-white px-3.5 text-[15px] text-slate-700 focus:border-brand-400 focus:outline-none md:h-10 md:rounded-lg md:px-3 md:text-sm"
                        >
                          <option value="">{f.placeholder || `Select ${f.label}`}</option>
                          {f.options.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
                        </select>
                      </div>
                    ) : (
                      <Input
                        key={f.id}
                        label={<>{f.label} {f.required && <span className="text-danger-500">*</span>}</>}
                        placeholder={f.placeholder}
                        type={f.type === 'date' ? 'date' : 'text'}
                        inputMode={f.type === 'number' ? 'numeric' : undefined}
                        maxLength={f.length ? f.length : undefined}
                        value={billerParams[i] ?? ''}
                        onChange={(e) => {
                          const raw = f.type === 'number' ? e.target.value.replace(/\D/g, '') : e.target.value;
                          setBillerParams((p) => p.map((v, j) => (j === i ? raw : v)));
                          setBill(null);
                        }}
                        hint={f.error || undefined}
                      />
                    ),
                  )}
                </div>
              )}

              {/* Fetched bill details */}
              {bill && String(bill.Status ?? '').toUpperCase() === 'SUCCESS' && (
                <div className="rounded-xl border border-success-200 bg-success-50/60 p-4">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <div>
                      <p className="text-sm font-semibold text-slate-800">{bill.CustomerName || 'Bill found'}</p>
                      <p className="mt-0.5 text-xs text-slate-500">
                        {bill.BillNumber ? `Bill no. ${bill.BillNumber}` : ''}
                        {bill.BillNumber && bill.DueDate ? ' · ' : ''}
                        {bill.DueDate ? `Due ${bill.DueDate}` : ''}
                      </p>
                    </div>
                    {bill.BillAmount != null && Number(bill.BillAmount) > 0 && (
                      <p className="font-display text-xl font-extrabold text-success-700 tabular-nums">{inr(Number(bill.BillAmount))}</p>
                    )}
                  </div>
                </div>
              )}

              {/* DTH customer details (planapi.in). Starts as the cheap basic lookup; the two
                  actions escalate to the paid calls only when the retailer asks for them. */}
              {dthInfo && (
                <div className="rounded-xl border border-slate-200 bg-slate-50/60 p-4">
                  <p className="text-sm font-semibold text-slate-800">{dthInfo.name || 'Customer found'}</p>
                  <dl className="mt-3 grid grid-cols-1 gap-x-4 gap-y-1.5 sm:grid-cols-2">
                    {dthInfo.fields.map((f) => (
                      <div
                        key={f.label}
                        className={cn('flex justify-between gap-3 text-xs', f.value.length > 40 && 'sm:col-span-2')}
                      >
                        <dt className="shrink-0 text-slate-500">{f.label}</dt>
                        <dd className="text-right font-medium text-slate-700">{f.value}</dd>
                      </div>
                    ))}
                  </dl>
                  <div className="mt-3 flex flex-wrap gap-2 border-t border-slate-200 pt-3">
                    <Button type="button" variant="outline" size="sm" loading={dthInfoMut.isPending} onClick={() => dthInfoMut.mutate('full')}>
                      Full details
                    </Button>
                    <Button type="button" variant="outline" size="sm" loading={dthInfoMut.isPending} onClick={() => dthInfoMut.mutate('lastRecharge')}>
                      With last recharge
                    </Button>
                  </div>
                </div>
              )}

              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <Input
                  label="Amount (₹)"
                  type="number"
                  step="1"
                  placeholder="0.00"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                  error={amountProblem || undefined}
                  hint={operator && operator.minAmount > 0 ? `${inr(operator.minAmount)} – ${inr(operator.maxAmount)}` : undefined}
                />
                {/* Aux actions depend on the service: bills fetch, prepaid gets plans/offers.
                    A label-height spacer keeps the buttons level with the Amount input box
                    (the input cell is taller because of its hint line below). */}
                {isBill ? (
                  <div>
                    <span aria-hidden="true" className="mb-1.5 block text-sm font-medium">&nbsp;</span>
                    <Button
                      type="button"
                      variant="secondary"
                      className="w-full"
                      loading={billMut.isPending}
                      disabled={!operator || !consumerNumber || missingExtras}
                      onClick={() => billMut.mutate()}
                    >
                      <FileSearch className="size-4" />
                      Fetch Bill
                    </Button>
                  </div>
                ) : isPrepaid || isDth ? (
                  <div>
                    <span aria-hidden="true" className="mb-1.5 block text-sm font-medium">&nbsp;</span>
                    <div className="flex gap-2">
                      <Button type="button" variant="secondary" className="flex-1" loading={planMut.isPending} disabled={!operator} onClick={() => planMut.mutate()}>
                        <ReceiptText className="size-4" />
                        Plan
                      </Button>
                      {isPrepaid && (
                        <Button type="button" variant="outline" className="flex-1" loading={offerMut.isPending} disabled={!operator} onClick={() => offerMut.mutate()}>
                          <BadgePercent className="size-4" />
                          Offer
                        </Button>
                      )}
                      {isDth && (
                        <Button
                          type="button"
                          variant="outline"
                          className="flex-1"
                          loading={dthInfoMut.isPending}
                          disabled={!operator || !consumerNumber}
                          onClick={() => dthInfoMut.mutate('basic')}
                        >
                          <UserSearch className="size-4" />
                          Customer
                        </Button>
                      )}
                    </div>
                  </div>
                ) : null}
              </div>

              {/* On a phone this pins to the bottom of the scroll area so the retailer never
                  has to scroll back down to pay. Card has no overflow clip, so sticky works. */}
              <div
                className={cn(
                  'flex items-center justify-between gap-3 border-t border-slate-100 pt-4',
                  isMobile && 'sticky bottom-0 z-10 -mx-5 -mb-5 border-slate-200 bg-white px-5 pb-5',
                )}
              >
                <p className="shrink-0 text-sm text-slate-500">
                  Payable: <span className="font-semibold text-slate-800">{inr(Number(amount) || 0)}</span>
                </p>
                <div className="flex gap-2">
                  <Button type="button" variant="outline" onClick={reset} aria-label="Reset">
                    <RotateCcw className="size-4" />
                    {!isMobile && 'Reset'}
                  </Button>
                  <Button type="submit" loading={rechargeMut.isPending} disabled={!canRecharge}>
                    <Zap className="size-4" />
                    {isBill ? 'Pay Bill' : 'Recharge'}
                  </Button>
                </div>
              </div>
            </form>
          </CardBody>
        </Card>

        {/* Recent recharges */}
        <Card className="xl:col-span-3">
          <CardHeader>
            <CardTitle>Recent recharges</CardTitle>
          </CardHeader>
          <CardBody className="p-0">
            {/* An eight-column table cannot be read on a phone — same rows, as a list. */}
            {isMobile ? (
              recentQ.isFetching && recentRows.length === 0 ? (
                <p className="px-4 py-8 text-center text-sm text-slate-400">Loading…</p>
              ) : recentRows.length === 0 ? (
                <EmptyState title="No recharges yet" description="Your recent recharges will show up here." />
              ) : (
                recentRows.map((r, i) => (
                  <div
                    key={`${r.id}-${i}`}
                    className={cn('flex items-center gap-3 px-4 py-3', i > 0 && 'border-t border-slate-100')}
                  >
                    <OperatorAvatar name={r.operator} image={r.image} className="size-9 shrink-0 text-xs" />
                    <span className="min-w-0 flex-1">
                      <span className="block truncate text-sm font-semibold text-slate-800">{r.operator}</span>
                      <span className="block truncate text-xs text-slate-400">
                        {[r.number, txnWhen(r.date)].filter(Boolean).join(' · ')}
                      </span>
                    </span>
                    <span className="shrink-0 text-right">
                      <span className="block font-display text-sm font-bold tabular-nums text-slate-900">{inr(r.amount)}</span>
                      <StatusBadge status={r.status} />
                    </span>
                  </div>
                ))
              )
            ) : (
            <div className="overflow-x-auto">
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50/80 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <th className="px-4 py-2.5">#</th>
                    <th className="px-4 py-2.5">Date &amp; Time</th>
                    <th className="px-4 py-2.5">Operator</th>
                    <th className="px-4 py-2.5">Number</th>
                    <th className="px-4 py-2.5">Status</th>
                    <th className="px-4 py-2.5 text-right">Amount</th>
                    <th className="px-4 py-2.5 text-right">Debit</th>
                    <th className="px-4 py-2.5 text-right">Cl Bal</th>
                  </tr>
                </thead>
                <tbody>
                  {recentQ.isFetching && recentRows.length === 0 ? (
                    <tr><td colSpan={8} className="px-4 py-8 text-center text-sm text-slate-400">Loading…</td></tr>
                  ) : recentRows.length === 0 ? (
                    <tr><td colSpan={8}><EmptyState title="No recharges yet" description="Your recent recharges will show up here." /></td></tr>
                  ) : (
                    recentRows.map((r, i) => (
                      <tr key={`${r.id}-${i}`} className="border-b border-slate-100 last:border-0 even:bg-slate-50/40">
                        <td className="px-4 py-2.5 text-slate-500">{i + 1}</td>
                        <td className="whitespace-nowrap px-4 py-2.5 text-slate-600">{r.date ? new Date(r.date).toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' }) : '—'}</td>
                        <td className="px-4 py-2.5">
                          <span className="flex items-center gap-2 font-medium text-slate-800">
                            <OperatorAvatar name={r.operator} image={r.image} className="size-6 text-[10px]" />
                            {r.operator}
                          </span>
                        </td>
                        <td className="px-4 py-2.5 text-slate-600 tabular-nums">{r.number}</td>
                        <td className="px-4 py-2.5"><StatusBadge status={r.status} /></td>
                        <td className="px-4 py-2.5 text-right tabular-nums">{inr(r.amount)}</td>
                        <td className="px-4 py-2.5 text-right tabular-nums">{inr(r.debit)}</td>
                        <td className="px-4 py-2.5 text-right tabular-nums">{inr(r.clBal)}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
            )}
          </CardBody>
        </Card>
      </div>

      {/* Plans / offers modal — one tab per provider category, click a plan to use its amount */}
      {planModal && (() => {
        const tab = Math.min(planTab, planModal.groups.length - 1);
        return (
          <Modal open onClose={() => setPlanModal(null)} title={planModal.title} size="lg">
            {planModal.groups.length > 1 && (
              <div className="mb-3 flex gap-2 overflow-x-auto pb-2 [scrollbar-width:thin]">
                {planModal.groups.map((g, i) => (
                  <button
                    key={`${g.name}-${i}`}
                    type="button"
                    onClick={() => setPlanTab(i)}
                    className={cn(
                      'flex shrink-0 items-center gap-1.5 whitespace-nowrap rounded-full px-3.5 py-1.5 text-sm font-semibold transition-colors',
                      i === tab ? 'bg-brand-600 text-white' : 'border border-slate-200 bg-white text-slate-600 hover:bg-slate-50',
                    )}
                  >
                    {g.name}
                    <span className={cn('text-xs font-medium tabular-nums', i === tab ? 'text-white/70' : 'text-slate-400')}>
                      {g.rows.length}
                    </span>
                  </button>
                ))}
              </div>
            )}
            <div className="max-h-[60vh] space-y-2 overflow-y-auto pr-1">
              {planModal.groups[tab]?.rows.map((p, i) => (
                <button
                  key={i}
                  type="button"
                  onClick={() => { if (p.amount !== null) setAmount(String(p.amount)); setPlanModal(null); }}
                  className="flex w-full items-start gap-3 rounded-lg border border-slate-200 p-3 text-left transition-colors hover:border-brand-300 hover:bg-brand-50/40"
                >
                  <span className="rounded-lg bg-brand-600 px-3 py-1.5 text-sm font-bold text-white tabular-nums">
                    {p.amount !== null ? `₹${p.amount}` : p.amountText || '—'}
                  </span>
                  <span className="flex-1">
                    <span className="block text-sm text-slate-700">{p.detail || '—'}</span>
                    {p.note ? <span className="mt-0.5 block text-xs text-slate-500">{p.note}</span> : null}
                    {p.validity ? <span className="mt-0.5 block text-xs text-slate-400">Validity: {p.validity}</span> : null}
                  </span>
                </button>
              ))}
            </div>
          </Modal>
        );
      })()}

      {/* Recharge outcome dialog — the user's clear SUCCESS / PENDING / FAILED confirmation */}
      {result && (
        <Modal open onClose={() => setResult(null)} title="Recharge status">
          <div className="flex flex-col items-center px-2 pb-2 text-center">
            {result.status === 'SUCCESS' ? (
              <CheckCircle2 className="size-16 text-success-500" />
            ) : result.status === 'PENDING' ? (
              <Clock3 className="size-16 text-amber-500" />
            ) : (
              <XCircle className="size-16 text-danger-500" />
            )}
            <p
              className={cn(
                'mt-3 font-display text-xl font-extrabold',
                result.status === 'SUCCESS' && 'text-success-600',
                result.status === 'PENDING' && 'text-amber-600',
                result.status === 'FAILED' && 'text-danger-600',
              )}
            >
              {result.status === 'SUCCESS' ? 'Recharge Successful' : result.status === 'PENDING' ? 'Recharge In Process' : 'Recharge Failed'}
            </p>
            <p className="mt-1 font-display text-3xl font-extrabold text-slate-800 tabular-nums">{inr(result.amount)}</p>
            {result.message && <p className="mt-2 max-w-sm text-sm text-slate-500">{result.message}</p>}

            <div className="mt-4 w-full divide-y divide-slate-100 rounded-xl border border-slate-200 text-left text-sm">
              <div className="flex items-center justify-between px-4 py-2.5">
                <span className="text-slate-500">Operator</span>
                <span className="font-medium text-slate-800">{result.operatorName || '—'}</span>
              </div>
              <div className="flex items-center justify-between px-4 py-2.5">
                <span className="text-slate-500">Number</span>
                <span className="font-medium text-slate-800 tabular-nums">{result.number || '—'}</span>
              </div>
              {result.txnId > 0 && (
                <div className="flex items-center justify-between px-4 py-2.5">
                  <span className="text-slate-500">Transaction ID</span>
                  <span className="font-medium text-slate-800 tabular-nums">{result.txnId}</span>
                </div>
              )}
              {result.refNo && (
                <div className="flex items-center justify-between px-4 py-2.5">
                  <span className="text-slate-500">Operator Ref</span>
                  <span className="font-medium text-slate-800 tabular-nums">{result.refNo}</span>
                </div>
              )}
              {result.closingBalance != null && (
                <div className="flex items-center justify-between px-4 py-2.5">
                  <span className="text-slate-500">Closing Balance</span>
                  <span className="font-medium text-slate-800 tabular-nums">{inr(result.closingBalance)}</span>
                </div>
              )}
            </div>

            <Button className="mt-5 w-full" onClick={() => setResult(null)}>Done</Button>
          </div>
        </Modal>
      )}
    </>
  );
}
