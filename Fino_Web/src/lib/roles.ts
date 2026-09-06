/** Mirrors Fino_API `StaticModels/UserTypeCode.cs` — the authoritative role map. */
export const UserTypeCode = {
  AllUsers: 0,
  Admin: 1,
  SuperDistributor: 2,
  Distributor: 3,
  Retailer: 4,
  ApiUser: 5,
  SupportUser: 6,
  Client: 7,
  Accountant: 9,
  SDFOS: 10,
  DFOS: 11,
  DeactiveUsers: 999,
} as const;

const NAMES: Record<number, string> = {
  [UserTypeCode.Admin]: 'Administrator',
  [UserTypeCode.SuperDistributor]: 'Super Distributor',
  [UserTypeCode.Distributor]: 'Distributor',
  [UserTypeCode.Retailer]: 'Retailer',
  [UserTypeCode.ApiUser]: 'API User',
  [UserTypeCode.SupportUser]: 'Support',
  [UserTypeCode.Client]: 'Client',
  [UserTypeCode.Accountant]: 'Accountant',
  [UserTypeCode.SDFOS]: 'SD Field Officer',
  [UserTypeCode.DFOS]: 'D Field Officer',
};

export function roleLabel(userTypeId: number, fallback = ''): string {
  return NAMES[userTypeId] ?? fallback;
}

export const isRetailer = (id: number) => id === UserTypeCode.Retailer;
export const isDistributorTier = (id: number) =>
  id === UserTypeCode.Distributor || id === UserTypeCode.SuperDistributor;
export const isAdminTier = (id: number) =>
  id === UserTypeCode.Admin || id === UserTypeCode.Accountant || id === UserTypeCode.SupportUser;
