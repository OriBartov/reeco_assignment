import { useMutation } from '@tanstack/react-query';
import { bulkAction } from '../api/orders';
import { ApiClientError } from '../api/client';
import type { BulkActionJobResponse, BulkActionRequest } from '../types';

export function useBulkAction() {
  return useMutation<BulkActionJobResponse, ApiClientError, BulkActionRequest>({
    mutationFn: (body) => bulkAction(body),
  });
}
