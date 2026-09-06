import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';

export interface UISettingRow {
  id: number;
  key: string;
  value: string;
  description: string;
}

/** GetAll UI settings (key/value/description). Readable by any authenticated user. */
export async function getUISettings(): Promise<UISettingRow[]> {
  const res = await apiPost<unknown>('UISetting/GetAll', { PageNumber: 1, PageSize: 200, IsActive: true });
  if (!res.ok) return [];
  const rows = Array.isArray(res.data)
    ? res.data
    : (res.data && typeof res.data === 'object'
        ? (Object.values(res.data as Record<string, unknown>).find(Array.isArray) as unknown[] | undefined) ?? []
        : []);
  return (rows as Record<string, unknown>[]).map((r) => ({
    id: Number(getField(r, 'UISettingMasterID', 'ID') ?? 0),
    key: String(getField(r, 'Key') ?? ''),
    value: String(getField(r, 'Value') ?? ''),
    description: String(getField(r, 'Description') ?? ''),
  }));
}

/** Field-level Value update (Admin only). */
export function updateUISettingValue(id: number, oldValue: string, newValue: string) {
  return apiPost('UISetting/Update', {
    UISettingMasterID: id,
    FieldName: 'Value',
    OldValue: oldValue,
    NewValue: newValue,
  });
}

export function createUISetting(key: string, value: string, description: string) {
  return apiPost('UISetting/Create', { Key: key, Value: value, Description: description });
}

/** Turn the rows into a case-insensitive key→value lookup. */
export function toSettingsMap(rows: UISettingRow[]): Record<string, string> {
  const m: Record<string, string> = {};
  for (const r of rows) m[r.key.toLowerCase()] = r.value;
  return m;
}
