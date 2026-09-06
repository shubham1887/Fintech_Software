import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { Lock, ShieldCheck, Smartphone, TrendingUp, Zap } from 'lucide-react';
import { useAuth } from '@/features/auth/AuthContext';
import { useBrand } from '@/theme/BrandProvider';
import { useToast } from '@/components/ui/Toast';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { BrandGlyph } from '@/components/ui/BrandMark';

const schema = z.object({
  mobileNumber: z.string().regex(/^\d{10}$/, 'Enter a valid 10-digit mobile number'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const { isAuthenticated, signIn } = useAuth();
  const { appName, tagline, loginHeadline, loginSubtext } = useBrand();
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

  if (isAuthenticated) return <Navigate to="/dashboard" replace />;

  const from = (location.state as { from?: string } | null)?.from ?? '/dashboard';

  const onSubmit = async (values: FormValues) => {
    const res = await signIn(values);
    if (res.ok) {
      toast.success('Signed in successfully.');
      navigate(from, { replace: true });
    } else {
      toast.error(res.message);
    }
  };

  return (
    <div className="flex min-h-screen bg-canvas">
      {/* Brand panel */}
      <div className="relative hidden w-1/2 overflow-hidden bg-ink-950 lg:block">
        <div className="absolute -left-24 -top-24 size-96 rounded-full bg-brand-600/30 blur-3xl" />
        <div className="absolute bottom-0 right-0 size-96 rounded-full bg-brand-500/20 blur-3xl" />
        <div className="relative flex h-full flex-col justify-between p-12 text-white">
          <div className="flex items-center gap-3">
            <BrandGlyph className="size-11 rounded-2xl" />
            <span className="font-display text-xl font-bold">
              {appName} {tagline}
            </span>
          </div>

          <div className="max-w-md">
            <h1 className="font-display text-4xl font-extrabold leading-tight">{loginHeadline}</h1>
            <p className="mt-4 text-ink-300">{loginSubtext}</p>
            <ul className="mt-8 space-y-3 text-sm text-ink-200">
              <Feature icon={Zap} text="Real-time recharge & money-movement pipelines" />
              <Feature icon={ShieldCheck} text="AES-encrypted, session-verified API access" />
              <Feature icon={TrendingUp} text="Live dashboards & reconciliation reports" />
            </ul>
          </div>

          <p className="text-xs text-ink-400">© {new Date().getFullYear()} Fino. All rights reserved.</p>
        </div>
      </div>

      {/* Form panel */}
      <div className="flex w-full items-center justify-center px-6 lg:w-1/2">
        <div className="w-full max-w-sm animate-rise">
          <div className="mb-8 lg:hidden">
            <BrandGlyph className="size-11 rounded-2xl" />
          </div>

          <h2 className="font-display text-2xl font-bold text-slate-900">Welcome back</h2>
          <p className="mt-1 text-sm text-slate-500">Sign in to your Fino account to continue.</p>

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
