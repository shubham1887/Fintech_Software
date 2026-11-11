import { apiPost, apiBaseUrl } from '@/lib/apiClient';

/** Grouped service catalog — Service/GetCatalog. Admin gets everything (hidden + inactive
 *  included); other roles get only active + displayable services with active operators. */
export interface CatalogOperator {
  OperatorID: number;
  OperatorName: string;
  OperatorImagePath: string;
  Opcode: string;
  IsActive: boolean;
}

export interface CatalogService {
  ServiceID: number;
  Name: string;
  ServiceImagePath: string;
  UISequence: number;
  Charge: number;
  MinBalance: number;
  IsActive: boolean;
  IsDisplay: boolean;
  IsBBPS: boolean | null;
  /** Report mapping — '' = auto (name/category keywords), 'NONE' = no report, else a report kind. */
  ReportKind: string;
  WalletID: number | null;
  WalletName: string;
  OperatorCount: number;
  /** The service is granted to this user but can't be used right now — an admin has set it to
   *  "Lock" on their record, or their KYC is not approved yet. It stays listed, but it can't be
   *  opened and transactions on it are refused. Services set to "Off" never reach the client. */
  IsLocked: boolean;
  /** Why it's locked — "KYC pending" / "Locked by your service provider". Empty when usable. */
  LockReason: string;
  Operators: CatalogOperator[];
}

export interface CatalogCategory {
  Category: string;
  Services: CatalogService[];
}

/**
 * The whole catalogue as one flat list in the admin's display order.
 *
 * The grids on the dashboard and the phone home screen show no category headings, so the order
 * that matters there is the single running order the admin set (ServiceMaster.UISequence),
 * across categories — not category by category, which is how the grouped response arrives.
 * Ties fall back to the name so the order is stable rather than whatever the server happened
 * to return.
 */
export function orderedServices(categories: CatalogCategory[]): CatalogService[] {
  return categories
    .flatMap((c) => c.Services)
    .sort((a, b) => (a.UISequence - b.UISequence) || a.Name.localeCompare(b.Name));
}

export async function fetchCatalog(): Promise<CatalogCategory[]> {
  const res = await apiPost<CatalogCategory[]>('Service/GetCatalog', {});
  return res.ok && Array.isArray(res.data) ? res.data : [];
}

/** Absolute URL for an uploaded image path ("/uploads/services/x.png") — the files are
 *  served by the API host, not the Vite dev server. Full URLs pass through untouched. */
export function imageUrl(path: string | null | undefined): string {
  const p = String(path ?? '').trim();
  if (!p) return '';
  if (/^(https?:)?\/\//i.test(p) || p.startsWith('data:')) return p;
  const origin = apiBaseUrl().replace(/\/api\/?$/i, '');
  return `${origin}${p.startsWith('/') ? '' : '/'}${p}`;
}

/** Read a File as base64 and push it to Upload/Image; resolves to the public path. */
export async function uploadImage(file: File, folder: 'services' | 'operators' | 'misc'): Promise<string> {
  const dataUri = await new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result ?? ''));
    reader.onerror = () => reject(new Error('Could not read the file.'));
    reader.readAsDataURL(file);
  });
  const res = await apiPost<{ Path: string }>('Upload/Image', {
    FileName: file.name,
    FileData: dataUri,
    Folder: folder,
  });
  if (!res.ok || !res.data?.Path) throw new Error(res.message || 'Upload failed.');
  return res.data.Path;
}

/** Read a File (image OR pdf) as base64 and push it to Upload/Document; resolves to the stored path. */
export async function uploadDocument(file: File, folder = 'kyc'): Promise<string> {
  const dataUri = await new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result ?? ''));
    reader.onerror = () => reject(new Error('Could not read the file.'));
    reader.readAsDataURL(file);
  });
  const res = await apiPost<{ Path: string }>('Upload/Document', {
    FileName: file.name,
    FileData: dataUri,
    Folder: folder,
  });
  if (!res.ok || !res.data?.Path) throw new Error(res.message || 'Upload failed.');
  return res.data.Path;
}

/**
 * Save the display order of the whole service grid in one call (Service/UpdateSequence).
 *
 * `serviceIds` is the order the admin arranged, first tile first; the API numbers them 1..N.
 * Sending the whole list at once matters: ordering is a statement about the set, and pushing it
 * one field-level update at a time left the catalogue half-renumbered between calls — which the
 * retailer app could read, because it polls the same catalogue.
 */
export async function saveServiceOrder(serviceIds: number[]) {
  return apiPost('Service/UpdateSequence', {
    Items: serviceIds.map((ServiceID, i) => ({ ServiceID, UISequence: i + 1 })),
  });
}

/** Field-level ServiceMaster update (Service/Update contract). */
export async function updateServiceField(serviceId: number, field: string, oldValue: unknown, newValue: unknown) {
  return apiPost('Service/Update', {
    ServiceID: serviceId,
    FieldName: field,
    OldValue: String(oldValue ?? '-'),
    NewValue: String(newValue),
  });
}
