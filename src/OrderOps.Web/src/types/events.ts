export type EventType = 'order_updated' | 'bulk_completed' | (string & {});

export interface OrderUpdatedEventData {
  id: string;
  old_status: string;
  new_status: string;
  updated_at: string;
}

/** bulk_completed payload uses camelCase jobId per API contract. */
export interface BulkCompletedEventData {
  jobId: string;
}

export interface ServerEvent<T = Record<string, unknown>> {
  type: EventType;
  data: T;
}

export type OrderUpdatedEvent = ServerEvent<OrderUpdatedEventData>;
export type BulkCompletedEvent = ServerEvent<BulkCompletedEventData>;
