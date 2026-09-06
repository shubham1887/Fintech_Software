import { useMemo, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { ArrowLeft, Cable, IndianRupee, Route, ScrollText } from 'lucide-react';
import { getField } from '@/lib/row';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { useProviders } from './providerShared';
import { EndpointsPanel } from './ProviderEndpointsScreen';
import { OperatorMappingPanel } from './ProviderOperatorsScreen';
import { RulesPanel } from './ProviderRulesScreen';
import { ProviderCommissionPanel } from './ProviderCommissionScreen';

const TABS = [
  { key: 'request', label: 'Request', icon: Cable, hint: 'Endpoint URL / body templates the gateway fires' },
  { key: 'configuration', label: 'Configuration', icon: Route, hint: 'Operator codes routed through this provider' },
  { key: 'response', label: 'Response', icon: ScrollText, hint: 'How provider replies map to Success / Failed / Pending' },
  { key: 'commission', label: 'Commission', icon: IndianRupee, hint: 'What this provider pays us per operator' },
] as const;

type TabKey = (typeof TABS)[number]['key'];

/**
 * Provider Setting — one page, three tabs (Request / Configuration / Response), reached from
 * the single "Setting" button on the provider list. Everything about talking to a provider
 * lives here; no separate menu pages.
 */
export function ProviderSettingScreen() {
  const params = new URLSearchParams(useLocation().search);
  const [providerId, setProviderId] = useState(params.get('id') ?? '');
  const [tab, setTab] = useState<TabKey>((params.get('tab') as TabKey) || 'request');
  const providers = useProviders();

  const providerName = useMemo(() => {
    const row = (providers.data ?? []).find((p) => String(getField(p, 'ProviderID')) === providerId);
    return row ? String(getField(row, 'ProviderName') ?? '') : (params.get('name') ?? '');
  }, [providers.data, providerId, params]);

  const active = TABS.find((t) => t.key === tab) ?? TABS[0];

  return (
    <>
      <Link to="/s/ProvidersAdd">
        <Button variant="ghost" size="sm" className="mb-2"><ArrowLeft className="size-4" /> Back to providers</Button>
      </Link>
      <PageHeader
        title={providerName ? `Setting — ${providerName}` : 'Provider setting'}
        subtitle={active.hint}
      />

      {/* ── Provider switcher + tab bar ──────────────────────────────────────── */}
      <div className="mb-6 rounded-card border border-slate-200/70 bg-white p-4 shadow-card">
        <div className="flex flex-wrap items-end justify-between gap-4">
          <div className="w-64">
            <Select label="Provider" value={providerId} onChange={(e) => setProviderId(e.target.value)}>
              <option value="">Select provider…</option>
              {(providers.data ?? []).map((p, i) => (
                <option key={i} value={String(getField(p, 'ProviderID'))}>{String(getField(p, 'ProviderName') ?? '')}</option>
              ))}
            </Select>
          </div>
          <div className="inline-flex overflow-hidden rounded-xl border border-slate-200 bg-slate-50">
            {TABS.map(({ key, label, icon: Icon }) => (
              <button
                key={key}
                type="button"
                onClick={() => setTab(key)}
                className={cn(
                  'flex items-center gap-2 px-5 py-2.5 text-sm font-semibold transition-colors',
                  tab === key ? 'bg-brand-600 text-white shadow-sm' : 'text-slate-600 hover:bg-slate-100',
                )}
              >
                <Icon className="size-4" />
                {label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {!providerId ? (
        <div className="rounded-card border border-slate-200/70 bg-white p-10 text-center text-sm text-slate-400 shadow-card">
          Pick a provider to configure it.
        </div>
      ) : (
        <>
          {tab === 'request' && <EndpointsPanel providerId={providerId} />}
          {tab === 'configuration' && <OperatorMappingPanel providerId={providerId} />}
          {tab === 'response' && <RulesPanel providerId={providerId} />}
          {tab === 'commission' && <ProviderCommissionPanel providerId={providerId} />}
        </>
      )}
    </>
  );
}
