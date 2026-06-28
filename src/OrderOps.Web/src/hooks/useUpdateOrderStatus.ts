import { useMutation, useQueryClient } from '@tanstack/react-query';
import { updateOrder } from '../api/orders';
import { ApiClientError } from '../api/client';
import type { OrderDetail, OrderListQuery, OrderListItem, OrderStatus, PaginatedResponse } from '../types';
import { ordersQueryKey } from './useOrdersQuery';

interface UpdateStatusVariables {
  id: string;
  status: OrderStatus;
  query: OrderListQuery;
}

interface MutationContext {
  previous?: [readonly unknown[], PaginatedResponse<OrderListItem> | undefined][];
}

export function useUpdateOrderStatus() {
  const queryClient = useQueryClient();

  return useMutation<OrderDetail, ApiClientError, UpdateStatusVariables, MutationContext>({
    mutationFn: ({ id, status }) => updateOrder(id, { status }),
    onMutate: async ({ id, status, query }) => {
      const key = ordersQueryKey(query);
      await queryClient.cancelQueries({ queryKey: ['orders'] });

      const previous = queryClient.getQueriesData<PaginatedResponse<OrderListItem>>({
        queryKey: ['orders'],
      });

      queryClient.setQueryData<PaginatedResponse<OrderListItem>>(key, (old) => {
        if (!old) {
          return old;
        }
        return {
          ...old,
          data: old.data.map((order) =>
            order.id === id ? { ...order, status } : order,
          ),
        };
      });

      return { previous: [...previous] };
    },
    onError: (_error, _variables, context) => {
      context?.previous?.forEach(([key, data]) => {
        queryClient.setQueryData(key, data);
      });
    },
    onSettled: (_data, _error, variables) => {
      queryClient.invalidateQueries({ queryKey: ordersQueryKey(variables.query) });
    },
  });
}
