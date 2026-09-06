import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ArrowLeft, CheckCircle2, KeyRound, Lock, Smartphone } from 'lucide-react';
import { useBrand } from '@/theme/BrandProvider';
import { useToast } from '@/components/ui/Toast';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { forgotPasswordGenerateOtp, forgotPasswordVerifyOtp } from '@/api/password.api';
import { BrandGlyph } from '@/components/ui/BrandMark';

type Step = 'mobile' | 'otp' | 'done';

export default function ForgotPasswordPage() {
  const { appName } = useBrand();
  const toast = useToast();
  const navigate = useNavigate();

  const [step, setStep] = useState<Step>('mobile');
  const [mobile, setMobile] = useState('');
  const [otp, setOtp] = useState('');
  const [busy, setBusy] = useState(false);

  const sendOtp = async () => {
    if (!/^\d{10}$/.test(mobile)) {
      toast.error('Enter a valid 10-digit mobile number.');
      return;
    }
    setBusy(true);
    const res = await forgotPasswordGenerateOtp(mobile);
    setBusy(false);
    if (res.ok) {
      toast.success('OTP sent to your registered mobile number.');
      setStep('otp');
    } else {
      toast.error(res.message);
    }
  };

  const verifyOtp = async () => {
    if (!/^\d{4,6}$/.test(otp)) {
      toast.error('Enter the OTP you received.');
      return;
    }
    setBusy(true);
    const res = await forgotPasswordVerifyOtp(mobile, otp);
    setBusy(false);
    if (res.ok) {
      setStep('done');
    } else {
      toast.error(res.message);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-canvas px-6">
      <div className="w-full max-w-sm animate-rise">
        <div className="mb-8 flex items-center gap-3">
          <BrandGlyph className="size-11 rounded-2xl" />
          <span className="font-display text-lg font-bold text-slate-900">{appName}</span>
        </div>

        {step === 'mobile' && (
          <>
            <h2 className="font-display text-2xl font-bold text-slate-900">Forgot password</h2>
            <p className="mt-1 text-sm text-slate-500">
              Enter your registered mobile number and we&apos;ll send you an OTP to reset it.
            </p>
            <div className="mt-8 space-y-4">
              <Input
                label="Mobile number"
                placeholder="10-digit mobile number"
                inputMode="numeric"
                maxLength={10}
                leftIcon={<Smartphone className="size-4" />}
                value={mobile}
                onChange={(e) => setMobile(e.target.value.replace(/\D/g, ''))}
              />
              <Button size="lg" loading={busy} className="w-full" onClick={sendOtp}>
                Send OTP
              </Button>
            </div>
          </>
        )}

        {step === 'otp' && (
          <>
            <h2 className="font-display text-2xl font-bold text-slate-900">Enter OTP</h2>
            <p className="mt-1 text-sm text-slate-500">
              We sent a one-time code to <span className="font-medium text-slate-700">{mobile}</span>.
            </p>
            <div className="mt-8 space-y-4">
              <Input
                label="OTP"
                placeholder="Enter the OTP"
                inputMode="numeric"
                maxLength={6}
                leftIcon={<KeyRound className="size-4" />}
                value={otp}
                onChange={(e) => setOtp(e.target.value.replace(/\D/g, ''))}
              />
              <Button size="lg" loading={busy} className="w-full" onClick={verifyOtp}>
                Verify &amp; reset password
              </Button>
              <button
                type="button"
                className="w-full text-center text-sm text-slate-500 hover:text-brand-600"
                onClick={sendOtp}
                disabled={busy}
              >
                Didn&apos;t get it? Resend OTP
              </button>
            </div>
          </>
        )}

        {step === 'done' && (
          <div className="text-center">
            <div className="mx-auto grid size-14 place-items-center rounded-full bg-emerald-50 text-emerald-600">
              <CheckCircle2 className="size-8" />
            </div>
            <h2 className="mt-4 font-display text-2xl font-bold text-slate-900">Check your phone</h2>
            <p className="mt-2 text-sm text-slate-500">
              A temporary password has been sent to <span className="font-medium text-slate-700">{mobile}</span>.
              Sign in with it, then set a new password.
            </p>
            <Button size="lg" className="mt-8 w-full" onClick={() => navigate('/login')}>
              <Lock className="size-4" /> Back to sign in
            </Button>
          </div>
        )}

        {step !== 'done' && (
          <Link
            to="/login"
            className="mt-6 flex items-center justify-center gap-1.5 text-sm text-slate-500 hover:text-brand-600"
          >
            <ArrowLeft className="size-4" /> Back to sign in
          </Link>
        )}
      </div>
    </div>
  );
}
