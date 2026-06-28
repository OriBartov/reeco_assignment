import { buildQueryString } from '../lib/build-query-string';
import type {
  AnomaliesListResponse,
  BulkActionJobResponse,
  BulkActionRequest,
  OrderDetail,
  OrderListItem,
  OrderListQuery,
  OrderStats,
  PaginatedResponse,
  UpdateOrderRequest,
} from '../types';
import { apiGet, apiPatch, apiPost, type ApiRequestOptions } from './client';

export function listOrders(query: OrderListQuery = {}, options?: ApiRequestOptions) {
  return apiGet<PaginatedResponse<OrderListItem>>(`/orders${buildQueryString(query)}`, options);
}

export function getOrder(id: string) {
  return apiGet<OrderDetail>(`/orders/${encodeURIComponent(id)}`);
}

export function updateOrder(id: string, body: UpdateOrderRequest) {
  return apiPatch<OrderDetail>(`/orders/${encodeURIComponent(id)}`, body);
}

export function getOrderStats(options?: ApiRequestOptions) {
  return apiGet<OrderStats>('/orders/stats', options);
}

export function getOrderAnomalies() {
  return apiGet<AnomaliesListResponse>('/orders/anomalies');
}

export function bulkAction(body: BulkActionRequest) {
  return apiPost<BulkActionJobResponse>('/orders/bulk-action', body);
}
