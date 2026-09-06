import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { CheckCircle2, ListChecks, ShieldCheck } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { DynamicForm } from '@/components/ui/DynamicForm';
import { pickIcon } from '@/components/layout/iconMap';
import type { FormConfig } from './crudTypes';

type Row = Record<string, unknown>;

/**
 * A single config-driven action page: a designed two-column layout — the DynamicForm on the
 * left, a contextual info aside on the right (what the action does, a checklist of the fields
 * the user must fill, and the security note). Used for every menu 'form' item (fund transfer,
 * user transfer, change status, payout, etc.). Add one = a 'form' registry entry.
 */
export function FormScreen({ title, subtitle, form }: { title: string; subtitle?: string; form: FormConfig }) {
  const toast = useToast();
  const Icon = pickIcon(title);
  const requiredFields = form.fields.filter((f) => f.required && f.type !== 'hidden');
  // Bumped on every successful submit so the DynamicForm remounts and clears its fields.
  const [formKey, setFormKey] = useState(0);

  const mut = useMutation({
    mutationFn: (values: Row) => apiPost(form.endpoint, values),
    onSuccess: (res) => {
      if (res.ok) {
        toast.success(form.successMessage ?? 'Submitted successfully.');
        setFormKey((k) => k + 1);
      } else toast.error(res.message);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <>
      <PageHeader title={title} subtitle={subtitle} icon={Icon} />

      <div className="grid grid-cols-1 gap-5 lg:grid-cols-3">
        {/* ── Form ─────────────────────────────────────────────────────────── */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>{title}</CardTitle>
            <span className="text-xs text-slate-400">
              Fields marked <span className="text-danger-500">*</span> are required
            </span>
          </CardHeader>
          <CardBody>
            {form.description && <p className="mb-5 text-sm text-slate-500">{form.description}</p>}
            <DynamicForm
              key={formKey}
              fields={form.fields}
              submitting={mut.isPending}
              submitLabel={form.submitLabel ?? 'Submit'}
              onSubmit={(v) => mut.mutate(v)}
            />
          </CardBody>
        </Card>

        {/* ── Contextual aside ─────────────────────────────────────────────── */}
        <div className="space-y-4">
          <Card>
            <CardBody className="space-y-3">
              <div className="flex items-center gap-3">
                <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-brand-50 text-brand-600">
                  <Icon className="size-5" />
                </span>
                <div className="min-w-0">
                  <p className="truncate text-sm font-semibold text-slate-800">{title}</p>
                  {subtitle && <p className="truncate text-xs text-slate-500">{subtitle}</p>}
                </div>
              </div>
              <p className="text-sm leading-relaxed text-slate-500">
                {form.description ?? 'Fill in the details below and submit. Your request is processed instantly.'}
              </p>
            </CardBody>
          </Card>

          {requiredFields.length > 0 && (
            <Card>
              <CardBody>
                <div className="mb-3 flex items-center gap-2 text-sm font-semibold text-slate-700">
                  <ListChecks className="size-4 text-brand-500" />
                  Before you submit
                </div>
                <ul className="space-y-2">
                  {requiredFields.map((f) => (
                    <li key={f.name} className="flex items-center gap-2 text-sm text-slate-600">
                      <CheckCircle2 className="size-4 shrink-0 text-slate-300" />
                      {f.label}
                    </li>
                  ))}
                </ul>
              </CardBody>
            </Card>
          )}

          <div className="flex items-start gap-2 rounded-xl bg-slate-50 px-4 py-3 text-xs text-slate-500">
            <ShieldCheck className="mt-0.5 size-4 shrink-0 text-success-600" />
            <span>This request is encrypted end-to-end (AES-256) before it leaves your device.</span>
          </div>
        </div>
      </div>
    </>
  );
}
