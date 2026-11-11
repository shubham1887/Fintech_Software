import { useMemo, useState } from 'react';
import { Pencil, Plus, Power, Radio, Signal, Trash2, Wifi } from 'lucide-react';
import { fetchListPage } from '@/api/list.api';
import { useCsvExport } from '@/lib/exportAll';
import { getField as get } from '@/lib/row';
import { inr } from '@/lib/format';
import { cn } from '@/lib/cn';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { StatTile, STAT_STRIP } from '@/components/ui/StatTile';
import { Modal } from '@/components/ui/Modal';
import { DynamicForm } from '@/components/ui/DynamicForm';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { ReportActions, ReportFilterBar, ReportTable, type ReportTableColumn } from '@/components/report';
import { useCrud } from './useCrud';
import type { CustomScreenProps } from './registry';

type Row = Record<string, unknown>;
const str = (r: Row, k: string) => String(get(r, k) ?? '').trim();

/** A number only counts as a limit when it is actually set — 0 means "no limit configured". */
function limit(v: unknown) {
  const n = Number(v);
  return Number.isFinite(n) && n > 0 ? inr(n) : '—';
}

/**
 * Operator Details.
 *
 * This was a grid of cards showing an operator's name, opcode and recharge limits. Two problems:
 * it never showed the SERVICE the operator belongs to — the one field that says what the operator
 * actually is, and without which "Airtel" under Prepaid and "Airtel" under DTH are indistinguishable
 * — and a card per operator meant a network with sixty operators could not be scanned or compared
 * at all. It is now the same table every other admin list uses, with the service, the input rules
 * (number length, prefix) and the limits side by side, plus filters for service and status.
 */
export function OperatorsScreen({ def }: CustomScreenProps) {
  const crud = def.crud;
  const { rows, isFetching, adding, setAdding, editing, setEditing, createMut, updateMut, deleteMut, toggleMut, onDelete } = useCrud(
    crud ?? { idField: 'OperatorID', list: { endpoint: 'Operator/GetAll', payload: {} } },
  );

  const [serviceFilter, setServiceFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [search, setSearch] = useState('');
  const [applied, setApplied] = useState({ service: '', status: '', search: '' });
  const { exporting, runExport } = useCsvExport();

  const services = useMemo(
    () => Array.from(new Set(rows.map((r) => str(r, 'ServiceName')).filter(Boolean))).sort((a, b) => a.localeCompare(b)),
    [rows],
  );

  /**
    * The filter bar as one predicate. Written as a function rather than inline so the export can
    * narrow the rows it fetches by exactly what the table is showing.
    */
  const matchesFilters = (r: Row) => {
    const q = applied.search.toLowerCase();
    if (applied.service && str(r, 'ServiceName') !== applied.service) return false;
    if (applied.status === 'active' && !get(r, 'IsActive')) return false;
    if (applied.status === 'off' && get(r, 'IsActive')) return false;
    if (!q) return true;
    return ['OperatorName', 'Opcode', 'ServiceName', 'Prefix', 'HSNCode']
      .some((k) => str(r, k).toLowerCase().includes(q));
  };

  const filtered = useMemo(() => rows.filter(matchesFilters), [rows, applied]);

  const stats = useMemo(() => ({
    total: rows.length,
    active: rows.filter((r) => Boolean(get(r, 'IsActive'))).length,
    bbps: rows.filter((r) => Boolean(get(r, 'IsBBPS'))).length,
  }), [rows]);

  const runSearch = () => setApplied({ service: serviceFilter, status: statusFilter, search: search.trim() });
  const reset = () => {
    setServiceFilter(''); setStatusFilter(''); setSearch('');
    setApplied({ service: '', status: '', search: '' });
  };

  if (!crud) return null;

  const columns: ReportTableColumn<Row>[] = [
    { key: '_serial', label: '#', type: 'serial' },
    {
      key: 'OperatorName',
      label: 'Operator',
      render: (r) => (
        <div className="flex items-center gap-2">
          {Boolean(str(r, 'OperatorImagePath')) && (
            <img
              src={str(r, 'OperatorImagePath')}
              alt=""
              className="size-6 rounded object-contain"
              onError={(e) => { (e.currentTarget as HTMLImageElement).style.display = 'none'; }}
            />
          )}
          <span className="font-medium text-slate-800">{str(r, 'OperatorName') || '—'}</span>
        </div>
      ),
    },
    // The field the card grid never showed, and the reason two identically-named operators are
    // not the same operator.
    { key: 'ServiceName', label: 'Service' },
    { key: 'Opcode', label: 'Opcode' },
    {
      key: 'IsActive',
      label: 'Status',
      render: (r) => <Badge tone={get(r, 'IsActive') ? 'success' : 'danger'}>{get(r, 'IsActive') ? 'Active' : 'Off'}</Badge>,
    },
    {
      key: '_flags',
      label: 'Flags',
      render: (r) => {
        const flags: string[] = [];
        if (get(r, 'IsBBPS')) flags.push('BBPS');
        if (get(r, 'IsBillFetch')) flags.push('Bill fetch');
        if (get(r, 'IsDenominationApplicable')) flags.push('Denomination');
        return flags.length === 0
          ? <span className="text-slate-300">—</span>
          : <span className="text-xs text-slate-600">{flags.join(' · ')}</span>;
      },
    },
    { key: 'MinimumRechargeAmount', label: 'Min', align: 'right', render: (r) => <span className="tabular-nums">{limit(get(r, 'MinimumRechargeAmount'))}</span> },
    { key: 'MaximumRechargeAmount', label: 'Max', align: 'right', render: (r) => <span className="tabular-nums">{limit(get(r, 'MaximumRechargeAmount'))}</span> },
    { key: 'NumberLength', label: 'No. length', type: 'number', optional: true },
    { key: 'Prefix', label: 'Prefix', optional: true },
    { key: 'HSNCode', label: 'HSN', optional: true },
    { key: 'RepeatRechargeInMinute', label: 'Repeat (min)', type: 'number', optional: true },
    {
      key: '_actions',
      label: 'Actions',
      render: (r) => (
        <div className="flex items-center justify-end gap-1">
          {crud.update && (
            <Button variant="ghost" size="sm" loading={toggleMut.isPending} onClick={() => toggleMut.mutate(r)} title="Toggle active">
              <Power className={cn('size-4', get(r, 'IsActive') ? 'text-success-600' : 'text-slate-400')} />
            </Button>
          )}
          {crud.update && (
            <Button variant="ghost" size="sm" onClick={() => setEditing(r)} title="Edit">
              <Pencil className="size-4" />
            </Button>
          )}
          {crud.remove && (
            <Button
              variant="ghost"
              size="sm"
              className="text-danger-600 hover:bg-danger-50"
              loading={deleteMut.isPending}
              onClick={() => onDelete(r)}
              title="Delete"
            >
              <Trash2 className="size-4" />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={Radio}
        actions={
          <ReportActions
            total={filtered.length}
            loading={isFetching}
            exportLoading={exporting}
            onExport={() => runExport({
              filename: def.title,
              onScreen: filtered,
              fetchAll: (skip, take) => fetchListPage(crud.list.endpoint, crud.list.payload ?? {}, skip, take),
              filter: (all) => all.filter(matchesFilters),
            })}
            exportDisabled={filtered.length === 0}
          >
            {crud.create && (
              <Button size="sm" onClick={() => setAdding(true)}>
                <Plus className="size-4" />
                Add operator
              </Button>
            )}
          </ReportActions>
        }
      />

      <div className={cn(STAT_STRIP, "grid-cols-2 lg:grid-cols-3")}>
        <StatTile label="Total operators" value={stats.total} icon={Radio} tone="bg-brand-50 text-brand-600" loading={isFetching} />
        <StatTile label="Active" value={stats.active} icon={Signal} tone="bg-success-50 text-success-600" loading={isFetching} />
        <StatTile label="BBPS enabled" value={stats.bbps} icon={Wifi} tone="bg-info-50 text-info-600" loading={isFetching} />
      </div>

      <ReportFilterBar onSearch={runSearch} onReset={reset} loading={isFetching}>
        <Select label="Service" value={serviceFilter} onChange={(e) => setServiceFilter(e.target.value)}>
          <option value="">All services</option>
          {services.map((s) => <option key={s} value={s}>{s}</option>)}
        </Select>
        <Select label="Status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          <option value="">All</option>
          <option value="active">Active only</option>
          <option value="off">Off only</option>
        </Select>
        <Input label="Search" placeholder="Name / opcode / prefix…" value={search} onChange={(e) => setSearch(e.target.value)} />
      </ReportFilterBar>

      <ReportTable
        columns={columns}
        rows={filtered}
        loading={isFetching}
        rowKey={(r, i) => String(get(r, 'OperatorID') ?? i)}
        emptyTitle="No operators"
        emptyDescription="Nothing matches these filters. Add an operator to start routing recharges and bill payments."
      />

      {crud.create && (
        <Modal open={adding} onClose={() => setAdding(false)} title={`Add ${def.title}`} size="lg">
          <DynamicForm
            fields={crud.create.fields}
            submitting={createMut.isPending}
            submitLabel="Create"
            onSubmit={(v) => createMut.mutate(v)}
            onCancel={() => setAdding(false)}
          />
        </Modal>
      )}

      {crud.update && (
        <Modal open={Boolean(editing)} onClose={() => setEditing(null)} title={`Edit ${def.title}`} size="lg">
          {editing && (
            <DynamicForm
              fields={crud.update.fields}
              initial={editing}
              submitting={updateMut.isPending}
              submitLabel="Save changes"
              onSubmit={(v) => updateMut.mutate(v)}
              onCancel={() => setEditing(null)}
            />
          )}
        </Modal>
      )}
    </>
  );
}
