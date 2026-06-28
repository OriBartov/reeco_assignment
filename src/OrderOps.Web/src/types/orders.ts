export type OrderStatus =
  | 'pending'
  | 'approved'
  | 'rejected'
  | 'shipped'
  | 'delivered'
  | 'cancelled';

export type OrderPriority = 'low' | 'normal' | 'high' | 'critical' | (string & {});

export interface OrderListItem {
  id: string;
  supplier_id: string;
  product_id: string;
  quantity: number;
  unit_price: number;
  total_price: number;
  status: OrderStatus;
  priority: OrderPriority;
  created_at: string;
  updated_at: string;
  warehouse: string | null;
  notes: string | null;
  version: number;
  product_name?: string;
}

export interface OrderDetail extends OrderListItem {
  supplier_name: string;
  product_name: string;
}

export interface OrderListQuery {
  limit?: number;
  offset?: number;
  status?: string;
  priority?: string;
  supplier_id?: string;
  warehouse?: string;
  date_from?: string;
  date_to?: string;
  min_total?: number;
  search?: string;
  sort?: string;
  order?: 'asc' | 'desc';
}

export interface UpdateOrderRequest {
  status?: OrderStatus;
  priority?: OrderPriority;
}

export type BulkActionType = 'approve' | 'reject' | 'flag';

/** Request body for POST /api/orders/bulk-action (orderIds uses camelCase per API contract). */
export interface BulkActionRequest {
  orderIds: string[];
  action: BulkActionType;
  reason?: string;
}

/** 202 response from POST /api/orders/bulk-action (jobId uses camelCase per API contract). */
export interface BulkActionJobResponse {
  jobId: string;
}
