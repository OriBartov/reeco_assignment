import { useQuery } from '@tanstack/react-query';
import { listSuppliers } from '../api/suppliers';

export function useSuppliersLookup() {
  return useQuery({
    queryKey: ['suppliers', { limit: 500 }],
    queryFn: ({ signal }) => listSuppliers({ limit: 500 }, { signal }),
    staleTime: 5 * 60_000,
    select: (response) => {
      const map = new Map<string, string>();
      for (const supplier of response.data) {
        map.set(supplier.id, supplier.name);
      }
      return map;
    },
  });
}
