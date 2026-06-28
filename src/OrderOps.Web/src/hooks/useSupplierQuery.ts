import { useQuery } from '@tanstack/react-query';
import { getSupplier } from '../api/suppliers';

export function supplierQueryKey(id: string) {
  return ['suppliers', id] as const;
}

export function useSupplierQuery(id: string | undefined) {
  return useQuery({
    queryKey: supplierQueryKey(id ?? ''),
    queryFn: ({ signal }) => getSupplier(id!, { signal }),
    enabled: Boolean(id),
  });
}
