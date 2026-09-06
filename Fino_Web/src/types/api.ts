/** Raw envelope returned by every Fino_API endpoint (`Responses/APIResponse.cs`). */
export interface ApiEnvelope {
  StatusCode: number;
  Status: string;
  Message: string;
  Data: string;
}

export const STATUS = {
  SUCCESS: 'SUCCESS',
  FAILED: 'FAILED',
  UNAUTHORIZED: 'UNAUTHORIZED',
  ERROR: 'ERROR',
} as const;

/** Normalised result the app works with after unwrapping + decrypting Data. */
export interface ApiResult<T> {
  ok: boolean;
  status: string;
  message: string;
  data: T | null;
}

/** Thrown for transport/auth failures so React Query can surface them. */
export class ApiError extends Error {
  status: string;
  httpStatus: number;
  constructor(message: string, status = 'ERROR', httpStatus = 0) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.httpStatus = httpStatus;
  }
}
