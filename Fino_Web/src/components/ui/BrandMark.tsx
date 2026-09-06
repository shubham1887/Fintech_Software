import { useId } from 'react';
import { useBrand } from '@/theme/BrandProvider';
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
