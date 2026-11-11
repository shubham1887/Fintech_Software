import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, BadgeCheck, CheckCircle2, ShieldCheck } from 'lucide-react';
import { useBrand } from '@/theme/BrandProvider';
import { useToast } from '@/components/ui/Toast';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Autocomplete } from '@/components/ui/Autocomplete';
import { Button } from '@/components/ui/Button';
import {
  fetchPincodeDetails,
  registerAccount,
  selectSignUpRoles,
  selectStates,
  signUpGenerateOtp,
  signUpVerifyOtp,
  type RegisterPayload,
} from '@/api/registration.api';
import { BrandLockup } from '@/components/ui/BrandMark';

/** Roles a self-service applicant may pick — Admin (1) is never offered via public signup. */
const HIDDEN_ROLE_IDS = new Set([1]);

const EMPTY = {
  PersonName: '',
  FirmName: '',
  EmailID: '',
  WhatsAppNumber: '',
  PANNo: '',
  AadharNo: '',
  GSTNo: '',
  Address: '',
  CityName: '',
  District: '',
  Pincode: '',
  StateID: '',
  UserTypeID: '',
  ReferralCode: '',
};

export default function RegisterPage() {
  const { appName } = useBrand();
  const toast = useToast();
  const navigate = useNavigate();

  const [mobile, setMobile] = useState('');
  const [otp, setOtp] = useState('');
  const [otpSent, setOtpSent] = useState(false);
  const [mobileVerified, setMobileVerified] = useState(false);
  const [otpBusy, setOtpBusy] = useState(false);

  const [form, setForm] = useState({ ...EMPTY });
  const [submitBusy, setSubmitBusy] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [errorBanner, setErrorBanner] = useState('');

  const { data: statesRes } = useQuery({ queryKey: ['signup-states'], queryFn: selectStates, staleTime: 300_000 });
  const { data: rolesRes } = useQuery({ queryKey: ['signup-roles'], queryFn: () => selectSignUpRoles(''), staleTime: 300_000 });

  const states = statesRes?.data ?? [];
  const roles = useMemo(
    () => (rolesRes?.data ?? []).filter((r) => !HIDDEN_ROLE_IDS.has(r.UserTypeID)),
    [rolesRes],
  );

  const set = (key: keyof typeof EMPTY, value: string) => setForm((f) => ({ ...f, [key]: value }));

  const sendOtp = async () => {
    if (!/^\d{10}$/.test(mobile)) {
      toast.error('Enter a valid 10-digit mobile number.');
      return;
    }
    setOtpBusy(true);
    const res = await signUpGenerateOtp(mobile);
    setOtpBusy(false);
    if (res.ok) {
      toast.success('OTP sent to your mobile number.');
      setOtpSent(true);
    } else {
      toast.error(res.message);
    }
  };

  const verifyOtp = async () => {
    if (!/^\d{4,6}$/.test(otp)) {
      toast.error('Enter the OTP you received.');
      return;
    }
    setOtpBusy(true);
    const res = await signUpVerifyOtp(mobile, otp);
    setOtpBusy(false);
    if (res.ok) {
      toast.success('Mobile number verified.');
      setMobileVerified(true);
    } else {
      toast.error(res.message);
    }
  };

  const onPincodeBlur = async () => {
    if (!/^\d{6}$/.test(form.Pincode)) return;
    const res = await fetchPincodeDetails(form.Pincode);
    if (res.ok && res.data) {
      setForm((f) => ({
        ...f,
        CityName: f.CityName || res.data!.City,
        District: f.District || res.data!.District,
        StateID: String(res.data!.StateID || f.StateID),
      }));
    }
  };

  const submit = async () => {
    setErrorBanner('');
    if (!mobileVerified) {
      toast.error('Please verify your mobile number first.');
      return;
    }
    if (!form.PersonName.trim()) {
      toast.error('Please enter your name.');
      return;
    }
    if (!form.UserTypeID) {
      toast.error('Please select a role.');
      return;
    }

    const payload: RegisterPayload = {
      PersonName: form.PersonName.trim(),
      FirmName: form.FirmName.trim(),
      MobileNumber: mobile,
      WhatsAppNumber: form.WhatsAppNumber.trim(),
      EmailID: form.EmailID.trim(),
      PANNo: form.PANNo.trim().toUpperCase(),
      AadharNo: form.AadharNo.trim(),
      GSTNo: form.GSTNo.trim().toUpperCase(),
      Address: form.Address.trim(),
      CityName: form.CityName.trim(),
      District: form.District.trim(),
      Pincode: form.Pincode.trim(),
      StateID: form.StateID ? Number(form.StateID) : null,
      UserTypeID: Number(form.UserTypeID),
      ReferralCode: form.ReferralCode.trim(),
    };

    setSubmitBusy(true);
    const res = await registerAccount(payload);
    setSubmitBusy(false);
    if (res.ok) {
      setSubmitted(true);
    } else {
      // Duplicate / validation messages ("… already exists …") surface here.
      setErrorBanner(res.message);
      toast.error(res.message);
    }
  };

  if (submitted) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-canvas px-6">
        <div className="w-full max-w-md animate-rise text-center">
          <div className="mx-auto grid size-14 place-items-center rounded-full bg-emerald-50 text-emerald-600">
            <CheckCircle2 className="size-8" />
          </div>
          <h2 className="mt-4 font-display text-2xl font-bold text-slate-900">Registration submitted</h2>
          <p className="mt-2 text-sm text-slate-500">
            Thanks for registering with {appName}. Our team will review your details and approve your account.
            Once approved, your login credentials will be sent to <span className="font-medium text-slate-700">{mobile}</span>.
          </p>
          <Button size="lg" className="mt-8 w-full" onClick={() => navigate('/login')}>
            Back to sign in
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-canvas px-6 py-10">
      <div className="mx-auto w-full max-w-2xl animate-rise">
        <BrandLockup tone="light" size="lg" className="mb-8" />

        <h1 className="font-display text-2xl font-bold text-slate-900">Create your account</h1>
        <p className="mt-1 text-sm text-slate-500">
          Verify your mobile number, fill in your details, and submit for approval.
        </p>

        {/* Step 1 — mobile verification gate */}
        <section className="mt-8 rounded-2xl border border-slate-200 bg-white p-5">
          <div className="flex items-center gap-2">
            <ShieldCheck className="size-4 text-brand-600" />
            <h2 className="font-display text-sm font-bold uppercase tracking-wide text-slate-700">
              1. Verify mobile number
            </h2>
            {mobileVerified && (
              <span className="ml-auto flex items-center gap-1 rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-600">
                <BadgeCheck className="size-3.5" /> Verified
              </span>
            )}
          </div>

          <div className="mt-4 grid gap-4 sm:grid-cols-2">
            <div className="flex items-end gap-2">
              <Input
                label="Mobile number"
                placeholder="10-digit mobile number"
                inputMode="numeric"
                maxLength={10}
                className="flex-1"
                value={mobile}
                disabled={mobileVerified}
                onChange={(e) => setMobile(e.target.value.replace(/\D/g, ''))}
              />
              {!mobileVerified && (
                <Button variant="secondary" loading={otpBusy} onClick={sendOtp} className="mb-0.5 shrink-0">
                  {otpSent ? 'Resend' : 'Send OTP'}
                </Button>
              )}
            </div>

            {otpSent && !mobileVerified && (
              <div className="flex items-end gap-2">
                <Input
                  label="OTP"
                  placeholder="Enter OTP"
                  inputMode="numeric"
                  maxLength={6}
                  className="flex-1"
                  value={otp}
                  onChange={(e) => setOtp(e.target.value.replace(/\D/g, ''))}
                />
                <Button loading={otpBusy} onClick={verifyOtp} className="mb-0.5 shrink-0">
                  Verify
                </Button>
              </div>
            )}
          </div>
        </section>

        {/* Step 2 — details (locked until mobile verified) */}
        <fieldset
          disabled={!mobileVerified}
          className={mobileVerified ? '' : 'pointer-events-none opacity-50'}
        >
          <section className="mt-5 rounded-2xl border border-slate-200 bg-white p-5">
            <h2 className="font-display text-sm font-bold uppercase tracking-wide text-slate-700">
              2. Business &amp; personal details
            </h2>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <Input label="Full name *" value={form.PersonName} onChange={(e) => set('PersonName', e.target.value)} />
              <Input label="Firm / shop name" value={form.FirmName} onChange={(e) => set('FirmName', e.target.value)} />
              <Input label="Email address" type="email" value={form.EmailID} onChange={(e) => set('EmailID', e.target.value)} />
              <Input
                label="WhatsApp number"
                inputMode="numeric"
                maxLength={10}
                value={form.WhatsAppNumber}
                onChange={(e) => set('WhatsAppNumber', e.target.value.replace(/\D/g, ''))}
              />
              <Select label="Role *" value={form.UserTypeID} onChange={(e) => set('UserTypeID', e.target.value)}>
                <option value="">Select a role</option>
                {roles.map((r) => (
                  <option key={r.UserTypeID} value={r.UserTypeID}>
                    {r.UserType}
                  </option>
                ))}
              </Select>
              <Input label="Referral code" value={form.ReferralCode} onChange={(e) => set('ReferralCode', e.target.value)} />
            </div>
          </section>

          <section className="mt-5 rounded-2xl border border-slate-200 bg-white p-5">
            <h2 className="font-display text-sm font-bold uppercase tracking-wide text-slate-700">3. KYC documents</h2>
            <div className="mt-4 grid gap-4 sm:grid-cols-3">
              <Input
                label="PAN number"
                maxLength={10}
                value={form.PANNo}
                onChange={(e) => set('PANNo', e.target.value.toUpperCase())}
              />
              <Input
                label="Aadhaar number"
                inputMode="numeric"
                maxLength={12}
                value={form.AadharNo}
                onChange={(e) => set('AadharNo', e.target.value.replace(/\D/g, ''))}
              />
              <Input
                label="GST number"
                maxLength={15}
                value={form.GSTNo}
                onChange={(e) => set('GSTNo', e.target.value.toUpperCase())}
              />
            </div>
          </section>

          <section className="mt-5 rounded-2xl border border-slate-200 bg-white p-5">
            <h2 className="font-display text-sm font-bold uppercase tracking-wide text-slate-700">4. Address</h2>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <Input
                label="Address"
                className="sm:col-span-2"
                value={form.Address}
                onChange={(e) => set('Address', e.target.value)}
              />
              <Input
                label="Pincode"
                inputMode="numeric"
                maxLength={6}
                value={form.Pincode}
                onChange={(e) => set('Pincode', e.target.value.replace(/\D/g, ''))}
                onBlur={onPincodeBlur}
              />
              <Input label="City" value={form.CityName} onChange={(e) => set('CityName', e.target.value)} />
              <Input label="District" value={form.District} onChange={(e) => set('District', e.target.value)} />
              <Autocomplete
                label="State"
                placeholder="Search a state…"
                value={form.StateID}
                options={states.map((s) => ({ value: String(s.StateID), label: s.StateName }))}
                onChange={(v) => set('StateID', String(v))}
              />
            </div>
          </section>
        </fieldset>

        {errorBanner && (
          <div className="mt-5 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {errorBanner}
          </div>
        )}

        <Button
          size="lg"
          className="mt-6 w-full"
          loading={submitBusy}
          disabled={!mobileVerified}
          onClick={submit}
        >
          Submit for approval
        </Button>

        <Link
          to="/login"
          className="mt-6 flex items-center justify-center gap-1.5 text-sm text-slate-500 hover:text-brand-600"
        >
          <ArrowLeft className="size-4" /> Already have an account? Sign in
        </Link>
      </div>
    </div>
  );
}
