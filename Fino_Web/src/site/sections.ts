/**
 * Public-website page sections. A page is an ordered list of these blocks,
 * stored server-side as JSON (WebsitePageMaster.SectionsJson) and rendered
 * top-to-bottom by the public site. The admin Website Builder edits them.
 */

export interface HeroSlide {
  heading: string;
  sub?: string;
  image?: string;
  ctaText?: string;
  ctaLink?: string;
}

export interface FeatureItem {
  icon?: string;
  title: string;
  text?: string;
}

export interface StatItem {
  value: string;
  label: string;
}

export type Section =
  | { type: 'hero'; slides: HeroSlide[] }
  | { type: 'features'; heading?: string; items: FeatureItem[] }
  | { type: 'textImage'; heading?: string; html?: string; image?: string; side: 'left' | 'right' }
  | { type: 'stats'; items: StatItem[] }
  | { type: 'cta'; heading: string; text?: string; buttonText?: string; buttonLink?: string }
  | { type: 'html'; html: string }
  | { type: 'contact'; address?: string; phone?: string; email?: string; mapEmbed?: string };

export type SectionType = Section['type'];

/** Icon names offered by the builder for feature items (rendered by the public site). */
export const FEATURE_ICON_OPTIONS = [
  'Zap', 'Shield', 'Wallet', 'Users', 'Phone', 'Globe', 'Star', 'Smartphone', 'Banknote', 'Clock', 'HeartHandshake', 'TrendingUp',
] as const;

export const SECTION_LABELS: Record<SectionType, string> = {
  hero: 'Hero slider',
  features: 'Features grid',
  textImage: 'Text + image',
  stats: 'Stats band',
  cta: 'Call to action',
  html: 'Custom HTML',
  contact: 'Contact info',
};

/** A sensible starter block for each type, so a new section renders immediately. */
export function newSection(type: SectionType): Section {
  switch (type) {
    case 'hero':
      return { type, slides: [{ heading: 'Your headline here', sub: 'A short supporting line.', ctaText: 'Get started', ctaLink: '/login' }] };
    case 'features':
      return { type, heading: 'Why choose us', items: [{ icon: 'Zap', title: 'Fast', text: 'Instant transactions.' }, { icon: 'Shield', title: 'Secure', text: 'Bank-grade encryption.' }, { icon: 'Wallet', title: 'Rewarding', text: 'Best-in-class commissions.' }] };
    case 'textImage':
      return { type, heading: 'About us', html: '<p>Write about your business here.</p>', image: '', side: 'right' };
    case 'stats':
      return { type, items: [{ value: '10K+', label: 'Retailers' }, { value: '₹50Cr+', label: 'Monthly volume' }, { value: '99.9%', label: 'Uptime' }] };
    case 'cta':
      return { type, heading: 'Ready to grow your business?', text: 'Join our network today.', buttonText: 'Sign in', buttonLink: '/login' };
    case 'html':
      return { type, html: '<div style="text-align:center"><h2>Custom HTML block</h2><p>Any markup you need.</p></div>' };
    case 'contact':
      return { type, address: '', phone: '', email: '', mapEmbed: '' };
  }
}

/** Parse a SectionsJson document defensively — bad/legacy data becomes an empty page, never a crash. */
export function parseSections(json: string): Section[] {
  if (!json?.trim()) return [];
  try {
    const arr = JSON.parse(json);
    if (!Array.isArray(arr)) return [];
    return arr.filter((s): s is Section => !!s && typeof s === 'object' && typeof s.type === 'string' && s.type in SECTION_LABELS);
  } catch {
    return [];
  }
}
