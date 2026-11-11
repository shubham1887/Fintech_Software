import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { ChevronRight, Lock } from 'lucide-react';
import { imageUrl, orderedServices, type CatalogCategory, type CatalogService } from '@/api/catalog.api';
import { gridColsClass, tileShapeClass, type MobileLayout } from '@/theme/mobileLayouts';
import { lockMessage, serviceHref, serviceLook } from '@/lib/serviceTiles';
import { cn } from '@/lib/cn';
import { haptic } from '@/lib/native';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * The service catalogue on the phone home screen, in the six shapes a preset can ask for.
 *
 * These are different ways of BROWSING, not different skins of one grid: a 5-across dense pad
 * suits a counter operator who knows the icons by position, a one-per-row list suits a field
 * agent holding the phone in one hand, and category sections suit an operator with forty
 * services where an undifferentiated grid is just a wall.
 *
 * Three rules hold across all six, because they are about the data rather than the look:
 *
 *  - Locked services stay visible but do not open. The server says WHY (KYC pending vs an
 *    admin lock), and the two read differently to the user — one they can fix themselves.
 *  - The admin's uploaded image always wins over the keyword icon.
 *  - The order is the admin's single running order across categories (UISequence), except in
 *    `sections`, which is the one variant where category grouping is the point.
 */

export interface ServicesProps {
  categories: CatalogCategory[];
  loading: boolean;
  layout: MobileLayout;
}

export function ServicesBlock(props: ServicesProps) {
  switch (props.layout.serviceStyle) {
    case 'dense':
      return <ServiceDense {...props} />;
    case 'cards':
      return <ServiceCards {...props} />;
    case 'list':
      return <ServiceList {...props} />;
    case 'sections':
      return <ServiceSections {...props} />;
    case 'focus':
      return <ServiceFocus {...props} />;
    default:
      return <ServiceGrid {...props} />;
  }
}

/* ------------------------------------------------------------------ parts */

function SectionHeading({ children, action }: { children: React.ReactNode; action?: React.ReactNode }) {
  return (
    <div className="mb-2 flex items-center justify-between px-1">
      <h2 className="text-[11px] font-semibold uppercase tracking-wider text-slate-400">{children}</h2>
      {action}
    </div>
  );
}

function EmptyCatalogue() {
  return <p className="py-8 text-center text-sm text-slate-400">No services enabled yet.</p>;
}

/** Wraps a tappable service so locked ones get the tooltip instead of a route. */
function ServiceLink({
  svc, className, children,
}: {
  svc: CatalogService;
  className: string;
  children: React.ReactNode;
}) {
  if (svc.IsLocked) {
    return (
      <div className={cn(className, 'cursor-not-allowed')} title={lockMessage(svc.LockReason)}>
        {children}
      </div>
    );
  }
  return (
    <Link to={serviceHref(svc.Name)} onClick={() => haptic()} className={className}>
      {children}
    </Link>
  );
}

/** The icon/image face shared by every variant, at whatever size the variant asks for. */
function ServiceFace({
  svc, size, shape, colourful, iconSize,
}: {
  svc: CatalogService;
  /** A literal Tailwind size class — written out by the caller so JIT keeps it. */
  size: string;
  shape: string;
  colourful: boolean;
  iconSize?: string;
}) {
  const look = serviceLook(svc.Name);
  const img = imageUrl(svc.ServiceImagePath);
  const tone = colourful ? look.tone : 'bg-slate-100 text-slate-600';

  return (
    <span
      className={cn(
        'relative grid shrink-0 place-items-center overflow-hidden',
        size,
        shape,
        svc.IsLocked ? 'bg-slate-100' : img ? 'bg-white ring-1 ring-black/[0.04]' : tone,
      )}
    >
      {img ? (
        <img src={img} alt="" className={cn('size-full object-contain p-2', svc.IsLocked && 'opacity-40 grayscale')} />
      ) : (
        <look.icon className={cn(iconSize ?? 'size-6', svc.IsLocked && 'text-slate-400')} />
      )}
      {svc.IsLocked && (
        <span className="absolute inset-0 grid place-items-center bg-slate-900/5">
          <Lock className="size-4 text-slate-500" />
        </span>
      )}
    </span>
  );
}

/** "+N / Less" — the fold shared by the tile variants. */
function MoreTile({
  hidden, expanded, onToggle, size, shape,
}: {
  hidden: number;
  expanded: boolean;
  onToggle: () => void;
  size: string;
  shape: string;
}) {
  return (
    <button
      type="button"
      onClick={() => {
        haptic();
        onToggle();
      }}
      className="flex flex-col items-center gap-2 rounded-xl py-1.5 active:scale-95"
    >
      <span className={cn('grid place-items-center bg-slate-100 text-slate-500', size, shape)}>
        {expanded ? (
          <ChevronRight className="size-6 -rotate-90" />
        ) : (
          <span className="font-display text-sm font-bold">+{hidden}</span>
        )}
      </span>
      <span className="text-[11px] font-medium leading-tight text-slate-600">{expanded ? 'Less' : 'All'}</span>
    </button>
  );
}

/** The visible slice plus whether a fold is needed at all. */
function useFold(services: CatalogService[], limit: number) {
  const [expanded, setExpanded] = useState(false);
  // One hidden service is not worth a fold — showing "+1" costs the same row as the service.
  const folds = services.length > limit + 1;
  const visible = !folds || expanded ? services : services.slice(0, limit);
  return { visible, folds, expanded, toggle: () => setExpanded((v) => !v), hidden: services.length - limit };
}

/* ------------------------------------------------------------------- grid */

/** The classic recharge-app pad: icon tiles with labels, inside one white card. */
function ServiceGrid({ categories, loading, layout }: ServicesProps) {
  const services = useMemo(() => orderedServices(categories), [categories]);
  const fold = useFold(services, layout.visibleTiles);
  const cols = gridColsClass(layout.gridColumns);
  // Five to a row needs a smaller tile or the labels wrap to three lines.
  const size = layout.gridColumns === 5 ? 'size-12' : 'size-14';
  const shape = tileShapeClass(layout.tileShape);

  return (
    <section>
      <SectionHeading>Services</SectionHeading>
      <div className="rounded-card border border-slate-200/70 bg-white p-3 shadow-card">
        {loading ? (
          <div className={cn('grid gap-y-4', cols)}>
            {Array.from({ length: layout.gridColumns * 2 }).map((_, i) => (
              <div key={i} className="flex flex-col items-center gap-2">
                <Skeleton className={cn(size, shape)} />
                <Skeleton className="h-2.5 w-12" />
              </div>
            ))}
          </div>
        ) : services.length === 0 ? (
          <EmptyCatalogue />
        ) : (
          <div className={cn('grid gap-y-3', cols)}>
            {fold.visible.map((svc) => (
              <ServiceLink
                key={svc.ServiceID}
                svc={svc}
                className="flex flex-col items-center gap-2 rounded-xl py-1.5 transition-transform active:scale-95"
              >
                <ServiceFace
                  svc={svc}
                  size={size}
                  shape={cn(shape, layout.tileShape === 'card' && 'shadow-sm ring-1 ring-black/[0.04]')}
                  colourful={layout.colourfulTiles}
                />
                <span
                  className={cn(
                    'line-clamp-2 px-0.5 text-center text-[11px] font-medium leading-tight',
                    svc.IsLocked ? 'text-slate-400' : 'text-slate-600',
                  )}
                >
                  {svc.Name}
                </span>
              </ServiceLink>
            ))}
            {fold.folds && (
              <MoreTile hidden={fold.hidden} expanded={fold.expanded} onToggle={fold.toggle} size={size} shape={shape} />
            )}
          </div>
        )}
      </div>
    </section>
  );
}

/* ------------------------------------------------------------------ dense */

/**
 * No card, no padding, no heading rule — the tiles run to both edges of the screen and the
 * labels drop to one line. On a 360px handset this fits three rows where the grid fits two,
 * which is the whole reason the compact preset exists.
 */
function ServiceDense({ categories, loading, layout }: ServicesProps) {
  const services = useMemo(() => orderedServices(categories), [categories]);
  const fold = useFold(services, layout.visibleTiles);
  const cols = gridColsClass(layout.gridColumns);
  const shape = tileShapeClass(layout.tileShape);

  return (
    <section>
      <SectionHeading>Services</SectionHeading>
      {loading ? (
        <div className={cn('grid gap-y-3', cols)}>
          {Array.from({ length: layout.gridColumns * 3 }).map((_, i) => (
            <div key={i} className="flex flex-col items-center gap-1.5">
              <Skeleton className={cn('size-11', shape)} />
              <Skeleton className="h-2 w-10" />
            </div>
          ))}
        </div>
      ) : services.length === 0 ? (
        <EmptyCatalogue />
      ) : (
        <div className={cn('grid gap-y-2.5', cols)}>
          {fold.visible.map((svc) => (
            <ServiceLink
              key={svc.ServiceID}
              svc={svc}
              className="flex flex-col items-center gap-1.5 py-1 transition-transform active:scale-95"
            >
              <ServiceFace
                svc={svc}
                size="size-11"
                shape={shape}
                colourful={layout.colourfulTiles}
                iconSize="size-[18px]"
              />
              <span
                className={cn(
                  'w-full truncate px-0.5 text-center text-[10px] font-medium leading-tight',
                  svc.IsLocked ? 'text-slate-400' : 'text-slate-600',
                )}
              >
                {svc.Name}
              </span>
            </ServiceLink>
          ))}
          {fold.folds && (
            <MoreTile hidden={fold.hidden} expanded={fold.expanded} onToggle={fold.toggle} size="size-11" shape={shape} />
          )}
        </div>
      )}
    </section>
  );
}

/* ------------------------------------------------------------------ cards */

/**
 * The first few services as wide colour-filled cards, the remainder as a small grid below.
 *
 * A card can carry a second line ("12 operators", or the lock reason), which an icon tile
 * cannot — so the services an operator pushes get an explanation and everything else stays
 * one tap away.
 */
function ServiceCards({ categories, loading, layout }: ServicesProps) {
  const services = useMemo(() => orderedServices(categories), [categories]);
  const featured = services.slice(0, 4);
  const rest = services.slice(4);
  const [showRest, setShowRest] = useState(false);
  const shape = tileShapeClass(layout.tileShape);

  if (loading) {
    return (
      <section>
        <SectionHeading>Services</SectionHeading>
        <div className="grid grid-cols-2 gap-2.5">
          {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-[76px] rounded-2xl" />)}
        </div>
      </section>
    );
  }

  if (services.length === 0) {
    return (
      <section>
        <SectionHeading>Services</SectionHeading>
        <EmptyCatalogue />
      </section>
    );
  }

  return (
    <section>
      <SectionHeading
        action={
          rest.length > 0 ? (
            <button
              type="button"
              onClick={() => {
                haptic();
                setShowRest((v) => !v);
              }}
              className="text-xs font-semibold text-brand-600"
            >
              {showRest ? 'Show less' : `All ${services.length}`}
            </button>
          ) : undefined
        }
      >
        Services
      </SectionHeading>

      <div className="grid grid-cols-2 gap-2.5">
        {featured.map((svc) => {
          const look = serviceLook(svc.Name);
          return (
            <ServiceLink
              key={svc.ServiceID}
              svc={svc}
              className={cn(
                'flex min-w-0 items-center gap-2.5 overflow-hidden rounded-2xl p-3 shadow-card transition-transform active:scale-[0.98]',
                svc.IsLocked ? 'bg-slate-50 ring-1 ring-slate-200' : layout.colourfulTiles ? look.tone : 'bg-white ring-1 ring-slate-200/70',
              )}
            >
              <ServiceFace
                svc={svc}
                size="size-10"
                shape="rounded-xl"
                colourful={false}
                iconSize="size-5"
              />
              <span className="min-w-0 flex-1">
                <span className={cn('block truncate text-[13px] font-semibold', svc.IsLocked ? 'text-slate-400' : 'text-slate-900')}>
                  {svc.Name}
                </span>
                <span className="block truncate text-[11px] text-slate-500">
                  {svc.IsLocked
                    ? svc.LockReason || 'Locked'
                    : svc.OperatorCount > 0
                      ? `${svc.OperatorCount} operators`
                      : 'Tap to open'}
                </span>
              </span>
            </ServiceLink>
          );
        })}
      </div>

      {showRest && rest.length > 0 && (
        <div className={cn('mt-3 grid gap-y-3 rounded-card border border-slate-200/70 bg-white p-3 shadow-card', gridColsClass(layout.gridColumns))}>
          {rest.map((svc) => (
            <ServiceLink
              key={svc.ServiceID}
              svc={svc}
              className="flex flex-col items-center gap-2 py-1.5 transition-transform active:scale-95"
            >
              <ServiceFace svc={svc} size="size-12" shape={shape} colourful={layout.colourfulTiles} iconSize="size-5" />
              <span
                className={cn(
                  'line-clamp-2 text-center text-[11px] font-medium leading-tight',
                  svc.IsLocked ? 'text-slate-400' : 'text-slate-600',
                )}
              >
                {svc.Name}
              </span>
            </ServiceLink>
          ))}
        </div>
      )}
    </section>
  );
}

/* ------------------------------------------------------------------- list */

/** One row per service — name, a quiet second line, chevron. Reads as a tool, not a shop. */
function ServiceList({ categories, loading, layout }: ServicesProps) {
  const services = useMemo(() => orderedServices(categories), [categories]);
  const fold = useFold(services, layout.visibleTiles);

  return (
    <section>
      <SectionHeading>Services</SectionHeading>
      <div className="overflow-hidden rounded-card border border-slate-200/70 bg-white shadow-card">
        {loading ? (
          <div className="space-y-3 p-4">
            {Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-8 rounded-lg" />)}
          </div>
        ) : services.length === 0 ? (
          <EmptyCatalogue />
        ) : (
          <>
            {fold.visible.map((svc, i) => (
              <ServiceLink
                key={svc.ServiceID}
                svc={svc}
                className={cn('flex items-center gap-3 px-4 py-3 active:bg-slate-50', i > 0 && 'border-t border-slate-100')}
              >
                <ServiceFace
                  svc={svc}
                  size="size-9"
                  shape={tileShapeClass(layout.tileShape)}
                  colourful={layout.colourfulTiles}
                  iconSize="size-[18px]"
                />
                <span className="min-w-0 flex-1">
                  <span className={cn('block truncate text-sm font-medium', svc.IsLocked ? 'text-slate-400' : 'text-slate-800')}>
                    {svc.Name}
                  </span>
                  {svc.IsLocked && (
                    <span className="block truncate text-[11px] text-slate-400">{svc.LockReason || 'Locked'}</span>
                  )}
                </span>
                {svc.IsLocked ? (
                  <Lock className="size-4 shrink-0 text-slate-300" />
                ) : (
                  <ChevronRight className="size-4 shrink-0 text-slate-300" />
                )}
              </ServiceLink>
            ))}

            {fold.folds && (
              <button
                type="button"
                onClick={() => {
                  haptic();
                  fold.toggle();
                }}
                className="w-full border-t border-slate-100 py-3 text-xs font-semibold text-brand-600 active:bg-slate-50"
              >
                {fold.expanded ? 'Show less' : `Show all ${services.length} services`}
              </button>
            )}
          </>
        )}
      </div>
    </section>
  );
}

/* --------------------------------------------------------------- sections */

/**
 * The admin's own categories as headings, each with its own small grid.
 *
 * This is the only variant that does NOT use the flat running order: here the grouping IS the
 * navigation. Empty categories are dropped so a half-configured catalogue does not render a
 * column of headings with nothing under them.
 */
function ServiceSections({ categories, loading, layout }: ServicesProps) {
  const groups = useMemo(
    () =>
      categories
        .map((c) => ({
          name: c.Category?.trim() || 'Other',
          services: [...c.Services].sort((a, b) => (a.UISequence - b.UISequence) || a.Name.localeCompare(b.Name)),
        }))
        .filter((g) => g.services.length > 0),
    [categories],
  );
  const cols = gridColsClass(layout.gridColumns);
  const shape = tileShapeClass(layout.tileShape);

  if (loading) {
    return (
      <section className="space-y-3">
        <SectionHeading>Services</SectionHeading>
        {Array.from({ length: 2 }).map((_, i) => (
          <Skeleton key={i} className="h-28 rounded-card" />
        ))}
      </section>
    );
  }

  if (groups.length === 0) {
    return (
      <section>
        <SectionHeading>Services</SectionHeading>
        <EmptyCatalogue />
      </section>
    );
  }

  return (
    <div className="space-y-3">
      {groups.map((group) => (
        <section key={group.name}>
          <SectionHeading action={<span className="text-[11px] tabular-nums text-slate-300">{group.services.length}</span>}>
            {group.name}
          </SectionHeading>
          <div className={cn('grid gap-y-3 rounded-card border border-slate-200/70 bg-white p-3 shadow-card', cols)}>
            {group.services.map((svc) => (
              <ServiceLink
                key={svc.ServiceID}
                svc={svc}
                className="flex flex-col items-center gap-2 py-1 transition-transform active:scale-95"
              >
                <ServiceFace svc={svc} size="size-12" shape={shape} colourful={layout.colourfulTiles} iconSize="size-5" />
                <span
                  className={cn(
                    'line-clamp-2 text-center text-[11px] font-medium leading-tight',
                    svc.IsLocked ? 'text-slate-400' : 'text-slate-600',
                  )}
                >
                  {svc.Name}
                </span>
              </ServiceLink>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

/* ------------------------------------------------------------------ focus */

/**
 * One service per row, 64px icon, name in 15px.
 *
 * Built for the two cases where a 4-across icon pad fails: an agent working outdoors one-handed,
 * and a first-time retailer who does not yet recognise the icons. Everything past the fold is
 * behind one obvious button rather than a "+9" tile.
 */
function ServiceFocus({ categories, loading, layout }: ServicesProps) {
  const services = useMemo(() => orderedServices(categories), [categories]);
  const fold = useFold(services, layout.visibleTiles);

  return (
    <section>
      <SectionHeading>Services</SectionHeading>
      {loading ? (
        <div className="space-y-2.5">
          {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-[72px] rounded-card" />)}
        </div>
      ) : services.length === 0 ? (
        <EmptyCatalogue />
      ) : (
        <div className="space-y-2.5">
          {fold.visible.map((svc) => (
            <ServiceLink
              key={svc.ServiceID}
              svc={svc}
              className={cn(
                'flex items-center gap-3.5 rounded-card border bg-white p-3 shadow-card transition-transform active:scale-[0.99]',
                svc.IsLocked ? 'border-slate-200' : 'border-slate-200/70',
              )}
            >
              <ServiceFace
                svc={svc}
                size="size-14"
                shape={tileShapeClass(layout.tileShape)}
                colourful={layout.colourfulTiles}
                iconSize="size-7"
              />
              <span className="min-w-0 flex-1">
                <span className={cn('block truncate text-[15px] font-semibold', svc.IsLocked ? 'text-slate-400' : 'text-slate-900')}>
                  {svc.Name}
                </span>
                <span className="block truncate text-xs text-slate-400">
                  {svc.IsLocked ? svc.LockReason || 'Locked' : svc.OperatorCount > 0 ? `${svc.OperatorCount} operators` : 'Tap to start'}
                </span>
              </span>
              {svc.IsLocked ? (
                <Lock className="size-5 shrink-0 text-slate-300" />
              ) : (
                <ChevronRight className="size-5 shrink-0 text-slate-300" />
              )}
            </ServiceLink>
          ))}

          {fold.folds && (
            <button
              type="button"
              onClick={() => {
                haptic();
                fold.toggle();
              }}
              className="w-full rounded-card border border-slate-200/70 bg-white py-3.5 text-sm font-semibold text-brand-600 shadow-card active:bg-slate-50"
            >
              {fold.expanded ? 'Show fewer services' : `Show all ${services.length} services`}
            </button>
          )}
        </div>
      )}
    </section>
  );
}
