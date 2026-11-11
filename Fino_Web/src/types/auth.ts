/** One of the roles a person may continue as — see `LoginUser.Profiles`. */
export interface LoginProfileOption {
  UserID: number;
  UserTypeID: number;
  UserType: string;
  FirmName: string;
  UserCode: string;
  /** The account sign-in authenticated against; the issued token belongs to this one. */
  IsPrimary: boolean;
}

/** Mirrors Fino_API `Responses/Login/LoginResponse.cs`. */
export interface LoginUser {
  UserID: number;
  PersonName: string;
  FirmName: string;
  MobileNumber: string;
  EmailID: string;
  UserTypeID: number;
  UserType: string;
  IsKYCApproved: boolean;
  IsTwoFA: boolean;
  /**
   * The logins this mobile number carries, when it carries more than one.
   *
   * Absent or a single entry for almost every account, and that is the old behaviour exactly:
   * sign in and go. Two or more means the person set up a second role, and they are asked
   * which one to continue as. The token that arrives with this is always the primary's.
   */
  Profiles?: LoginProfileOption[];
}

/** Mirrors Fino_API `Responses/Login/TokenResponse.cs`. */
export interface Tokens {
  AccessToken: string;
  RefreshToken: string;
  ExpiresIn: number;
  TokenType: string;
  JwtId: string;
}

/**
 * What `POST api/Auth/login` returns inside its (encrypted) Data field.
 *
 * `tokens` is null when the password was right but the account has an authenticator enrolled:
 * the call succeeds, `user.IsTwoFA` is true, and nothing else on `user` is populated — a correct
 * password alone must not reveal the person's name or firm. The sign-in finishes through
 * `POST api/Auth/login-2fa`.
 */
export interface LoginResult {
  user: LoginUser;
  tokens: Tokens | null;
}

/** Persisted client session. */
export interface Session {
  user: LoginUser;
  tokens: Tokens;
}
