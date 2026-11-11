/**
 * Indian state codes as GST uses them — the first two digits of a GSTIN.
 *
 * This list is shared rather than copied because two screens have to agree on it: the tax
 * simulator picks a supplier state and a place of supply from it (same code → CGST + SGST,
 * different codes → IGST), and the company profile checks that the GSTIN the admin types
 * actually starts with the code of the state they picked. A GSTIN whose first two digits
 * disagree with the company's state sends every intra-state supply to the wrong tax head, and
 * nothing downstream can detect it — the number looks perfectly valid.
 *
 * 25 (the old Daman & Diu) is absent on purpose: it merged into 26 in 2020 and a new
 * registration cannot be issued under it. 97 / 99 (Other Territory, Centre Jurisdiction) are
 * absent because this is a company profile, not a return filing.
 */
export interface GstState {
  /** Two-digit GST state code, zero-padded — '07', '27'. */
  code: string;
  name: string;
}

export const GST_STATES: GstState[] = [
  { code: '01', name: 'Jammu & Kashmir' },
  { code: '02', name: 'Himachal Pradesh' },
  { code: '03', name: 'Punjab' },
  { code: '04', name: 'Chandigarh' },
  { code: '05', name: 'Uttarakhand' },
  { code: '06', name: 'Haryana' },
  { code: '07', name: 'Delhi' },
  { code: '08', name: 'Rajasthan' },
  { code: '09', name: 'Uttar Pradesh' },
  { code: '10', name: 'Bihar' },
  { code: '11', name: 'Sikkim' },
  { code: '12', name: 'Arunachal Pradesh' },
  { code: '13', name: 'Nagaland' },
  { code: '14', name: 'Manipur' },
  { code: '15', name: 'Mizoram' },
  { code: '16', name: 'Tripura' },
  { code: '17', name: 'Meghalaya' },
  { code: '18', name: 'Assam' },
  { code: '19', name: 'West Bengal' },
  { code: '20', name: 'Jharkhand' },
  { code: '21', name: 'Odisha' },
  { code: '22', name: 'Chhattisgarh' },
  { code: '23', name: 'Madhya Pradesh' },
  { code: '24', name: 'Gujarat' },
  { code: '26', name: 'Dadra & Nagar Haveli and Daman & Diu' },
  { code: '27', name: 'Maharashtra' },
  { code: '29', name: 'Karnataka' },
  { code: '30', name: 'Goa' },
  { code: '31', name: 'Lakshadweep' },
  { code: '32', name: 'Kerala' },
  { code: '33', name: 'Tamil Nadu' },
  { code: '34', name: 'Puducherry' },
  { code: '35', name: 'Andaman & Nicobar Islands' },
  { code: '36', name: 'Telangana' },
  { code: '37', name: 'Andhra Pradesh' },
  { code: '38', name: 'Ladakh' },
];

/** "27 Maharashtra" — the label the state pickers show, code first so it is searchable by code. */
export const gstStateLabel = (s: GstState) => `${s.code} ${s.name}`;

/** The state whose name matches (case / punctuation tolerant), or undefined. */
export function gstStateByName(name: string): GstState | undefined {
  const key = normalise(name);
  if (!key) return undefined;
  return GST_STATES.find((s) => normalise(s.name) === key) ?? ALIASES[key];
}

export const gstStateByCode = (code: string): GstState | undefined =>
  GST_STATES.find((s) => s.code === code.trim());

/**
 * The supplier state code of a GSTIN — its first two characters. This is the authoritative
 * source for "which state is the supplier in": GST assigns the registration per state and
 * stamps that state into the number, so no separate column can disagree with it.
 * Returns '' for anything that is not 15 characters starting with two digits.
 */
export function stateCodeFromGstin(gstin: string): string {
  const g = gstin.trim().toUpperCase();
  return g.length === 15 && /^[0-9]{2}/.test(g) ? g.slice(0, 2) : '';
}

/** A regular taxpayer's GSTIN: state code, PAN, entity number, 'Z', checksum. */
export const GSTIN_PATTERN = /^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$/;
export const PAN_PATTERN = /^[A-Z]{5}[0-9]{4}[A-Z]$/;
export const IFSC_PATTERN = /^[A-Z]{4}0[A-Z0-9]{6}$/;

/** The PAN a GSTIN carries in characters 3–12, or '' if the number is not 15 long. */
export function panFromGstin(gstin: string): string {
  const g = gstin.trim().toUpperCase();
  return g.length === 15 ? g.slice(2, 12) : '';
}

/** Names that older installs stored, mapped to the current list. */
const ALIASES: Record<string, GstState> = {
  orissa: { code: '21', name: 'Odisha' },
  pondicherry: { code: '34', name: 'Puducherry' },
  uttaranchal: { code: '05', name: 'Uttarakhand' },
  'newdelhi': { code: '07', name: 'Delhi' },
};

const normalise = (v: string) => v.trim().toLowerCase().replace(/[^a-z]/g, '');
