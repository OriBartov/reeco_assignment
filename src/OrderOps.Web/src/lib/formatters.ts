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
