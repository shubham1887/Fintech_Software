import { useEffect, useId, useState } from 'react';
import { useBrand } from '@/theme/BrandProvider';
import { useIsNativeApp } from '@/lib/native';
import { cn } from '@/lib/cn';

/**
 * The product's built-in logo mark.
 *
 * Design brief it answers:
 *  - **White-label safe.** No letterform. A monogram works for "Fino" and is wrong the moment a
 *    reseller is called "Sharma Pay" — and every reseller uses this same build.
 *  - **One colour.** Drawn in `currentColor` and knocked out, so it inherits whatever brand ramp
 *    the operator picked and needs no per-theme artwork.
 *  - **Legible at 16px.** Three strokes, generous weight, nothing that fills in when small.
 *
 * The shape: a rounded token (secure element / card) with an upward arrow standing on a base
 * line — value moving up off a settled ledger. The Android launcher icon and splash use the
 * exact same geometry so the app and the console read as one product.
 */
export function BrandMark({ className }: { className?: string }) {
  // Two marks on one page would otherwise share a mask id and one would render solid.
  const maskId = useId();

  return (
    <svg
      viewBox="0 0 32 32"
      role="img"
      aria-hidden="true"
      className={cn('text-brand-600', className)}
    >
      <defs>
        <mask id={maskId}>
          <rect width="32" height="32" fill="white" />
          <g
            fill="none"
            stroke="black"
            strokeWidth="3.2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="M16 8.5 V17.8" />
            <path d="M10.8 13.7 L16 8.5 L21.2 13.7" />
            <path d="M9.5 24 H22.5" />
          </g>
        </mask>
      </defs>
      <rect width="32" height="32" rx="9" fill="currentColor" mask={`url(#${maskId})`} />
    </svg>
  );
}

/**
 * What every screen should render for "the logo": the operator's uploaded image when they have
 * one (config.json `logoUrl`), otherwise the built-in mark. Keeping the choice in one place is
 * why the fallback no longer drifts between an initial-letter tile here and an image there.
 */
export function BrandGlyph({ className }: { className?: string }) {
  const { appName, logoUrl } = useBrand();

  if (logoUrl) {
    return <img src={logoUrl} alt={appName} className={cn('object-contain', className)} />;
  }
  return <BrandMark className={className} />;
}

/**
 * The logo in a small app-bar slot, sized by HEIGHT so the caller's class only has to say how
 * tall it is (`h-9`, not `size-9`).
 *
 * A square slot is wrong for the artwork most clients send: a 3:1 wordmark letterboxed into 36px
 * comes out 36x12 and unreadable, which is what the phone header used to show. A wordmark is
 * given the width it needs (up to a cap, so it cannot push the rest of the bar off the screen);
 * a square logo still gets a square slot.
 *
 * `tone="dark"` sets it on white for the same reason the lockup does: a client logo is drawn for
 * white paper and disappears against the dark app bar.
 */
export function BrandBadge({
  className,
  tone = 'dark',
}: {
  className?: string;
  tone?: 'dark' | 'light';
}) {
  const { appName } = useBrand();
  const { url, shape } = useBrandArtwork();

  if (!url || shape === 'broken') {
    return <BrandMark className={cn('aspect-square', className)} />;
  }

  const wordmark = shape === 'wordmark';

  return (
    <span
      className={cn(
        'grid shrink-0 place-items-center overflow-hidden',
        wordmark ? 'w-auto max-w-[132px] px-1.5' : 'aspect-square rounded-xl',
        // A square app icon is artwork in its own right - no white card, no padding, it fills
        // the slot the way it fills the launcher.
        tone === 'dark' && wordmark && 'bg-white shadow-sm ring-1 ring-black/5',
        className,
      )}
    >
      <img src={url} alt={appName} className="max-h-full max-w-full object-contain" />
    </span>
  );
}

/**
 * A full-height "we are loading" screen with the brand on it.
 *
 * A bare spinner is the same spinner every app has. Inside the phone app in particular, the
 * moment after the native splash hands over should still look like the client's app rather than
 * a blank page with a wheel on it.
 */
export function BrandLoading({ className }: { className?: string }) {
  const { appName } = useBrand();
  const { url, shape } = useBrandArtwork();
  const icon = shape === 'icon';

  return (
    <div className={cn('flex min-h-[60vh] flex-col items-center justify-center gap-6', className)}>
      {url && shape !== 'broken' ? (
        <img
          src={url}
          alt={appName}
          className={cn('w-auto object-contain', icon ? 'h-20 rounded-2xl' : 'max-h-14 max-w-[240px]')}
        />
      ) : (
        <BrandMark className="size-16 rounded-2xl" />
      )}
      <span
        className="size-6 animate-spin rounded-full border-2 border-brand-500/25 border-t-brand-500"
        role="status"
        aria-label="Loading"
      />
    </div>
  );
}

/* ------------------------------------------------------------------ lockup */

/**
 * Shape of the client's logo file.
 *  - `wordmark` — wider than it is tall (the usual case: the artwork already spells the company
 *    name, so printing the name next to it says it twice and leaves the logo squeezed into a
 *    square slot a third of its natural width).
 *  - `icon` — roughly square: a symbol with no name in it, so the name is still needed.
 */
type LogoShape = 'wordmark' | 'icon' | 'broken';

/** url -> shape, so the header doesn't re-measure (or flicker) on every mount. */
const shapeCache = new Map<string, LogoShape>();

/**
 * Measure the logo once. `undefined` until the image has loaded (or when there is no logo);
 * `broken` when it cannot be fetched at all, which falls the lockup back to the built-in mark and
 * the company name rather than leaving a blank header.
 */
function useLogoShape(url: string): LogoShape | undefined {
  const [shape, setShape] = useState<LogoShape | undefined>(() => shapeCache.get(url));

  useEffect(() => {
    if (!url) return;
    const cached = shapeCache.get(url);
    if (cached) {
      setShape(cached);
      return;
    }
    let alive = true;
    const img = new Image();
    img.onload = () => {
      const measured: LogoShape =
        img.naturalWidth >= img.naturalHeight * 1.6 ? 'wordmark' : 'icon';
      shapeCache.set(url, measured);
      if (alive) setShape(measured);
    };
    img.onerror = () => {
      // The logo is served by the API; if that host is unreachable the panel must still show a
      // brand rather than an empty box.
      shapeCache.set(url, 'broken');
      if (alive) setShape('broken');
    };
    img.src = url;
    return () => {
      alive = false;
    };
  }, [url]);

  return shape;
}

/**
 * Which piece of the client's artwork a small or transitional surface should show.
 *
 * Inside the phone app the square app icon wins when the client has one: in the app bar a 3:1
 * wordmark is either unreadably small or the whole header, and on a loading screen the icon is
 * the thing the person just tapped - showing it again is what makes the app feel like the app.
 *
 * The LOGIN screen and the sidebar deliberately do NOT use this. They are the places the company
 * name has to be legible, and that is what the wide logo is for - on the phone as much as on the
 * desktop.
 */
function useBrandArtwork(): { url: string; shape: LogoShape | undefined } {
  const { logoUrl, appIconUrl } = useBrand();
  const inApp = useIsNativeApp();

  const url = inApp && appIconUrl ? appIconUrl : logoUrl;
  const shape = useLogoShape(url);
  return { url, shape };
}

/**
 * How large the logo is drawn, per surface.
 *
 * The headers are 64px and stay 64px - a taller header steals space from the work below it, and
 * the fix for "the logo looks small" is not a taller bar. Two things do the work instead:
 *
 *  1. The artwork is trimmed of its own whitespace when the installer prepares it, so the height
 *     it is given is filled by the mark rather than by the designer's margin (the file this was
 *     built for carried 15% empty space at the top).
 *  2. `logo` caps the height only, so the mark takes all the WIDTH its aspect ratio asks for, up
 *     to the plate's own cap. That is what makes it bigger in a bar that has not grown.
 */
const LOCKUP_SIZES = {
  // Horizontal-menu app bar, and anywhere tight.
  sm: { plate: 'h-11 max-w-[210px] px-3 py-1.5', logo: 'max-h-8', glyph: 'size-8', name: 'text-sm', tag: 'text-[10px]' },
  // Sidebar header inside a 76px bar - the one people look at all day. Thin padding, because
  // every pixel of it is height the logo does not get, and on a 3.5:1 wordmark every pixel of
  // height is three and a half pixels of width.
  md: { plate: 'h-[4.25rem] max-w-[248px] px-3 py-1', logo: 'max-h-[3.75rem]', glyph: 'size-11', name: 'text-base', tag: 'text-[11px]' },
  // Login, where there is room for the brand to be the point and the width is what is wanted.
  lg: { plate: 'h-28 max-w-[400px] px-5 py-2', logo: 'max-h-24', glyph: 'size-16', name: 'text-2xl', tag: 'text-sm' },
} as const;

/**
 * The brand header every surface should render — logo, and the company name only when the logo
 * isn't already carrying it.
 *
 * Three rules, all of them lessons from real client artwork:
 *
 *  1. **A wordmark stands alone.** Client logos are nearly always wide artwork with the company
 *     name drawn into them. Rendering "ACME · CONSOLE" beside it prints the name twice, and the
 *     old square slot shrank a 3:1 logo to a third of the header's height. A wordmark now gets
 *     the full height of the lockup and as much width as it needs.
 *  2. **An icon keeps the name.** A square logo has no name in it, so the text stays — that is
 *     also the no-logo case, where the built-in mark plus the name is all there is.
 *  3. **Dark surfaces get a white plate.** Client logos are drawn for white paper; this one is
 *     navy-on-transparent and was invisible against the dark sidebar. Setting it on a small white
 *     card is what a print designer would do and works for every logo we are ever handed.
 */
export function BrandLockup({
  tone = 'light',
  size = 'md',
  fill = false,
  className,
}: {
  /** Surface the lockup sits on: `dark` puts the logo on a white plate. */
  tone?: 'dark' | 'light';
  size?: keyof typeof LOCKUP_SIZES;
  /**
   * Stretch the plate across its container instead of hugging the logo.
   *
   * The sidebar header is 288px wide and a 3.5:1 logo that fits a 64px bar is about 190px, so a
   * hugging plate left a block of empty navy to its right - a gap that reads as a mistake rather
   * than as spacing. Filling the width turns the header into a deliberate band with the logo
   * centred in it.
   */
  fill?: boolean;
  className?: string;
}) {
  // Always the wide logo, app or browser: this is the login screen and the sidebar, where the
  // company name is the point.
  const { appName, tagline, logoUrl: url } = useBrand();
  const shape = useLogoShape(url);
  const s = LOCKUP_SIZES[size];

  const nameCls = tone === 'dark' ? 'text-white' : 'text-slate-900';
  const tagCls = tone === 'dark' ? 'text-white/55' : 'text-slate-400';

  if (url && shape !== 'broken') {
    const icon = shape === 'icon';

    // The white card exists to stop a dark logo disappearing into a dark bar. A square app icon
    // is already a finished tile with its own background, so it is shown as-is.
    const plate =
      tone === 'dark' && !icon
        ? 'inline-flex items-center justify-center rounded-xl bg-white shadow-sm ring-1 ring-black/5'
        : 'inline-flex items-center justify-center';

    // The app icon's own dark field is a square; rounding the slot turns it back into the tile
    // people see on their home screen.
    const logo = (
      <span
        className={cn(
          plate,
          s.plate,
          'shrink-0 overflow-hidden',
          icon && 'aspect-square rounded-2xl',
          // A filled plate must be allowed past the width cap that keeps a hugging one tidy.
          fill && !icon && 'w-full max-w-none',
        )}
      >
        <img
          src={url}
          alt={appName}
          className={cn('w-auto max-w-full object-contain', icon ? 'h-full' : s.logo)}
        />
      </span>
    );

    // Wordmark (or still measuring — the common case, and assuming it avoids a flash of
    // duplicated text): the artwork is the whole lockup.
    if (shape !== 'icon') {
      return <div className={cn('flex items-center', fill && 'w-full', className)}>{logo}</div>;
    }

    return (
      <div className={cn('flex items-center gap-2.5', className)}>
        {logo}
        <span className="min-w-0 leading-tight">
          <span className={cn('block truncate font-display font-bold', s.name, nameCls)}>{appName}</span>
          {tagline && (
            <span className={cn('block truncate uppercase tracking-widest', s.tag, tagCls)}>{tagline}</span>
          )}
        </span>
      </div>
    );
  }

  // No client logo: the built-in mark carries the brand colour, the text carries the name.
  return (
    <div className={cn('flex items-center gap-2.5', className)}>
      <BrandMark className={cn('shrink-0 rounded-xl', s.glyph)} />
      <span className="min-w-0 leading-tight">
        <span className={cn('block truncate font-display font-bold', s.name, nameCls)}>{appName}</span>
        {tagline && (
          <span className={cn('block truncate uppercase tracking-widest', s.tag, tagCls)}>{tagline}</span>
        )}
      </span>
    </div>
  );
}
