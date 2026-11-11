import { useEffect, useMemo, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  ArrowRight,
  Banknote,
  ChevronLeft,
  ChevronRight,
  Clock,
  Globe,
  HeartHandshake,
  LogIn,
  Mail,
  MapPin,
  Menu,
  Phone,
  Shield,
  Smartphone,
  Star,
  TrendingUp,
  Users,
  Wallet,
  X,
  Zap,
  type LucideIcon,
} from 'lucide-react';
import { cachedPublicPages, fetchPublicPages, type WebsitePageRow } from '@/api/website.api';
import { parseSections, type HeroSlide, type Section } from '@/site/sections';
import { useBrand } from '@/theme/BrandProvider';
import { loadSession } from '@/lib/storage';
import { Spinner } from '@/components/ui/Spinner';
import { cn } from '@/lib/cn';
import { BrandLockup } from '@/components/ui/BrandMark';

/** Icon names the builder offers for feature items. */
const FEATURE_ICONS: Record<string, LucideIcon> = {
  Zap, Shield, Wallet, Users, Phone, Globe, Star, Smartphone, Banknote, Clock, HeartHandshake, TrendingUp,
};

export function featureIcon(name: string | undefined): LucideIcon {
  return (name && FEATURE_ICONS[name]) || Star;
}

export const FEATURE_ICON_NAMES = Object.keys(FEATURE_ICONS);

/** True for links that leave the SPA (http/https/mailto/tel). */
function isExternal(href: string): boolean {
  return /^(https?:|mailto:|tel:)/i.test(href);
}

function SmartLink({ href, className, children }: { href: string; className?: string; children: React.ReactNode }) {
  if (!href) return <span className={className}>{children}</span>;
  if (isExternal(href)) {
    return (
      <a href={href} className={className} target="_blank" rel="noreferrer">
        {children}
      </a>
    );
  }
  return (
    <Link to={href} className={className}>
      {children}
    </Link>
  );
}

/** Public URL for a page — the home page owns the root, others live at /<slug>. */
function hrefFor(p: WebsitePageRow): string {
  return p.slug === 'home' ? '/' : `/${p.slug}`;
}

/** The whole public website: dynamic navbar + section-rendered pages + footer.
 *  Mounted at "/" and as the router's catch-all, so pages resolve straight off
 *  the pathname: "/" → home, "/about" → about, unknown → in-site not-found. */
export default function PublicSite() {
  const { pathname } = useLocation();
  const { appName } = useBrand();
  const [navOpen, setNavOpen] = useState(false);
  const authed = !!loadSession();

  // Paint from the last-known copy, then correct it in the background.
  //
  // `initialData` with `initialDataUpdatedAt: 0` is the whole trick: React Query treats the
  // cached pages as data it already has (so the body renders on the FIRST frame, with no
  // spinner) but as infinitely stale (so it still refetches straight away). A returning
  // visitor therefore sees /services immediately even when the API is cold, and an edit
  // published in the Website Builder still lands on the very next load.
  const seed = useMemo(() => cachedPublicPages() ?? undefined, []);

  const { data: pages = [], isLoading } = useQuery({
    queryKey: ['public-pages'],
    queryFn: fetchPublicPages,
    staleTime: 60_000,
    initialData: seed,
    initialDataUpdatedAt: seed ? 0 : undefined,
    // The visitor is reading marketing copy, not watching a balance — refetching because they
    // tabbed back to the window costs a request and can change nothing they are looking at.
    refetchOnWindowFocus: false,
  });

  const segments = pathname.split('/').filter(Boolean);
  const slug = segments[0]?.toLowerCase();

  const menuPages = pages.filter((p) => p.showInMenu);
  const page: WebsitePageRow | undefined =
    segments.length > 1
      ? undefined // deep unknown paths are a not-found, never a page
      : slug
        ? pages.find((p) => p.slug === slug)
        : pages.find((p) => p.slug === 'home') ?? menuPages[0] ?? pages[0];
  const notFound = !isLoading && !page && pages.length > 0 && segments.length > 0;

  const sections = useMemo(() => (page ? parseSections(page.sectionsJson) : []), [page]);

  useEffect(() => {
    if (page) document.title = `${page.title} · ${appName}`;
  }, [page, appName]);

  useEffect(() => setNavOpen(false), [pathname]);

  return (
    <div className="min-h-screen bg-white font-sans text-slate-800">
      {/* ---------------------------------------------------------- navbar */}
      <header className="sticky top-0 z-40 border-b border-slate-100 bg-white/90 backdrop-blur">
        <div className="mx-auto flex h-[4.75rem] max-w-6xl items-center gap-3 px-4 sm:px-6">
          <Link to="/" className="flex items-center">
            <BrandLockup tone="light" size="md" />
          </Link>

          <nav className="ml-auto hidden items-center gap-1 md:flex">
            {menuPages.map((p) => (
              <Link
                key={p.id}
                to={hrefFor(p)}
                className={cn(
                  'rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  page?.id === p.id ? 'bg-brand-50 text-brand-700' : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900',
                )}
              >
                {p.title}
              </Link>
            ))}
            <Link
              to={authed ? '/dashboard' : '/login'}
              className="ml-2 flex items-center gap-1.5 rounded-lg bg-brand-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-brand-700"
            >
              <LogIn className="size-4" /> {authed ? 'Open Console' : 'Login'}
            </Link>
          </nav>

          <button
            type="button"
            onClick={() => setNavOpen((v) => !v)}
            className="ml-auto grid size-9 place-items-center rounded-lg text-slate-600 hover:bg-slate-100 md:hidden"
            aria-label="Menu"
          >
            {navOpen ? <X className="size-5" /> : <Menu className="size-5" />}
          </button>
        </div>

        {navOpen && (
          <nav className="border-t border-slate-100 bg-white px-4 py-2 md:hidden">
            {menuPages.map((p) => (
              <Link
                key={p.id}
                to={hrefFor(p)}
                className="block rounded-lg px-3 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
              >
                {p.title}
              </Link>
            ))}
            <Link
              to={authed ? '/dashboard' : '/login'}
              className="mt-1 flex items-center gap-1.5 rounded-lg bg-brand-600 px-3 py-2.5 text-sm font-semibold text-white"
            >
              <LogIn className="size-4" /> {authed ? 'Open Console' : 'Login'}
            </Link>
          </nav>
        )}
      </header>

      {/* ---------------------------------------------------------- body */}
      {isLoading ? (
        <div className="grid min-h-[60vh] place-items-center">
          <Spinner className="size-7 text-brand-500" />
        </div>
      ) : notFound ? (
        <NotFoundBlock />
      ) : !page ? (
        <ComingSoon appName={appName} />
      ) : (
        <main>
          {sections.length === 0 ? (
            <ComingSoon appName={appName} note="This page has no content yet." />
          ) : (
            sections.map((s, i) => <SectionBlock key={i} section={s} />)
          )}
        </main>
      )}

      {/* ---------------------------------------------------------- footer */}
      <footer className="border-t border-slate-100 bg-slate-50">
        <div className="mx-auto flex max-w-6xl flex-col items-center gap-3 px-4 py-8 text-center sm:flex-row sm:justify-between sm:text-left">
          <p className="text-sm text-slate-500">
            © {new Date().getFullYear()} {appName}. All rights reserved.
          </p>
          <div className="flex flex-wrap items-center justify-center gap-3">
            {menuPages.map((p) => (
              <Link key={p.id} to={hrefFor(p)} className="text-sm text-slate-500 hover:text-brand-600">
                {p.title}
              </Link>
            ))}
            <Link to="/login" className="text-sm font-medium text-brand-600 hover:text-brand-700">
              Partner login
            </Link>
          </div>
        </div>
      </footer>
    </div>
  );
}

function NotFoundBlock() {
  return (
    <div className="grid min-h-[60vh] place-items-center px-4">
      <div className="text-center">
        <p className="font-display text-6xl font-extrabold text-brand-100">404</p>
        <p className="mt-2 font-display text-xl font-bold text-slate-900">Page not found</p>
        <p className="mt-1 text-slate-500">The page you're looking for doesn't exist or was unpublished.</p>
        <Link
          to="/"
          className="mt-6 inline-flex items-center gap-2 rounded-xl bg-brand-600 px-5 py-2.5 font-semibold text-white hover:bg-brand-700"
        >
          Go to homepage
        </Link>
      </div>
    </div>
  );
}

function ComingSoon({ appName, note }: { appName: string; note?: string }) {
  return (
    <div className="grid min-h-[60vh] place-items-center px-4">
      <div className="text-center">
        <p className="font-display text-3xl font-bold text-slate-900">{appName}</p>
        <p className="mt-2 text-slate-500">{note ?? 'Our website is coming soon.'}</p>
        <Link
          to="/login"
          className="mt-6 inline-flex items-center gap-2 rounded-xl bg-brand-600 px-5 py-2.5 font-semibold text-white hover:bg-brand-700"
        >
          <LogIn className="size-4" /> Partner login
        </Link>
      </div>
    </div>
  );
}

/* ------------------------------------------------------------------ sections */

function SectionBlock({ section }: { section: Section }) {
  switch (section.type) {
    case 'hero':
      return <HeroSection slides={section.slides} />;
    case 'features':
      return (
        <section className="mx-auto max-w-6xl px-4 py-14 sm:px-6">
          {section.heading && (
            <h2 className="mb-8 text-center font-display text-2xl font-bold text-slate-900 sm:text-3xl">{section.heading}</h2>
          )}
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {section.items.map((f, i) => {
              const Icon = featureIcon(f.icon);
              return (
                <div key={i} className="rounded-2xl border border-slate-100 bg-white p-6 shadow-card">
                  <span className="grid size-11 place-items-center rounded-xl bg-brand-50 text-brand-600">
                    <Icon className="size-5" />
                  </span>
                  <h3 className="mt-4 font-display text-lg font-semibold text-slate-900">{f.title}</h3>
                  {f.text && <p className="mt-1.5 text-sm leading-relaxed text-slate-500">{f.text}</p>}
                </div>
              );
            })}
          </div>
        </section>
      );
    case 'textImage':
      return (
        <section className="mx-auto max-w-6xl px-4 py-14 sm:px-6">
          <div className={cn('flex flex-col items-center gap-8 lg:gap-14', section.side === 'left' ? 'lg:flex-row-reverse' : 'lg:flex-row')}>
            <div className="min-w-0 flex-1">
              {section.heading && (
                <h2 className="mb-4 font-display text-2xl font-bold text-slate-900 sm:text-3xl">{section.heading}</h2>
              )}
              {section.html && (
                <div
                  className="prose-site text-slate-600 [&_a]:text-brand-600 [&_h3]:font-display [&_h3]:text-lg [&_h3]:font-semibold [&_h3]:text-slate-900 [&_li]:my-1 [&_p]:my-2 [&_ul]:list-disc [&_ul]:pl-5"
                  dangerouslySetInnerHTML={{ __html: section.html }}
                />
              )}
            </div>
            <div className="w-full max-w-md flex-1">
              {section.image ? (
                <img src={section.image} alt={section.heading ?? ''} className="w-full rounded-2xl object-cover shadow-card" />
              ) : (
                <div className="grid aspect-[4/3] w-full place-items-center rounded-2xl bg-gradient-to-br from-brand-500 to-brand-700 text-white/80">
                  <Globe className="size-16" />
                </div>
              )}
            </div>
          </div>
        </section>
      );
    case 'stats':
      return (
        <section className="bg-gradient-to-r from-brand-600 to-brand-800 py-12">
          <div className="mx-auto grid max-w-6xl grid-cols-2 gap-6 px-4 sm:px-6 lg:grid-cols-4">
            {section.items.map((s, i) => (
              <div key={i} className="text-center">
                <p className="font-display text-3xl font-extrabold text-white">{s.value}</p>
                <p className="mt-1 text-sm font-medium text-white/70">{s.label}</p>
              </div>
            ))}
          </div>
        </section>
      );
    case 'cta':
      return (
        <section className="mx-auto max-w-4xl px-4 py-16 text-center sm:px-6">
          <h2 className="font-display text-2xl font-bold text-slate-900 sm:text-3xl">{section.heading}</h2>
          {section.text && <p className="mx-auto mt-3 max-w-xl text-slate-500">{section.text}</p>}
          {section.buttonText && (
            <SmartLink
              href={section.buttonLink ?? '/login'}
              className="mt-7 inline-flex items-center gap-2 rounded-xl bg-brand-600 px-6 py-3 font-semibold text-white shadow-glow transition-colors hover:bg-brand-700"
            >
              {section.buttonText} <ArrowRight className="size-4" />
            </SmartLink>
          )}
        </section>
      );
    case 'html':
      return (
        <section className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
          <div dangerouslySetInnerHTML={{ __html: section.html }} />
        </section>
      );
    case 'contact':
      return (
        <section className="mx-auto max-w-6xl px-4 py-14 sm:px-6">
          <div className="grid gap-5 sm:grid-cols-3">
            {section.address && <ContactCard icon={MapPin} label="Address" value={section.address} />}
            {section.phone && <ContactCard icon={Phone} label="Phone" value={section.phone} href={`tel:${section.phone}`} />}
            {section.email && <ContactCard icon={Mail} label="Email" value={section.email} href={`mailto:${section.email}`} />}
          </div>
          {section.mapEmbed && (
            <div className="mt-8 overflow-hidden rounded-2xl border border-slate-100 shadow-card">
              <iframe src={section.mapEmbed} title="Map" className="h-80 w-full border-0" loading="lazy" />
            </div>
          )}
        </section>
      );
    default:
      return null;
  }
}

function ContactCard({ icon: Icon, label, value, href }: { icon: LucideIcon; label: string; value: string; href?: string }) {
  const body = (
    <div className="flex h-full gap-4 rounded-2xl border border-slate-100 bg-white p-6 shadow-card">
      <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-brand-50 text-brand-600">
        <Icon className="size-5" />
      </span>
      <div className="min-w-0">
        <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">{label}</p>
        <p className="mt-1 break-words text-sm font-medium text-slate-800">{value}</p>
      </div>
    </div>
  );
  return href ? <a href={href}>{body}</a> : body;
}

/* ------------------------------------------------------------------ hero slider */

function HeroSection({ slides }: { slides: HeroSlide[] }) {
  const { appName, tagline } = useBrand();
  const [index, setIndex] = useState(0);
  const count = slides.length;
  const active = slides[index];

  useEffect(() => {
    if (count < 2) return;
    const t = setInterval(() => setIndex((i) => (i + 1) % count), 6000);
    return () => clearInterval(t);
  }, [count]);

  if (count === 0) return null;
  const go = (d: number) => setIndex((i) => (i + d + count) % count);

  return (
    <section className="relative isolate overflow-hidden bg-slate-950">
      {/* Full-bleed slide backgrounds, cross-faded (no sliding = no half-image bleed). */}
      {slides.map((s, i) => (
        <div
          key={i}
          className={cn('absolute inset-0 transition-opacity duration-1000 ease-out', i === index ? 'opacity-100' : 'opacity-0')}
          aria-hidden={i === index ? undefined : true}
        >
          {s.image ? (
            <img src={s.image} alt="" className="h-full w-full object-cover" />
          ) : (
            <div className="h-full w-full bg-gradient-to-br from-brand-700 to-brand-900" />
          )}
          {/* Dark scrim — heavy on the left where the copy sits, image kept visible on the right. */}
          <div className="absolute inset-0 bg-gradient-to-r from-slate-950/95 via-slate-950/80 to-slate-950/30" />
          <div className="absolute inset-0 bg-gradient-to-t from-slate-950/70 via-transparent to-transparent" />
          <div className="absolute -left-20 top-1/4 size-80 rounded-full bg-brand-500/20 blur-3xl" />
        </div>
      ))}

      {/* Copy for the active slide. */}
      <div className="relative mx-auto flex min-h-[30rem] max-w-6xl items-center px-4 sm:px-6 lg:min-h-[36rem]">
        <div className="max-w-2xl py-20">
          <span className="inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/10 px-3.5 py-1.5 text-xs font-semibold uppercase tracking-widest text-white/80 backdrop-blur">
            <Zap className="size-3.5 text-brand-300" /> {appName} <span className="text-white/40">·</span> {tagline}
          </span>
          <h1 className="mt-6 font-display text-4xl font-extrabold leading-[1.05] tracking-tight text-white drop-shadow sm:text-5xl lg:text-6xl">
            {active.heading}
          </h1>
          {active.sub && <p className="mt-5 max-w-xl text-lg leading-relaxed text-white/75">{active.sub}</p>}
          {active.ctaText && (
            <div className="mt-9">
              <SmartLink
                href={active.ctaLink ?? '/login'}
                className="inline-flex items-center gap-2 rounded-xl bg-white px-6 py-3.5 text-sm font-semibold text-slate-900 shadow-xl shadow-black/25 transition-transform hover:-translate-y-0.5"
              >
                {active.ctaText} <ArrowRight className="size-4" />
              </SmartLink>
            </div>
          )}
        </div>
      </div>

      {count > 1 && (
        <>
          <button
            type="button"
            onClick={() => go(-1)}
            className="absolute left-3 top-1/2 hidden size-11 -translate-y-1/2 place-items-center rounded-full border border-white/20 bg-white/5 text-white backdrop-blur transition-colors hover:bg-white/15 sm:grid"
            aria-label="Previous slide"
          >
            <ChevronLeft className="size-5" />
          </button>
          <button
            type="button"
            onClick={() => go(1)}
            className="absolute right-3 top-1/2 hidden size-11 -translate-y-1/2 place-items-center rounded-full border border-white/20 bg-white/5 text-white backdrop-blur transition-colors hover:bg-white/15 sm:grid"
            aria-label="Next slide"
          >
            <ChevronRight className="size-5" />
          </button>
          <div className="absolute bottom-7 left-4 flex gap-2 sm:left-6">
            {slides.map((_, i) => (
              <button
                key={i}
                type="button"
                onClick={() => setIndex(i)}
                className={cn('h-1.5 rounded-full transition-all', i === index ? 'w-8 bg-white' : 'w-4 bg-white/40 hover:bg-white/60')}
                aria-label={`Slide ${i + 1}`}
              />
            ))}
          </div>
        </>
      )}

      {/* Soft fade into the page below. */}
      <div className="pointer-events-none absolute inset-x-0 bottom-0 h-16 bg-gradient-to-t from-white to-transparent" />
    </section>
  );
}
