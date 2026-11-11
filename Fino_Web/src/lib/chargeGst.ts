/**
 * The GST on one configured charge — the client-side mirror of the server's ChargeGstResolver.
 *
 * Every band of a provider's rate card and of a retailer slab carries its own decision:
 *   INCLUSIVE   the charge already contains the tax   GST = c x r / (100 + r),  base = c - GST
 *   EXCLUSIVE   the tax goes on top                   GST = c x r / 100,        total = c + GST
 *   NONE        no tax on this band at all
 *   DEFAULT     the band does not decide — the commission tax profile does
 *
 * This exists so the two editors can show what a row WILL do while it is being typed. It is not
 * the source of truth: the server resolves and stamps the figure, and when a band says DEFAULT, or
 * pins no rate of its own, only the server knows what the profile and the tax engine will say. In
 * those cases this returns no numbers rather than guessing a rate, because a confident 18% on
 * screen that the engine does not actually hold is worse than no number at all.
 */

export const GST_TYPES = ['DEFAULT', 'INCLUSIVE', 'EXCLUSIVE', 'NONE'] as const;
export type GstType = (typeof GST_TYPES)[number];

/** The label for a GST type, in the words the screens use. */
export const GST_TYPE_LABEL: Record<GstType, string> = {
  DEFAULT: 'Follow profile',
  INCLUSIVE: 'Inclusive',
  EXCLUSIVE: 'Exclusive',
  NONE: 'No GST',
};

export function normalizeGstType(value: string | null | undefined): GstType {
  const v = (value ?? '').trim().toUpperCase();
  return (GST_TYPES as readonly string[]).includes(v) ? (v as GstType) : 'DEFAULT';
}

export interface ChargeSplit {
  /** The charge as the card quotes it. */
  charge: number;
  /** The tax inside it (inclusive) or added to it (exclusive). */
  gst: number;
  /** The charge without tax — the income, and what TDS is computed on. */
  base: number;
  /** What the leg settles at: base + GST. */
  total: number;
}

const r2 = (v: number) => Math.round((v + Number.EPSILON) * 100) / 100;

/**
 * Split a charge under a known type and rate. A rate of 0 splits nothing out whatever the type
 * says — there is no tax at 0% — which is also how the server reads it.
 */
export function splitCharge(charge: number, gstType: string, ratePercent: number): ChargeSplit {
  const type = normalizeGstType(gstType);
  const magnitude = Math.abs(charge);
  const sign = charge < 0 ? -1 : 1;

  if (!magnitude || type === 'NONE' || type === 'DEFAULT' || ratePercent <= 0) {
    return { charge: r2(charge), gst: 0, base: r2(charge), total: r2(charge) };
  }

  if (type === 'INCLUSIVE') {
    const gst = r2((magnitude * ratePercent) / (100 + ratePercent));
    return { charge: r2(charge), gst: sign * gst, base: sign * r2(magnitude - gst), total: r2(charge) };
  }

  const added = r2((magnitude * ratePercent) / 100);
  return { charge: r2(charge), gst: sign * added, base: r2(charge), total: sign * r2(magnitude + added) };
}

/**
 * What a band's GST setting will do, in one phrase, for the line under an editable row.
 * `fmt` formats a money figure the way the screen already does.
 */
export function gstPhrase(
  charge: number,
  gstType: string,
  ratePercent: number,
  fmt: (v: number) => string,
): string {
  const type = normalizeGstType(gstType);
  if (type === 'DEFAULT') return 'GST follows the commission tax profile';
  if (type === 'NONE') return 'no GST on this band';
  if (!charge) return type === 'INCLUSIVE' ? 'GST will be taken out of the charge' : 'GST will be added to the charge';
  if (ratePercent <= 0) {
    return type === 'INCLUSIVE'
      ? "GST taken out of the charge at the tax engine's rate for this service"
      : "GST added on top at the tax engine's rate for this service";
  }

  const s = splitCharge(charge, type, ratePercent);
  return type === 'INCLUSIVE'
    ? `includes ${fmt(Math.abs(s.gst))} GST at ${ratePercent}% — ${fmt(Math.abs(s.base))} net`
    : `plus ${fmt(Math.abs(s.gst))} GST at ${ratePercent}% — ${fmt(Math.abs(s.total))} in total`;
}
