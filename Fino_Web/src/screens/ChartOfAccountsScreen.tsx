import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { BookOpen, Download, ShieldCheck } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { useCsvDownload } from '@/lib/exportAll';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Spinner } from '@/components/ui/Spinner';
import { pickIcon } from '@/components/layout/iconMap';
import type { CustomScreenProps } from './registry';

interface AccountRow {
  AcctAccountID: number;
  Code: string;
  Name: string;
  AccountType: string;
  NormalBalance: string;
  IsControl: boolean;
  IsActive: boolean;
}

/** The five classes, in the order a balance sheet reads. */
const TYPE_ORDER = ['Asset', 'Liability', 'Equity', 'Income', 'Expense'];

const typeTone = (t: string): 'success' | 'warning' | 'danger' | 'neutral' | 'info' => {
  switch (t) {
    case 'Asset': return 'info';
    case 'Liability': return 'warning';
    case 'Income': return 'success';
    case 'Expense': return 'danger';
    default: return 'neutral';
  }
};

export function ChartOfAccountsScreen({ def }: CustomScreenProps) {
  const downloadCsv = useCsvDownload();
  const accounts = useQuery({
    queryKey: ['acct-accounts'],
    queryFn: async (): Promise<AccountRow[]> => {
      const res = await apiPost<AccountRow[]>('AcctAccount/Select', {});
      if (!res.ok) throw new Error(res.message || 'Could not load the chart of accounts.');
      return res.data ?? [];
    },
  });

  const rows = accounts.data ?? [];

  const grouped = useMemo(() => {
    const byType = new Map<string, AccountRow[]>();
    for (const r of rows) {
      const k = r.AccountType || 'Other';
      if (!byType.has(k)) byType.set(k, []);
      byType.get(k)!.push(r);
    }
    for (const list of byType.values()) list.sort((a, b) => a.Code.localeCompare(b.Code));
    // Balance-sheet order, with anything unrecognised after the five known classes.
    const rank = (t: string) => {
      const i = TYPE_ORDER.indexOf(t);
      return i === -1 ? TYPE_ORDER.length : i;
    };
    return [...byType.entries()].sort((a, b) => rank(a[0]) - rank(b[0]));
  }, [rows]);

  const download = () =>
    downloadCsv(
      [...rows]
        .sort((a, b) => a.Code.localeCompare(b.Code))
        .map((r) => ({
          Code: r.Code,
          Name: r.Name,
          Type: r.AccountType,
          NormalBalance: r.NormalBalance,
          Control: r.IsControl ? 'Yes' : 'No',
          Active: r.IsActive ? 'Yes' : 'No',
        })),
      'chart-of-accounts.csv',
    );

  return (
    <div className="space-y-4">
      <PageHeader
        title={def.title}
        subtitle="The accounts the posting rules and the integrity checks refer to — by code, never by id"
        icon={pickIcon(def.title)}
        actions={
          <Button variant="ghost" onClick={download} disabled={rows.length === 0}>
            <Download className="mr-1.5 size-4" />
            CSV
          </Button>
        }
      />

      <div className="flex gap-2.5 rounded-lg border border-slate-200 bg-white p-3 text-sm text-slate-500">
        <ShieldCheck className="mt-0.5 size-4 shrink-0 text-slate-400" />
        <p>
          A <strong>control account</strong> is the ledger's own figure for a balance that also lives in an
          operational table — user wallets, provider stock. The daily check on{' '}
          <strong>Trial Balance &amp; Integrity</strong> proves the two still agree, which is the whole reason
          for keeping books next to the wallets.
        </p>
      </div>

      {accounts.isFetching ? (
        <Card>
          <div className="flex min-h-[30vh] items-center justify-center">
            <Spinner className="size-6 text-brand-500" />
          </div>
        </Card>
      ) : accounts.isError ? (
        <Card>
          <CardBody>
            <EmptyState title="Could not load" description={(accounts.error as Error).message} />
          </CardBody>
        </Card>
      ) : rows.length === 0 ? (
        <Card>
          <CardBody>
            <EmptyState
              title="No accounts"
              description="The chart of accounts is seeded by the install script (65_seed_accounting_defaults.sql). If it is empty, that seed has not been run on this database."
            />
          </CardBody>
        </Card>
      ) : (
        <div className="grid gap-4 lg:grid-cols-2">
          {grouped.map(([type, list]) => (
            <Card key={type}>
              <CardBody className="pb-2">
                <div className="flex items-center justify-between gap-2">
                  <div className="flex items-center gap-2 text-sm font-semibold text-slate-700">
                    <BookOpen className="size-4 text-slate-400" />
                    {type}
                  </div>
                  <Badge tone={typeTone(type)}>{list.length}</Badge>
                </div>
              </CardBody>
              <div className="px-3 pb-3">
                <table className="w-full text-sm">
                  <tbody className="divide-y divide-slate-100">
                    {list.map((r) => (
                      <tr key={r.AcctAccountID} className="hover:bg-slate-50">
                        <td className="w-16 px-3 py-2 font-mono text-sm font-semibold text-slate-800">{r.Code}</td>
                        <td className="px-3 py-2 text-slate-700">
                          {r.Name}
                          {!r.IsActive && <span className="ml-2 text-xs text-slate-400">(inactive)</span>}
                        </td>
                        <td className="w-14 px-3 py-2 text-center">
                          <span className="font-mono text-xs text-slate-500">{r.NormalBalance}</span>
                        </td>
                        <td className="w-24 px-3 py-2 text-right">
                          {r.IsControl && <Badge tone="info">control</Badge>}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
