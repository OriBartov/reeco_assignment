import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { buildEventsUrl } from '../api/events';
import type { ServerEvent } from '../types';
import { statsQueryKey } from './useStatsQuery';

export interface UseEventStreamOptions {
  supplierId?: string;
  enabled?: boolean;
}

function parseServerEvent(raw: string): ServerEvent | null {
  try {
    const parsed = JSON.parse(raw) as ServerEvent;
    if (typeof parsed.type === 'string') {
      return parsed;
    }
  } catch {
    // ignore malformed payloads
  }
  return null;
}

export function useEventStream(options: UseEventStreamOptions = {}) {
  const { supplierId, enabled = true } = options;
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!enabled) {
      return;
    }

    const url = buildEventsUrl(supplierId ? { supplier_id: supplierId } : undefined);
    const source = new EventSource(url);

    const handleMessage = (event: MessageEvent<string>) => {
      const serverEvent = parseServerEvent(event.data);
      if (!serverEvent) {
        return;
      }

      if (serverEvent.type === 'order_updated' || serverEvent.type === 'bulk_completed') {
        queryClient.invalidateQueries({ queryKey: ['orders'] });
      }

      if (serverEvent.type === 'bulk_completed') {
        queryClient.invalidateQueries({ queryKey: statsQueryKey });
      }
    };

    source.addEventListener('message', handleMessage);

    return () => {
      source.removeEventListener('message', handleMessage);
      source.close();
    };
  }, [enabled, queryClient, supplierId]);
}
