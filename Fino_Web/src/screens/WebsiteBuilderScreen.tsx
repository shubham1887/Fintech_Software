import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ArrowDown,
  ArrowUp,
  ExternalLink,
  Eye,
  FilePlus2,
  Globe,
  Plus,
  Save,
  Trash2,
} from 'lucide-react';
import {
  createWebsitePage,
  deleteWebsitePage,
  getWebsitePages,
  updateWebsitePage,
  type WebsitePagePayload,
  type WebsitePageRow,
} from '@/api/website.api';
import {
  FEATURE_ICON_OPTIONS,
  SECTION_LABELS,
  newSection,
  parseSections,
  type Section,
  type SectionType,
} from '@/site/sections';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';
import { cn } from '@/lib/cn';
import type { CustomScreenProps } from './registry';

interface Draft {
  title: string;
  slug: string;
  metaDescription: string;
  menuOrder: number;
  showInMenu: boolean;
  isPublished: boolean;
  sections: Section[];
}

const EMPTY_DRAFT: Draft = {
  title: '',
  slug: '',
  metaDescription: '',
  menuOrder: 1,
  showInMenu: true,
  isPublished: false,
  sections: [],
};

function draftFromRow(r: WebsitePageRow): Draft {
  return {
    title: r.title,
    slug: r.slug,
    metaDescription: r.metaDescription,
    menuOrder: r.menuOrder,
    showInMenu: r.showInMenu,
    isPublished: r.isPublished,
    sections: parseSections(r.sectionsJson),
  };
}

function toPayload(d: Draft): WebsitePagePayload {
  return {
    Slug: d.slug.trim().toLowerCase(),
    Title: d.title.trim(),
    MetaDescription: d.metaDescription.trim(),
    SectionsJson: JSON.stringify(d.sections),
    MenuOrder: d.menuOrder,
    ShowInMenu: d.showInMenu,
    IsPublished: d.isPublished,
  };
}

const slugify = (s: string) => s.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');

/** Console routes the website must not shadow — a page can't take these slugs. */
const RESERVED_SLUGS = new Set([
  'login', 'dashboard', 'recharge', 'transactions', 'appearance', 'theme', 'profile', 'coming-soon', 's', 'site',
]);

/** Admin page builder for the public website (WebsitePage master). */
export function WebsiteBuilderScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();

  const { data: pages = [], isLoading } = useQuery({ queryKey: ['website-pages'], queryFn: getWebsitePages });

  // selected: null = nothing, 'new' = creating, number = editing that page id
  const [selected, setSelected] = useState<number | 'new' | null>(null);
  const [draft, setDraft] = useState<Draft>(EMPTY_DRAFT);
  const [slugTouched, setSlugTouched] = useState(false);

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ['website-pages'] });
    qc.invalidateQueries({ queryKey: ['public-pages'] });
  };

  const saveMut = useMutation({
    mutationFn: () =>
      selected === 'new' ? createWebsitePage(toPayload(draft)) : updateWebsitePage(selected as number, toPayload(draft)),
    onSuccess: (res) => {
      if (!res.ok) {
        toast.error(res.message);
        return;
      }
      toast.success(selected === 'new' ? 'Page created.' : 'Page saved.');
      invalidate();
      if (selected === 'new') setSelected(null);
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => deleteWebsitePage(id),
    onSuccess: (res) => {
      if (!res.ok) {
        toast.error(res.message);
        return;
      }
      toast.success('Page deleted.');
      setSelected(null);
      invalidate();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  function selectPage(row: WebsitePageRow) {
    setSelected(row.id);
    setDraft(draftFromRow(row));
    setSlugTouched(true);
  }

  function startNew() {
    setSelected('new');
    setDraft({ ...EMPTY_DRAFT, menuOrder: pages.length + 1, sections: [newSection('hero')] });
    setSlugTouched(false);
  }

  const up = (patch: Partial<Draft>) => setDraft((d) => ({ ...d, ...patch }));

  const setSection = (i: number, s: Section) =>
    setDraft((d) => ({ ...d, sections: d.sections.map((x, j) => (j === i ? s : x)) }));
  const removeSection = (i: number) => setDraft((d) => ({ ...d, sections: d.sections.filter((_, j) => j !== i) }));
  const moveSection = (i: number, dir: -1 | 1) =>
    setDraft((d) => {
      const arr = [...d.sections];
      const j = i + dir;
      if (j < 0 || j >= arr.length) return d;
      [arr[i], arr[j]] = [arr[j], arr[i]];
      return { ...d, sections: arr };
    });
  const addSection = (t: SectionType) => setDraft((d) => ({ ...d, sections: [...d.sections, newSection(t)] }));

  const slugReserved = RESERVED_SLUGS.has(draft.slug.trim().toLowerCase());
  const canSave = draft.title.trim().length > 0 && /^[a-z0-9-]+$/.test(draft.slug.trim().toLowerCase()) && !slugReserved;

  return (
    <>
      <PageHeader
        title={def.title}
        subtitle={def.subtitle}
        icon={Globe}
        actions={
          <a
            href="/"
            target="_blank"
            rel="noreferrer"
            className="flex items-center gap-1.5 rounded-lg border border-brand-200 bg-brand-50 px-3 py-2 text-sm font-medium text-brand-700 transition-colors hover:bg-brand-100"
          >
            <Eye className="size-4" /> View website
          </a>
        }
      />

      <div className="grid items-start gap-4 lg:grid-cols-[300px,1fr]">
        {/* ------------------------------------------------ page list */}
        <Card>
          <CardHeader>
            <CardTitle>Pages</CardTitle>
            <button
              type="button"
              onClick={startNew}
              className="flex items-center gap-1 rounded-lg bg-brand-600 px-2.5 py-1.5 text-xs font-semibold text-white hover:bg-brand-700"
            >
              <FilePlus2 className="size-3.5" /> New page
            </button>
          </CardHeader>
          <CardBody className="space-y-1.5">
            {isLoading ? (
              <p className="py-6 text-center text-sm text-slate-400">Loading…</p>
            ) : pages.length === 0 && selected !== 'new' ? (
              <p className="py-6 text-center text-sm text-slate-400">
                No pages yet.
                <br />
                Create your Home page to get started.
              </p>
            ) : (
              pages.map((p) => (
                <button
                  key={p.id}
                  type="button"
                  onClick={() => selectPage(p)}
                  className={cn(
                    'w-full rounded-xl border p-3 text-left transition-colors',
                    selected === p.id ? 'border-brand-500 bg-brand-50/60 ring-1 ring-brand-200' : 'border-slate-200 hover:border-slate-300',
                  )}
                >
                  <span className="flex items-center justify-between gap-2">
                    <span className="truncate text-sm font-semibold text-slate-800">{p.title}</span>
                    <span
                      className={cn(
                        'shrink-0 rounded-full px-2 py-0.5 text-[11px] font-semibold',
                        p.isPublished ? 'bg-success-50 text-success-600' : 'bg-slate-100 text-slate-500',
                      )}
                    >
                      {p.isPublished ? 'Live' : 'Draft'}
                    </span>
                  </span>
                  <span className="mt-0.5 block text-xs text-slate-400">
                    {p.slug === 'home' ? '/' : `/${p.slug}`} · order {p.menuOrder}
                    {!p.showInMenu && ' · hidden from menu'}
                  </span>
                </button>
              ))
            )}
            {selected === 'new' && (
              <div className="rounded-xl border border-dashed border-brand-400 bg-brand-50/40 p-3 text-sm font-medium text-brand-700">
                New page (unsaved)
              </div>
            )}
          </CardBody>
        </Card>

        {/* ------------------------------------------------ editor */}
        {selected === null ? (
          <Card>
            <CardBody>
              <EmptyState
                title="Pick a page to edit"
                description="Select a page on the left, or create a new one. Build it from sections — hero slider, features, custom HTML and more — then publish."
              />
            </CardBody>
          </Card>
        ) : (
          <div className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>{selected === 'new' ? 'New page' : 'Page settings'}</CardTitle>
                {selected !== 'new' && (
                  <a
                    href={draft.slug === 'home' ? '/' : `/${draft.slug}`}
                    target="_blank"
                    rel="noreferrer"
                    className="flex items-center gap-1 text-xs font-medium text-brand-600 hover:text-brand-700"
                  >
                    Preview <ExternalLink className="size-3" />
                  </a>
                )}
              </CardHeader>
              <CardBody className="space-y-4">
                <div className="grid gap-4 sm:grid-cols-2">
                  <Field label="Page title">
                    <input
                      value={draft.title}
                      onChange={(e) => {
                        up({ title: e.target.value, ...(slugTouched ? {} : { slug: slugify(e.target.value) }) });
                      }}
                      placeholder="Home"
                      className={INPUT}
                    />
                  </Field>
                  <Field label="Slug (URL)" hint={draft.slug === 'home' ? '/' : `/${draft.slug || '…'}`}>
                    <input
                      value={draft.slug}
                      onChange={(e) => {
                        setSlugTouched(true);
                        up({ slug: slugify(e.target.value) });
                      }}
                      placeholder="home"
                      className={cn(INPUT, 'font-mono', slugReserved && 'border-danger-500')}
                    />
                    {slugReserved && (
                      <span className="mt-1 block text-xs text-danger-600">
                        “{draft.slug}” is a console route — pick a different slug.
                      </span>
                    )}
                  </Field>
                </div>
                <Field label="Meta description (SEO)">
                  <input
                    value={draft.metaDescription}
                    onChange={(e) => up({ metaDescription: e.target.value })}
                    placeholder="Short description for search engines"
                    className={INPUT}
                  />
                </Field>
                <div className="flex flex-wrap items-center gap-5">
                  <Field label="Menu order">
                    <input
                      type="number"
                      value={draft.menuOrder}
                      onChange={(e) => up({ menuOrder: Number(e.target.value) || 0 })}
                      className={cn(INPUT, 'w-24')}
                    />
                  </Field>
                  <label className="flex cursor-pointer items-center gap-2 pt-5 text-sm text-slate-700">
                    <input
                      type="checkbox"
                      checked={draft.showInMenu}
                      onChange={(e) => up({ showInMenu: e.target.checked })}
                      className="size-4 accent-[var(--color-brand-600)]"
                    />
                    Show in navbar
                  </label>
                  <label className="flex cursor-pointer items-center gap-2 pt-5 text-sm font-medium text-slate-700">
                    <input
                      type="checkbox"
                      checked={draft.isPublished}
                      onChange={(e) => up({ isPublished: e.target.checked })}
                      className="size-4 accent-[var(--color-brand-600)]"
                    />
                    Published (visible on the website)
                  </label>
                </div>
              </CardBody>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Sections</CardTitle>
                <span className="text-xs text-slate-400">Rendered top to bottom — reorder with the arrows</span>
              </CardHeader>
              <CardBody className="space-y-4">
                {draft.sections.length === 0 && (
                  <p className="rounded-xl border border-dashed border-slate-300 py-6 text-center text-sm text-slate-400">
                    No sections yet — add one below.
                  </p>
                )}
                {draft.sections.map((s, i) => (
                  <SectionEditor
                    key={i}
                    section={s}
                    index={i}
                    total={draft.sections.length}
                    onChange={(next) => setSection(i, next)}
                    onMove={(dir) => moveSection(i, dir)}
                    onRemove={() => removeSection(i)}
                  />
                ))}

                <div className="rounded-xl border border-dashed border-slate-300 p-3">
                  <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-400">Add section</p>
                  <div className="flex flex-wrap gap-2">
                    {(Object.keys(SECTION_LABELS) as SectionType[]).map((t) => (
                      <button
                        key={t}
                        type="button"
                        onClick={() => addSection(t)}
                        className="flex items-center gap-1.5 rounded-lg border border-slate-200 px-3 py-1.5 text-sm text-slate-600 transition-colors hover:border-brand-300 hover:bg-brand-50 hover:text-brand-700"
                      >
                        <Plus className="size-3.5" /> {SECTION_LABELS[t]}
                      </button>
                    ))}
                  </div>
                </div>
              </CardBody>
            </Card>

            <div className="flex flex-wrap items-center gap-2">
              <Button loading={saveMut.isPending} disabled={!canSave} onClick={() => saveMut.mutate()}>
                <Save className="size-4" /> {selected === 'new' ? 'Create page' : 'Save changes'}
              </Button>
              {selected !== 'new' && (
                <Button
                  variant="outline"
                  loading={deleteMut.isPending}
                  onClick={() => {
                    if (window.confirm(`Delete page "${draft.title}"? This cannot be undone.`)) {
                      deleteMut.mutate(selected as number);
                    }
                  }}
                >
                  <Trash2 className="size-4 text-danger-600" /> Delete
                </Button>
              )}
              {!draft.isPublished && (
                <span className="text-xs text-slate-400">Draft — tick “Published” above to make it visible.</span>
              )}
            </div>
          </div>
        )}
      </div>
    </>
  );
}

/* ------------------------------------------------------------------ shared bits */

const INPUT =
  'h-9 w-full rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-700 focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20';
const AREA =
  'w-full rounded-lg border border-slate-300 bg-white px-3 py-2 font-mono text-xs leading-relaxed text-slate-700 focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20';

function Field({ label, hint, children }: { label: string; hint?: string; children: React.ReactNode }) {
  return (
    <div className="min-w-0">
      <span className="mb-1 flex items-baseline justify-between text-xs font-medium uppercase tracking-wide text-slate-500">
        {label}
        {hint && <span className="normal-case tracking-normal text-slate-400">{hint}</span>}
      </span>
      {children}
    </div>
  );
}

/* ------------------------------------------------------------------ section editors */

function SectionEditor({ section, index, total, onChange, onMove, onRemove }: {
  section: Section;
  index: number;
  total: number;
  onChange: (s: Section) => void;
  onMove: (dir: -1 | 1) => void;
  onRemove: () => void;
}) {
  return (
    <div className="rounded-xl border border-slate-200">
      <div className="flex items-center gap-2 border-b border-slate-100 bg-slate-50/70 px-3 py-2">
        <span className="rounded-md bg-brand-600 px-2 py-0.5 text-[11px] font-bold uppercase tracking-wide text-white">
          {index + 1}
        </span>
        <span className="text-sm font-semibold text-slate-700">{SECTION_LABELS[section.type]}</span>
        <span className="ml-auto flex items-center gap-1">
          <IconBtn onClick={() => onMove(-1)} disabled={index === 0} label="Move up"><ArrowUp className="size-4" /></IconBtn>
          <IconBtn onClick={() => onMove(1)} disabled={index === total - 1} label="Move down"><ArrowDown className="size-4" /></IconBtn>
          <IconBtn onClick={onRemove} label="Remove section" danger><Trash2 className="size-4" /></IconBtn>
        </span>
      </div>
      <div className="p-3">
        <SectionForm section={section} onChange={onChange} />
      </div>
    </div>
  );
}

function IconBtn({ onClick, disabled, label, danger, children }: {
  onClick: () => void; disabled?: boolean; label: string; danger?: boolean; children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      aria-label={label}
      title={label}
      className={cn(
        'grid size-7 place-items-center rounded-md transition-colors disabled:opacity-30',
        danger ? 'text-danger-600 hover:bg-danger-50' : 'text-slate-500 hover:bg-slate-200/70',
      )}
    >
      {children}
    </button>
  );
}

function SectionForm({ section, onChange }: { section: Section; onChange: (s: Section) => void }) {
  switch (section.type) {
    case 'hero':
      return (
        <ItemList
          items={section.slides}
          label="Slide"
          onChange={(slides) => onChange({ ...section, slides })}
          blank={{ heading: 'New slide', sub: '', image: '', ctaText: '', ctaLink: '' }}
          render={(slide, set) => (
            <div className="grid gap-3 sm:grid-cols-2">
              <Field label="Heading"><input value={slide.heading} onChange={(e) => set({ ...slide, heading: e.target.value })} className={INPUT} /></Field>
              <Field label="Sub text"><input value={slide.sub ?? ''} onChange={(e) => set({ ...slide, sub: e.target.value })} className={INPUT} /></Field>
              <Field label="Image URL (optional)"><input value={slide.image ?? ''} onChange={(e) => set({ ...slide, image: e.target.value })} placeholder="https://…" className={INPUT} /></Field>
              <div className="grid grid-cols-2 gap-3">
                <Field label="Button text"><input value={slide.ctaText ?? ''} onChange={(e) => set({ ...slide, ctaText: e.target.value })} className={INPUT} /></Field>
                <Field label="Button link"><input value={slide.ctaLink ?? ''} onChange={(e) => set({ ...slide, ctaLink: e.target.value })} placeholder="/login or https://…" className={INPUT} /></Field>
              </div>
            </div>
          )}
        />
      );
    case 'features':
      return (
        <div className="space-y-3">
          <Field label="Section heading (optional)">
            <input value={section.heading ?? ''} onChange={(e) => onChange({ ...section, heading: e.target.value })} className={INPUT} />
          </Field>
          <ItemList
            items={section.items}
            label="Feature"
            onChange={(items) => onChange({ ...section, items })}
            blank={{ icon: 'Star', title: 'New feature', text: '' }}
            render={(f, set) => (
              <div className="grid gap-3 sm:grid-cols-[8rem,1fr,1.4fr]">
                <Field label="Icon">
                  <select value={f.icon ?? 'Star'} onChange={(e) => set({ ...f, icon: e.target.value })} className={INPUT}>
                    {FEATURE_ICON_OPTIONS.map((n) => <option key={n} value={n}>{n}</option>)}
                  </select>
                </Field>
                <Field label="Title"><input value={f.title} onChange={(e) => set({ ...f, title: e.target.value })} className={INPUT} /></Field>
                <Field label="Text"><input value={f.text ?? ''} onChange={(e) => set({ ...f, text: e.target.value })} className={INPUT} /></Field>
              </div>
            )}
          />
        </div>
      );
    case 'textImage':
      return (
        <div className="space-y-3">
          <div className="grid gap-3 sm:grid-cols-[1fr,1fr,8rem]">
            <Field label="Heading"><input value={section.heading ?? ''} onChange={(e) => onChange({ ...section, heading: e.target.value })} className={INPUT} /></Field>
            <Field label="Image URL (optional)"><input value={section.image ?? ''} onChange={(e) => onChange({ ...section, image: e.target.value })} placeholder="https://…" className={INPUT} /></Field>
            <Field label="Image side">
              <select value={section.side} onChange={(e) => onChange({ ...section, side: e.target.value as 'left' | 'right' })} className={INPUT}>
                <option value="right">Right</option>
                <option value="left">Left</option>
              </select>
            </Field>
          </div>
          <Field label="Content (HTML allowed)">
            <textarea rows={5} value={section.html ?? ''} onChange={(e) => onChange({ ...section, html: e.target.value })} className={AREA} />
          </Field>
        </div>
      );
    case 'stats':
      return (
        <ItemList
          items={section.items}
          label="Stat"
          onChange={(items) => onChange({ ...section, items })}
          blank={{ value: '100+', label: 'New stat' }}
          render={(s, set) => (
            <div className="grid grid-cols-2 gap-3">
              <Field label="Value"><input value={s.value} onChange={(e) => set({ ...s, value: e.target.value })} className={INPUT} /></Field>
              <Field label="Label"><input value={s.label} onChange={(e) => set({ ...s, label: e.target.value })} className={INPUT} /></Field>
            </div>
          )}
        />
      );
    case 'cta':
      return (
        <div className="grid gap-3 sm:grid-cols-2">
          <Field label="Heading"><input value={section.heading} onChange={(e) => onChange({ ...section, heading: e.target.value })} className={INPUT} /></Field>
          <Field label="Text"><input value={section.text ?? ''} onChange={(e) => onChange({ ...section, text: e.target.value })} className={INPUT} /></Field>
          <Field label="Button text"><input value={section.buttonText ?? ''} onChange={(e) => onChange({ ...section, buttonText: e.target.value })} className={INPUT} /></Field>
          <Field label="Button link"><input value={section.buttonLink ?? ''} onChange={(e) => onChange({ ...section, buttonLink: e.target.value })} placeholder="/login or https://…" className={INPUT} /></Field>
        </div>
      );
    case 'html':
      return (
        <Field label="Raw HTML" hint="Rendered as-is on the public page">
          <textarea rows={8} value={section.html} onChange={(e) => onChange({ ...section, html: e.target.value })} className={AREA} />
        </Field>
      );
    case 'contact':
      return (
        <div className="grid gap-3 sm:grid-cols-2">
          <Field label="Address"><input value={section.address ?? ''} onChange={(e) => onChange({ ...section, address: e.target.value })} className={INPUT} /></Field>
          <Field label="Phone"><input value={section.phone ?? ''} onChange={(e) => onChange({ ...section, phone: e.target.value })} className={INPUT} /></Field>
          <Field label="Email"><input value={section.email ?? ''} onChange={(e) => onChange({ ...section, email: e.target.value })} className={INPUT} /></Field>
          <Field label="Map embed URL (optional)"><input value={section.mapEmbed ?? ''} onChange={(e) => onChange({ ...section, mapEmbed: e.target.value })} placeholder="https://www.google.com/maps/embed?…" className={INPUT} /></Field>
        </div>
      );
    default:
      return null;
  }
}

/** Generic add/remove list editor for slides / features / stats. */
function ItemList<T>({ items, label, blank, onChange, render }: {
  items: T[];
  label: string;
  blank: T;
  onChange: (items: T[]) => void;
  render: (item: T, set: (next: T) => void, index: number) => React.ReactNode;
}) {
  return (
    <div className="space-y-2.5">
      {items.map((item, i) => (
        <div key={i} className="rounded-lg border border-slate-100 bg-slate-50/50 p-3">
          <div className="mb-2 flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-400">
              {label} {i + 1}
            </span>
            <button
              type="button"
              onClick={() => onChange(items.filter((_, j) => j !== i))}
              className="text-xs font-medium text-danger-600 hover:underline"
            >
              Remove
            </button>
          </div>
          {render(item, (next) => onChange(items.map((x, j) => (j === i ? next : x))), i)}
        </div>
      ))}
      <button
        type="button"
        onClick={() => onChange([...items, blank])}
        className="flex items-center gap-1.5 text-sm font-medium text-brand-600 hover:text-brand-700"
      >
        <Plus className="size-4" /> Add {label.toLowerCase()}
      </button>
    </div>
  );
}
