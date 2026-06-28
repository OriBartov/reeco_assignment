import type { OrderPriority, OrderStatus } from '../types';

export const ORDER_STATUSES: OrderStatus[] = [
  'pending',
  'approved',
  'rejected',
  'shipped',
  'delivered',
  'cancelled',
];

export const ORDER_PRIORITIES: OrderPriority[] = ['low', 'normal', 'high', 'critical'];

export const WAREHOUSES = [
  'warehouse_east',
  'warehouse_west',
  'warehouse_north',
  'warehouse_south',
  'warehouse_central',
  'unassigned',
] as const;

export const DEFAULT_PAGE_SIZE = 20;

export const PAGE_SIZE_OPTIONS = [20, 50, 100] as const;

export type SortableOrderColumn =
  | 'created_at'
  | 'total_price'
  | 'quantity'
  | 'status'
  | 'priority'
  | 'warehouse';

export const SORTABLE_COLUMNS: { key: SortableOrderColumn; label: string }[] = [
  { key: 'created_at', label: 'Created' },
  { key: 'total_price', label: 'Total' },
  { key: 'quantity', label: 'Qty' },
  { key: 'status', label: 'Status' },
  { key: 'priority', label: 'Priority' },
  { key: 'warehouse', label: 'Warehouse' },
];
