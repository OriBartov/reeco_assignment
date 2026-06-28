/** Paginated list envelope returned by list endpoints. */
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  limit: number;
  offset: number;
}

/** Error body shape returned by the API on non-2xx responses. */
export interface ApiErrorBody {
  error: string;
  code: string;
}

export type ApiErrorCode =
  | 'NOT_FOUND'
  | 'BAD_REQUEST'
  | 'CONFLICT'
  | 'INTERNAL_ERROR'
  | (string & {});
