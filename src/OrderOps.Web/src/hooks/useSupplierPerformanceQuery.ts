import { useQuery } from '@tanstack/react-query';
import { getSupplierPerformance } from '../api/suppliers';

export function supplierPerformanceQueryKey(id: string) {
  return ['suppliers', id, 'performance'] as const;
}

export function useSupplierPerformanceQuery(id: string | undefined, enabled = true) {
  return useQuery({
    queryKey: supplierPerformanceQueryKey(id ?? ''),
    queryFn: ({ signal }) => getSupplierPerformance(id!, { signal }),
    enabled: Boolean(id) && enabled,
  });
}
