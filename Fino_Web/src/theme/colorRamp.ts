/** Small hex-colour helpers to derive a full brand ramp from one seed colour. */

function clamp(n: number): number {
  return Math.max(0, Math.min(255, Math.round(n)));
}

function parseHex(hex: string): [number, number, number] | null {
  let h = hex.trim().replace(/^#/, '');
  if (h.length === 3) h = h.split('').map((c) => c + c).join('');
  if (h.length !== 6 || /[^0-9a-fA-F]/.test(h)) return null;
  return [parseInt(h.slice(0, 2), 16), parseInt(h.slice(2, 4), 16), parseInt(h.slice(4, 6), 16)];
}

function toHex([r, g, b]: [number, number, number]): string {
  return '#' + [r, g, b].map((v) => clamp(v).toString(16).padStart(2, '0')).join('');
}

/** Mix a colour toward white (amount>0) or black (amount<0), amount in [-1,1]. */
function mix(rgb: [number, number, number], amount: number): [number, number, number] {
  const target = amount >= 0 ? 255 : 0;
  const t = Math.abs(amount);
  return [
    rgb[0] + (target - rgb[0]) * t,
    rgb[1] + (target - rgb[1]) * t,
    rgb[2] + (target - rgb[2]) * t,
  ];
}

/**
 * Build a 50→950 Tailwind-style ramp from a single seed colour (treated as the 500 shade),
 * so a client's `PrimaryColor` themes every brand utility coherently. Returns {} on bad input.
 */
export function rampFromSeed(seed: string): Record<string, string> {
  const rgb = parseHex(seed);
  if (!rgb) return {};
  return {
    '50': toHex(mix(rgb, 0.92)),
    '100': toHex(mix(rgb, 0.84)),
    '200': toHex(mix(rgb, 0.68)),
    '300': toHex(mix(rgb, 0.48)),
    '400': toHex(mix(rgb, 0.24)),
    '500': toHex(rgb),
    '600': toHex(mix(rgb, -0.12)),
    '700': toHex(mix(rgb, -0.28)),
    '800': toHex(mix(rgb, -0.44)),
    '900': toHex(mix(rgb, -0.6)),
    '950': toHex(mix(rgb, -0.74)),
  };
}

/** True if the string is a usable hex colour. */
export function isHexColor(v: string): boolean {
  return parseHex(v) !== null;
}
