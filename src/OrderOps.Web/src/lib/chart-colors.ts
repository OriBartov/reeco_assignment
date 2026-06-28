/** Chart palette aligned with order status semantics. */
export const STATUS_CHART_COLORS: Record<string, string> = {
  pending: '#f59e0b',
  approved: '#22c55e',
  rejected: '#ef4444',
  shipped: '#3b82f6',
  delivered: '#14b8a6',
  cancelled: '#94a3b8',
};

export const CHART_COLORS = [
  '#2563eb',
  '#7c3aed',
  '#db2777',
  '#ea580c',
  '#059669',
  '#0891b2',
  '#ca8a04',
  '#64748b',
];

export function statusColor(status: string, index = 0): string {
  return STATUS_CHART_COLORS[status] ?? CHART_COLORS[index % CHART_COLORS.length];
}
