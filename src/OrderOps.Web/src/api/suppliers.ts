import { buildQueryString } from '../lib/build-query-string';
import type {
  PaginatedResponse,
  SupplierDetail,
  SupplierListItem,
  SupplierListQuery,
  SupplierPerformance,
} from '../types';
import { apiGet, type ApiRequestOptions } from './client';

export function listSuppliers(query: SupplierListQuery = {}, options?: ApiRequestOptions) {
  return apiGet<PaginatedResponse<SupplierListItem>>(`/suppliers${buildQueryString(query)}`, options);
}

export function getSupplier(id: string, options?: ApiRequestOptions) {
  return apiGet<SupplierDetail>(`/suppliers/${encodeURIComponent(id)}`, options);
}

export function getSupplierPerformance(id: string, options?: ApiRequestOptions) {
  return apiGet<SupplierPerformance>(`/suppliers/${encodeURIComponent(id)}/performance`, options);
}
