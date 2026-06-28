const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

export function formatCurrency(value: number): string {
  return currencyFormatter.format(value);
}

export function formatDate(iso: string): string {
  return dateFormatter.format(new Date(iso));
}

export function formatWarehouse(warehouse: string | null): string {
  if (!warehouse) {
    return 'unassigned';
  }
  return warehouse.replace(/^warehouse_/, '').replace(/_/g, ' ');
}

const monthFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  year: 'numeric',
});

const compactNumberFormatter = new Intl.NumberFormat('en-US');

export function formatMonth(monthKey: string): string {
  const [year, month] = monthKey.split('-');
  const date = new Date(Number(year), Number(month) - 1, 1);
  return monthFormatter.format(date);
}

export function formatCount(value: number): string {
  return compactNumberFormatter.format(value);
}

export function formatStatusLabel(status: string): string {
  return status.replace(/_/g, ' ');
}

const percentFormatter = new Intl.NumberFormat('en-US', {
  style: 'percent',
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
});

export function formatPercent(ratio: number): string {
  return percentFormatter.format(ratio);
}

export function formatRating(rating: number | null): string {
  if (rating === null) {
    return '—';
  }
  return `${rating.toFixed(1)} / 5`;
}

export function formatDays(days: number): string {
  if (days === 0) {
    return '—';
  }
  const rounded = Math.round(days * 10) / 10;
  return `${rounded} day${rounded === 1 ? '' : 's'}`;
}
