import { useMemo, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { ArrowLeft, Cable, IndianRupee, PlugZap, Route, ScrollText } from 'lucide-react';
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

  const providerRow = useMemo(
    () => (providers.data ?? []).find((p) => String(getField(p, 'ProviderID')) === providerId),
    [providers.data, providerId],
  );
  const providerName = providerRow
    ? String(getField(providerRow, 'ProviderName') ?? '')
    : (params.get('name') ?? '');

  // Pay1 and JaiJui are compiled integrations: the gateway calls their library, not a URL
  // template, so the Request and Response tabs have nothing to fill in. Without saying so, the
  // Request tab reads "No endpoints yet — add the Transaction endpoint first" on a provider
  // that needs none, and the admin concludes it is unconfigured and stops there.
  const isCoded = Boolean(getField(providerRow ?? {}, 'IsCodedIntegration'));
  const codedFamily = String(getField(providerRow ?? {}, 'CodedFamily') ?? '');

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

      {isCoded && (tab === 'request' || tab === 'response') && (
        <div className="mb-6 flex gap-3 rounded-card border border-info-500/20 bg-info-50 p-4">
          <PlugZap className="mt-0.5 size-5 shrink-0 text-info-600" />
          <div className="text-sm text-info-600">
            <p className="font-semibold text-slate-800">
              {providerName} is a built-in integration{codedFamily ? ` for ${codedFamily}` : ''} — nothing to add here.
            </p>
            <p className="mt-1 text-slate-600">
              It is called through its own library, so it has no URL templates and no response
              rules. Three things make it live:
            </p>
            <ol className="mt-2 list-decimal space-y-1 pl-5 text-slate-600">
              <li>Its credentials, on the provider row itself (API Providers → Edit).</li>
              <li>
                Its operators, on the{' '}
                <button type="button" className="font-semibold text-brand-700 underline" onClick={() => setTab('configuration')}>
                  Configuration
                </button>{' '}
                tab — one row per operator, carrying the provider&apos;s own code for it.
              </li>
              <li>
                Its priority, in{' '}
                <Link to="/s/ProvidersSequence" className="font-semibold text-brand-700 underline">
                  Provider Routing
                </Link>.
              </li>
            </ol>
          </div>
        </div>
      )}

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
