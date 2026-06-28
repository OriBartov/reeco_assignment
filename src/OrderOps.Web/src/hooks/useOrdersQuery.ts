import { useQuery } from '@tanstack/react-query';
import { listOrders } from '../api/orders';
import type { OrderListQuery } from '../types';

export function ordersQueryKey(query: OrderListQuery) {
  return ['orders', query] as const;
}

export function useOrdersQuery(query: OrderListQuery, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ordersQueryKey(query),
    queryFn: ({ signal }) => listOrders(query, { signal }),
    enabled: options?.enabled ?? true,
  });
}
