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
}

/** Mirrors Fino_API `Responses/Login/TokenResponse.cs`. */
export interface Tokens {
  AccessToken: string;
  RefreshToken: string;
  ExpiresIn: number;
  TokenType: string;
  JwtId: string;
}

/** What `POST api/Auth/login` returns inside its (encrypted) Data field. */
export interface LoginResult {
  user: LoginUser;
  tokens: Tokens;
}

/** Persisted client session. */
export interface Session {
  user: LoginUser;
  tokens: Tokens;
}
