import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';

/**
 * Published news / offers, as the network reads them.
 *
 * News had exactly one consumer before this file existed: the admin screen that WRITES it.
 * Nothing displayed it, so publishing an offer reached nobody — the row was saved and that was
 * the end of it. Publishing now also raises an alert (see `Fino_API/Controllers/NewsController.cs`),
 * and this is the readable surface that goes with it.
 *
 * `News/GetAll` already drops unpublished drafts for non-admin callers server-side; the
 * `IsView` filter here is for the admin, who is deliberately shown their own drafts on the
 * management screen but must not see them in the banner.
 */

export interface NewsItem {
  id: number;
  title: string;
  description: string;
  imagePath: string;
  hyperLink: string;
  at: string;
  /** Past its hide-after date. The admin's own list keeps these; nothing else shows them. */
  isExpired: boolean;
}

export async function getPublishedNews(): Promise<NewsItem[]> {
  const res = await apiPost<unknown>('News/GetAll', {});
  // "No record found" is how the API answers an empty list — not an error worth surfacing.
  if (!res.ok || !Array.isArray(res.data)) return [];

  return (res.data as Record<string, unknown>[])
    .filter((r) => Boolean(getField(r, 'IsView')))
    .map((r) => ({
      id: Number(getField(r, 'NewsID') ?? 0),
      title: String(getField(r, 'Title') ?? ''),
      description: String(getField(r, 'Description') ?? ''),
      imagePath: String(getField(r, 'ImagePath') ?? ''),
      hyperLink: String(getField(r, 'HyperLink') ?? ''),
      at: String(getField(r, 'DateTime') ?? ''),
      isExpired: Boolean(getField(r, 'IsExpired')),
    }))
    // The server already drops expired items for everyone who cannot manage news; an admin
    // is deliberately sent the whole list for the management screen, so the banner has to
    // drop them again here or the operator alone keeps seeing last month's offer.
    .filter((n) => !n.isExpired)
    .filter((n) => n.title !== '' || n.description !== '')
    .sort((a, b) => new Date(b.at).getTime() - new Date(a.at).getTime());
}
