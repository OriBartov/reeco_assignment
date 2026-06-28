import type { ApiErrorBody } from '../types/common';

const DEFAULT_API_BASE = '/api';

export class ApiClientError extends Error {
  readonly status: number;
  readonly code: string;

  constructor(status: number, body: ApiErrorBody) {
    super(body.error);
    this.name = 'ApiClientError';
    this.status = status;
    this.code = body.code;
  }

  get isNotFound(): boolean {
    return this.status === 404 || this.code === 'NOT_FOUND';
  }

  get isConflict(): boolean {
    return this.status === 409 || this.code === 'CONFLICT';
  }

  get isBadRequest(): boolean {
    return this.status === 400 || this.code === 'BAD_REQUEST';
  }
}

export interface ApiRequestOptions {
  /** Override base path (default `/api`). Useful for tests. */
  baseUrl?: string;
  signal?: AbortSignal;
}

async function parseErrorBody(response: Response): Promise<ApiErrorBody> {
  const contentType = response.headers.get('content-type') ?? '';

  if (contentType.includes('application/json')) {
    try {
      const body = (await response.json()) as Partial<ApiErrorBody>;
      return {
        error: body.error ?? response.statusText,
        code: body.code ?? 'UNKNOWN',
      };
    } catch {
      // fall through
    }
  }

  return {
    error: response.statusText || 'Request failed',
    code: 'UNKNOWN',
  };
}

export async function apiRequest<T>(
  method: string,
  path: string,
  body?: unknown,
  options: ApiRequestOptions = {},
): Promise<T> {
  const baseUrl = options.baseUrl ?? DEFAULT_API_BASE;
  const url = path.startsWith('http')
    ? path
    : `${baseUrl}${path.startsWith('/') ? path : `/${path}`}`;

  const headers: HeadersInit = {};
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }

  const response = await fetch(url, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    signal: options.signal,
  });

  if (!response.ok) {
    const errorBody = await parseErrorBody(response);
    throw new ApiClientError(response.status, errorBody);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get('content-type') ?? '';
  if (!contentType.includes('application/json')) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const apiGet = <T>(path: string, options?: ApiRequestOptions) =>
  apiRequest<T>('GET', path, undefined, options);

export const apiPost = <T>(path: string, body?: unknown, options?: ApiRequestOptions) =>
  apiRequest<T>('POST', path, body, options);

export const apiPatch = <T>(path: string, body?: unknown, options?: ApiRequestOptions) =>
  apiRequest<T>('PATCH', path, body, options);

export const apiDelete = <T>(path: string, options?: ApiRequestOptions) =>
  apiRequest<T>('DELETE', path, undefined, options);
