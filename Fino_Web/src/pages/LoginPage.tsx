import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { ArrowRight, Lock, ShieldCheck, Smartphone, TrendingUp, Zap } from 'lucide-react';
import { useAuth } from '@/features/auth/AuthContext';
import { useBrand } from '@/theme/BrandProvider';
import { useToast } from '@/components/ui/Toast';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { BrandLockup } from '@/components/ui/BrandMark';
import { roleLabel } from '@/lib/roles';
import type { LoginProfileOption } from '@/types/auth';

const schema = z.object({
  mobileNumber: z.string().regex(/^\d{10}$/, 'Enter a valid 10-digit mobile number'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const { isAuthenticated, signIn, completeTwoFactor, switchProfile } = useAuth();
  const { appName, loginHeadline, loginSubtext } = useBrand();
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { mobileNumber: '', password: '' },
  });

  /**
   * The credentials that passed step one, held in memory only for as long as the code box is up.
   *
   * The server keeps no half-finished login, so the password has to go up a second time with the
   * code. Nothing is written to storage: refreshing the page drops this and returns to step one.
   */
  const [pending, setPending] = useState<FormValues | null>(null);
  const [code, setCode] = useState('');
  const [codeBusy, setCodeBusy] = useState(false);

  /**
   * "Continue as" — shown only to someone whose mobile number carries two roles.
   *
   * The session is already established as their primary account by the time this appears, so
   * this is a choice, not a gate: picking the primary just goes on, and picking the other one
   * re-issues the session for it. Anyone else never sees this screen at all.
   */
  const [chooser, setChooser] = useState<LoginProfileOption[] | null>(null);
  const [switchingTo, setSwitchingTo] = useState(0);

  // The chooser renders while authenticated, so the usual "already signed in" redirect has to
  // stand aside for it — otherwise establishing the session would bounce straight past the
  // question we are trying to ask.
  if (isAuthenticated && chooser === null) return <Navigate to="/dashboard" replace />;

  const from = (location.state as { from?: string } | null)?.from ?? '/dashboard';

  const onSubmit = async (values: FormValues) => {
    const res = await signIn(values);
    if (res.ok) {
      if (res.chooseProfile) { setChooser(res.chooseProfile); return; }
      toast.success('Signed in successfully.');
      navigate(from, { replace: true });
      return;
    }
    if (res.twoFactorRequired) {
      setPending(values);
      setCode('');
      return;
    }
    toast.error(res.message);
  };

  const onSubmitCode = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!pending) return;
    // Accepts a 6-digit authenticator code or a recovery code, so no length check here —
    // the server decides which it is.
    if (!code.trim()) { toast.error('Enter the code from your authenticator app.'); return; }

    setCodeBusy(true);
    try {
      const res = await completeTwoFactor({ ...pending, code: code.trim() });
      if (res.ok) {
        if (res.chooseProfile) { setChooser(res.chooseProfile); setPending(null); return; }
        toast.success('Signed in successfully.');
        navigate(from, { replace: true });
        return;
      }
      toast.error(res.message);
      setCode('');
    } finally {
      setCodeBusy(false);
    }
  };

  /** Continue as the chosen login. The primary needs no call — it is already the session. */
  const onPickProfile = async (option: LoginProfileOption) => {
    if (option.IsPrimary) {
      toast.success('Signed in successfully.');
      navigate(from, { replace: true });
      return;
    }
    setSwitchingTo(option.UserID);
    try {
      const res = await switchProfile(option.UserID);
      if (!res.ok) { toast.error(res.message); return; }
      toast.success(`Signed in as ${roleLabel(option.UserTypeID, option.UserType)}.`);
      navigate(from, { replace: true });
    } finally {
      setSwitchingTo(0);
    }
  };

  return (
    <div className="flex min-h-screen bg-canvas">
      {/* Brand panel */}
      <div className="relative hidden w-1/2 overflow-hidden bg-ink-950 lg:block">
        <div className="absolute -left-24 -top-24 size-96 rounded-full bg-brand-600/30 blur-3xl" />
        <div className="absolute bottom-0 right-0 size-96 rounded-full bg-brand-500/20 blur-3xl" />
        <div className="relative flex h-full flex-col justify-between p-12 text-white">
          <BrandLockup tone="dark" size="lg" />

          <div className="max-w-md">
            <h1 className="font-display text-4xl font-extrabold leading-tight">{loginHeadline}</h1>
            <p className="mt-4 text-ink-300">{loginSubtext}</p>
            <ul className="mt-8 space-y-3 text-sm text-ink-200">
              <Feature icon={Zap} text="Real-time recharge & money-movement pipelines" />
              <Feature icon={ShieldCheck} text="AES-encrypted, session-verified API access" />
              <Feature icon={TrendingUp} text="Live dashboards & reconciliation reports" />
            </ul>
          </div>

          <p className="text-xs text-ink-400">© {new Date().getFullYear()} {appName}. All rights reserved.</p>
        </div>
      </div>

      {/* Form panel */}
      <div className="flex w-full items-center justify-center px-6 lg:w-1/2">
        <div className="w-full max-w-sm animate-rise">
          <BrandLockup tone="light" size="lg" className="mb-8 lg:hidden" />

          {chooser ? (
            <>
              <h2 className="font-display text-2xl font-bold text-slate-900">Continue as</h2>
              <p className="mt-1 text-sm text-slate-500">
                This number has more than one login. Pick the one you want to work in — you can
                switch at any time from the account menu.
              </p>

              <div className="mt-8 space-y-3">
                {chooser.map((option) => (
                  <button
                    key={option.UserID}
                    type="button"
                    disabled={switchingTo > 0}
                    onClick={() => void onPickProfile(option)}
                    className="flex w-full items-center gap-3 rounded-xl border border-slate-200 bg-white p-4 text-left shadow-sm transition-colors hover:border-brand-400 hover:bg-brand-50/40 disabled:opacity-60"
                  >
                    <span className="grid size-10 shrink-0 place-items-center rounded-full bg-brand-600 text-sm font-semibold text-white">
                      {(option.FirmName || '?').slice(0, 1).toUpperCase()}
                    </span>
                    <span className="min-w-0 flex-1">
                      <span className="block truncate text-sm font-semibold text-slate-800">
                        {roleLabel(option.UserTypeID, option.UserType)}
                      </span>
                      <span className="block truncate text-xs text-slate-500">
                        {option.FirmName}
                        {option.UserCode ? ` · ${option.UserCode}` : ''}
                      </span>
                    </span>
                    <ArrowRight className="size-4 shrink-0 text-slate-400" />
                  </button>
                ))}
              </div>

              <p className="mt-6 text-center text-xs text-slate-400">
                Both logins use this same mobile number and password.
              </p>
            </>
          ) : pending ? (
            <>
              <h2 className="font-display text-2xl font-bold text-slate-900">Two-step verification</h2>
              <p className="mt-1 text-sm text-slate-500">
                Open your authenticator app and enter the 6-digit code for {appName}.
              </p>

              <form onSubmit={onSubmitCode} className="mt-8 space-y-4">
                <Input
                  label="Authenticator code"
                  placeholder="000000"
                  inputMode="numeric"
                  autoComplete="one-time-code"
                  autoFocus
                  // Long enough for a recovery code (XXXXX-XXXXX) as well as the 6 digits.
                  maxLength={14}
                  leftIcon={<ShieldCheck className="size-4" />}
                  value={code}
                  onChange={(e) => setCode(e.target.value)}
                  hint="Codes change every 30 seconds. You can also use one of your recovery codes."
                />

                <Button type="submit" size="lg" loading={codeBusy} className="w-full">
                  Verify and sign in
                </Button>

                <button
                  type="button"
                  className="w-full text-center text-sm font-medium text-slate-500 hover:text-slate-700"
                  onClick={() => { setPending(null); setCode(''); }}
                >
                  Use a different account
                </button>
              </form>

              <p className="mt-6 text-center text-xs text-slate-400">
                Lost your phone and your recovery codes? Ask your administrator to reset two-step
                verification on your account.
              </p>
            </>
          ) : (
          <>
          <h2 className="font-display text-2xl font-bold text-slate-900">Welcome back</h2>
          <p className="mt-1 text-sm text-slate-500">Sign in to your {appName} account to continue.</p>

          <form onSubmit={handleSubmit(onSubmit)} className="mt-8 space-y-4">
            <Input
              label="Mobile number"
              placeholder="10-digit mobile number"
              inputMode="numeric"
              maxLength={10}
              autoComplete="username"
              leftIcon={<Smartphone className="size-4" />}
              error={errors.mobileNumber?.message}
              {...register('mobileNumber')}
            />
            <Input
              label="Password"
              type="password"
              placeholder="Your password"
              autoComplete="current-password"
              leftIcon={<Lock className="size-4" />}
              error={errors.password?.message}
              {...register('password')}
            />

            <div className="flex justify-end">
              <Link to="/forgot-password" className="text-sm font-medium text-brand-600 hover:text-brand-700">
                Forgot password?
              </Link>
            </div>

            <Button type="submit" size="lg" loading={isSubmitting} className="w-full">
              Sign in
            </Button>
          </form>

          <p className="mt-6 text-center text-sm text-slate-500">
            Don&apos;t have an account?{' '}
            <Link to="/register" className="font-semibold text-brand-600 hover:text-brand-700">
              Create account
            </Link>
          </p>

          <p className="mt-4 text-center text-xs text-slate-400">
            Protected by encrypted, session-verified access.
          </p>
          </>
          )}
        </div>
      </div>
    </div>
  );
}

function Feature({ icon: Icon, text }: { icon: typeof Zap; text: string }) {
  return (
    <li className="flex items-center gap-3">
      <span className="grid size-8 place-items-center rounded-lg bg-white/10 text-brand-300">
        <Icon className="size-4" />
      </span>
      {text}
    </li>
  );
}
