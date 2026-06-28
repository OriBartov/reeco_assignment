export interface EventsFilter {
  supplier_id?: string;
}

export function buildEventsUrl(filter?: EventsFilter, baseUrl = '/api'): string {
  const params = new URLSearchParams();
  if (filter?.supplier_id) {
    params.set('supplier_id', filter.supplier_id);
  }
  const query = params.toString();
  return `${baseUrl}/events${query ? `?${query}` : ''}`;
}
