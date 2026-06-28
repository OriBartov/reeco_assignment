import { useQuery } from '@tanstack/react-query';
import { getOrderStats } from '../api/orders';

export const statsQueryKey = ['orders', 'stats'] as const;

export function useStatsQuery() {
  return useQuery({
    queryKey: statsQueryKey,
    queryFn: ({ signal }) => getOrderStats({ signal }),
  });
}
