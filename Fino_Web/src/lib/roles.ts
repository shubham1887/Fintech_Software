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

/**
 * Which tiers a user of a given role may map under. Mirrors Fino_API
 * `UserService.AllowedParentTiers`.
 */
export const ALLOWED_PARENT_TIERS: Record<number, number[]> = {
  [UserTypeCode.SuperDistributor]: [UserTypeCode.Admin],
  [UserTypeCode.Distributor]: [UserTypeCode.Admin, UserTypeCode.SuperDistributor],
  [UserTypeCode.Retailer]: [UserTypeCode.Admin, UserTypeCode.SuperDistributor, UserTypeCode.Distributor],
  [UserTypeCode.ApiUser]: [UserTypeCode.Admin],
  // Back-office, not a network tier: sits under the admin, has nobody below it. The Add User
  // form drops any role missing from this map, so without this line "Accountant" never appears
  // in the role dropdown however well the rest of the role is wired up.
  [UserTypeCode.Accountant]: [UserTypeCode.Admin],
};

/**
 * Which roles the signed-in tier may CREATE from Add User. Mirrors Fino_API
 * `UserService.CreatableRoles`, which enforces it.
 *
 * A tier may only create strictly below itself. The form used to offer the same four roles to
 * everyone, so a distributor was shown Super Distributor and an upline picker covering the whole
 * network - and the server took both at face value. An API user is admin-created only: it is an
 * integration credential, not a network tier.
 */
export const CREATABLE_ROLES: Record<number, number[]> = {
  [UserTypeCode.Admin]: [
    UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
    UserTypeCode.Retailer, UserTypeCode.ApiUser,
    // Admin only — an accountant reads the company's own books, so support staff do not mint one.
    UserTypeCode.Accountant,
  ],
  [UserTypeCode.SupportUser]: [
    UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
    UserTypeCode.Retailer, UserTypeCode.ApiUser,
  ],
  [UserTypeCode.SuperDistributor]: [UserTypeCode.Distributor, UserTypeCode.Retailer],
  [UserTypeCode.Distributor]: [UserTypeCode.Retailer],
};

/** The roles `roleId` may create — empty when that tier creates nobody. */
export const creatableRoles = (roleId: number): number[] => CREATABLE_ROLES[roleId] ?? [];

export const isRetailer = (id: number) => id === UserTypeCode.Retailer;
export const isDistributorTier = (id: number) =>
  id === UserTypeCode.Distributor || id === UserTypeCode.SuperDistributor;
export const isAdminTier = (id: number) =>
  id === UserTypeCode.Admin || id === UserTypeCode.Accountant || id === UserTypeCode.SupportUser;
