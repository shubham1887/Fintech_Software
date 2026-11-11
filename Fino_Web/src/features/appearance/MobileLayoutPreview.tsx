import { gridColsClass, headerStyleFor, heroSurfaceStyle, type MobileLayout } from '@/theme/mobileLayouts';
import { cn } from '@/lib/cn';

/**
 * A miniature of the phone home screen, drawn from the same layout object the app renders.
 *
 * This exists because the presets are now structurally different, and a picker that cannot
 * show the difference is the reason the old one felt like five names for one screen. Nothing
 * here fetches anything — it is a sketch in proportion, not a live render — but every block
 * reads the SAME field the real screen reads, so a preview can never claim a shape the app
 * does not draw.
 *
 * Roughly 1:4.3 scale against a 360px handset: 84×152 CSS pixels.
 */
export function MobileLayoutPreview({ layout, className }: { layout: MobileLayout; className?: string }) {
  const hero = layout.showBalanceCard && layout.balanceStyle === 'hero';

  return (
    <span
      className={cn(
        'grid h-[152px] w-[84px] shrink-0 grid-rows-[auto_1fr_auto] overflow-hidden rounded-[7px] bg-canvas ring-1 ring-slate-200',
        className,
      )}
      aria-hidden
    >
      {/* app bar */}
      <span className="flex h-[13px] items-center gap-[3px] px-[4px]" style={headerStyleFor(layout.headerStyle)}>
        <span className="size-[7px] rounded-[2px] bg-white/70" />
        <span className="h-[3px] w-[18px] rounded-full bg-white/45" />
        <span className="ml-auto size-[5px] rounded-full bg-white/40" />
      </span>

      {/* content */}
      <span className={cn('flex min-h-0 flex-col overflow-hidden', hero ? 'gap-[4px]' : 'gap-[4px] px-[4px] pt-[4px]')}>
        {layout.showBalanceCard && <Balance layout={layout} />}
        <span className={cn('flex min-h-0 flex-1 flex-col gap-[4px]', hero && 'px-[4px]')}>
          {layout.statsFirst && layout.showTodayStats && <Stats layout={layout} />}
          <Services layout={layout} />
          {!layout.statsFirst && layout.showTodayStats && <Stats layout={layout} />}
          <Activity layout={layout} />
        </span>
      </span>

      {/* tab bar */}
      <Tabs layout={layout} />
    </span>
  );
}

/* ------------------------------------------------------------------ bits */

const NAVBAR = { backgroundColor: 'var(--ui-navbar)' } as const;

/** A neutral content bar — the preview's unit of "some text lives here". */
function Bar({ w, dark, tone, h = 3 }: { w: string; dark?: boolean; tone?: string; h?: number }) {
  return (
    <span
      className={cn('block rounded-full', tone ?? (dark ? 'bg-white/40' : 'bg-slate-300'))}
      style={{ width: w, height: `${h}px` }}
    />
  );
}

function Panel({ children, className }: { children?: React.ReactNode; className?: string }) {
  return <span className={cn('block rounded-[4px] bg-white ring-1 ring-slate-200/80', className)}>{children}</span>;
}

/* --------------------------------------------------------------- balance */

function Balance({ layout }: { layout: MobileLayout }) {
  switch (layout.balanceStyle) {
    case 'hero':
      return (
        <span className="block rounded-b-[8px] px-[6px] pb-[6px] pt-[5px]" style={heroSurfaceStyle()}>
          <Bar w="22px" dark h={2} />
          <span className="mt-[4px] block h-[8px] w-[46px] rounded-[2px] bg-white/80" />
          <span className="mt-[5px] flex gap-[3px]">
            <span className="h-[7px] flex-1 rounded-full bg-white/85" />
            <span className="h-[7px] flex-1 rounded-full bg-white/25" />
          </span>
        </span>
      );

    case 'strip':
      return (
        <Panel className="flex items-center gap-[4px] px-[4px] py-[3px]">
          <span className="block h-[6px] w-[20px] rounded-[2px] bg-slate-700" />
          <span className="h-[8px] w-px bg-slate-200" />
          <span className="flex flex-1 gap-[3px]">
            <Bar w="12px" h={4} />
            <Bar w="12px" h={4} />
          </span>
          <span className="size-[7px] rounded-[2px] bg-brand-500" />
        </Panel>
      );

    case 'plain':
      return (
        <span className="block border-b border-slate-200 pb-[5px]">
          <Bar w="20px" h={2} />
          <span className="mt-[3px] block h-[9px] w-[48px] rounded-[2px] bg-slate-800" />
          <span className="mt-[4px] flex gap-[4px]">
            <Bar w="16px" h={2} tone="bg-brand-400" />
            <Bar w="14px" h={2} />
          </span>
        </span>
      );

    case 'split':
      return (
        <Panel className="flex overflow-hidden">
          <span className="flex w-[34px] shrink-0 flex-col justify-center gap-[3px] p-[4px]" style={NAVBAR}>
            <Bar w="14px" dark h={2} />
            <span className="block h-[6px] w-[24px] rounded-[2px] bg-white/80" />
          </span>
          <span className="flex flex-1 flex-col justify-center gap-[3px] p-[4px]">
            {Array.from({ length: 3 }).map((_, i) => (
              <span key={i} className="flex items-center gap-[3px]">
                <Bar w="10px" h={2} />
                <span className="h-[2px] flex-1 rounded-full bg-brand-400/60" />
              </span>
            ))}
          </span>
        </Panel>
      );

    default:
      return (
        <span className="block rounded-[5px] p-[5px]" style={NAVBAR}>
          <Bar w="20px" dark h={2} />
          <span className="mt-[3px] block h-[8px] w-[42px] rounded-[2px] bg-white/85" />
          <span className="mt-[4px] flex gap-[2px] rounded-[2px] bg-white/10 p-[2px]">
            {Array.from({ length: 3 }).map((_, i) => (
              <span key={i} className="h-[6px] flex-1 rounded-[1px] bg-white/25" />
            ))}
          </span>
          <span className="mt-[4px] flex gap-[3px]">
            <span className="h-[6px] flex-1 rounded-[2px] bg-white/85" />
            <span className="h-[6px] flex-1 rounded-[2px] border border-white/40" />
          </span>
        </span>
      );
  }
}

/* -------------------------------------------------------------- services */

function Tile({ layout, size = 9 }: { layout: MobileLayout; size?: number }) {
  return (
    <span
      className={cn(
        'block w-full',
        layout.tileShape === 'circle' ? 'rounded-full' : layout.tileShape === 'square' ? 'rounded-[1px]' : 'rounded-[2px]',
        layout.colourfulTiles ? 'bg-brand-400/75' : 'bg-slate-300',
      )}
      style={{ height: `${size}px` }}
    />
  );
}

function TileGrid({ layout, rows, className }: { layout: MobileLayout; rows: number; className?: string }) {
  return (
    <span className={cn('grid gap-[2px]', gridColsClass(layout.gridColumns), className)}>
      {Array.from({ length: layout.gridColumns * rows }).map((_, i) => (
        <Tile key={i} layout={layout} size={layout.gridColumns === 5 ? 7 : 8} />
      ))}
    </span>
  );
}

function Services({ layout }: { layout: MobileLayout }) {
  switch (layout.serviceStyle) {
    case 'dense':
      // No card, no padding — three rows where the grid fits two.
      return <TileGrid layout={layout} rows={3} />;

    case 'cards':
      return (
        <span className="grid grid-cols-2 gap-[3px]">
          {Array.from({ length: 4 }).map((_, i) => (
            <span
              key={i}
              className={cn(
                'flex items-center gap-[3px] rounded-[4px] p-[3px]',
                layout.colourfulTiles ? 'bg-brand-400/35' : 'bg-white ring-1 ring-slate-200/80',
              )}
            >
              <span className="size-[8px] shrink-0 rounded-[2px] bg-white/90" />
              <span className="flex-1">
                <Bar w="100%" h={2} />
                <span className="mt-[2px] block h-[1.5px] w-[60%] rounded-full bg-slate-300/70" />
              </span>
            </span>
          ))}
        </span>
      );

    case 'list':
      return (
        <Panel className="divide-y divide-slate-100">
          {Array.from({ length: 4 }).map((_, i) => (
            <span key={i} className="flex items-center gap-[3px] px-[4px] py-[3px]">
              <span
                className={cn(
                  'size-[7px] shrink-0',
                  layout.tileShape === 'circle' ? 'rounded-full' : 'rounded-[1px]',
                  layout.colourfulTiles ? 'bg-brand-400/75' : 'bg-slate-300',
                )}
              />
              <Bar w="60%" h={2} />
              <span className="ml-auto h-[4px] w-[2px] rounded-full bg-slate-200" />
            </span>
          ))}
        </Panel>
      );

    case 'sections':
      return (
        <span className="flex flex-col gap-[4px]">
          {Array.from({ length: 2 }).map((_, s) => (
            <span key={s} className="block">
              <Bar w="20px" h={2} />
              <Panel className="mt-[2px] p-[3px]">
                <TileGrid layout={layout} rows={1} />
              </Panel>
            </span>
          ))}
        </span>
      );

    case 'focus':
      return (
        <span className="flex flex-col gap-[3px]">
          {Array.from({ length: 3 }).map((_, i) => (
            <Panel key={i} className="flex items-center gap-[4px] p-[3px]">
              <span
                className={cn(
                  'size-[12px] shrink-0 rounded-[3px]',
                  layout.colourfulTiles ? 'bg-brand-400/75' : 'bg-slate-300',
                )}
              />
              <span className="flex-1">
                <Bar w="70%" h={2.5} />
                <span className="mt-[2px] block h-[1.5px] w-[40%] rounded-full bg-slate-200" />
              </span>
            </Panel>
          ))}
        </span>
      );

    default:
      return (
        <Panel className="p-[3px]">
          <TileGrid layout={layout} rows={2} />
        </Panel>
      );
  }
}

/* ----------------------------------------------------------------- stats */

function Stats({ layout }: { layout: MobileLayout }) {
  if (layout.statsStyle === 'cards') {
    return (
      <span className="grid grid-cols-2 gap-[3px]">
        {Array.from({ length: 4 }).map((_, i) => (
          <Panel key={i} className="p-[3px]">
            <Bar w="60%" h={1.5} />
            <span className="mt-[2px] block h-[4px] w-[70%] rounded-[1px] bg-slate-700" />
          </Panel>
        ))}
      </span>
    );
  }

  return (
    <Panel className="flex items-center gap-[2px] px-[3px] py-[3px]">
      {Array.from({ length: 4 }).map((_, i) => (
        <span key={i} className="flex-1">
          <span className="mx-auto block h-[1.5px] w-[70%] rounded-full bg-slate-200" />
          <span className="mx-auto mt-[2px] block h-[3px] w-[80%] rounded-[1px] bg-slate-400" />
        </span>
      ))}
    </Panel>
  );
}

/* -------------------------------------------------------------- activity */

function Activity({ layout }: { layout: MobileLayout }) {
  switch (layout.activityStyle) {
    case 'compact':
      return (
        <Panel className="divide-y divide-slate-100">
          {Array.from({ length: 4 }).map((_, i) => (
            <span key={i} className="flex items-center gap-[3px] py-[2px] pr-[4px]">
              <span className="h-[5px] w-[1.5px] rounded-r bg-success-500" />
              <Bar w="40%" h={2} />
              <span className="ml-auto h-[2px] w-[14px] rounded-full bg-slate-400" />
            </span>
          ))}
        </Panel>
      );

    case 'timeline':
      return (
        <span className="relative block pl-[7px]">
          <span className="absolute bottom-[3px] left-[2px] top-[3px] w-px bg-slate-200" />
          {Array.from({ length: 3 }).map((_, i) => (
            <span key={i} className={cn('relative block', i > 0 && 'mt-[5px]')}>
              <span className="absolute -left-[7px] top-[1px] size-[4px] rounded-full bg-success-500 ring-[1.5px] ring-canvas" />
              <Bar w="52%" h={2} />
              <span className="mt-[2px] block h-[1.5px] w-[32%] rounded-full bg-slate-200" />
            </span>
          ))}
        </span>
      );

    case 'table':
      return (
        <Panel className="overflow-hidden">
          <span className="flex items-center gap-[3px] border-b border-slate-100 bg-slate-50 px-[3px] py-[2px]">
            <span className="h-[1.5px] w-[10px] rounded-full bg-slate-300" />
            <span className="h-[1.5px] w-[20px] rounded-full bg-slate-300" />
            <span className="ml-auto h-[1.5px] w-[12px] rounded-full bg-slate-300" />
          </span>
          {Array.from({ length: 3 }).map((_, i) => (
            <span key={i} className={cn('flex items-center gap-[3px] px-[3px] py-[2.5px]', i > 0 && 'border-t border-slate-100')}>
              <span className="h-[2px] w-[10px] rounded-full bg-slate-200" />
              <Bar w="28px" h={2} />
              <span className="ml-auto h-[2px] w-[12px] rounded-full bg-slate-500" />
            </span>
          ))}
        </Panel>
      );

    default:
      return (
        <Panel className="divide-y divide-slate-100">
          {Array.from({ length: 3 }).map((_, i) => (
            <span key={i} className="flex items-center gap-[3px] px-[4px] py-[3px]">
              <span className="size-[3px] shrink-0 rounded-full bg-success-500" />
              <span className="flex-1">
                <Bar w="60%" h={2} />
                <span className="mt-[2px] block h-[1.5px] w-[36%] rounded-full bg-slate-200" />
              </span>
              <span className="h-[2px] w-[12px] rounded-full bg-slate-500" />
            </span>
          ))}
        </Panel>
      );
  }
}

/* ------------------------------------------------------------------ tabs */

function Tabs({ layout }: { layout: MobileLayout }) {
  const floating = layout.tabStyle === 'floating';

  return (
    <span
      className={cn(
        'flex items-center justify-around',
        floating
          ? 'mx-[5px] mb-[4px] rounded-[5px] bg-white py-[3px] shadow-sm ring-1 ring-slate-200'
          : 'border-t border-slate-200 bg-white py-[3px]',
      )}
    >
      {Array.from({ length: 4 }).map((_, i) => (
        <span key={i} className="flex flex-col items-center gap-[1.5px]">
          <span className={cn('size-[5px] rounded-[1px]', i === 0 ? 'bg-brand-500' : 'bg-slate-300')} />
          {layout.tabStyle !== 'icons' && (
            <span className={cn('block h-[1.5px] w-[7px] rounded-full', i === 0 ? 'bg-brand-400' : 'bg-slate-200')} />
          )}
        </span>
      ))}
    </span>
  );
}
