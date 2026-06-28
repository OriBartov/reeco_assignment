import type { OrderStatus } from '../../types';
import {
  ORDER_PRIORITIES,
  ORDER_STATUSES,
  WAREHOUSES,
} from '../../lib/order-constants';
import styles from './OrdersFilters.module.css';

export interface OrdersFiltersProps {
  statuses: OrderStatus[];
  priority: string;
  supplierId: string;
  warehouse: string;
  dateFrom: string;
  dateTo: string;
  search: string;
  suppliers: { id: string; name: string }[];
  hasActiveFilters: boolean;
  onStatusesChange: (statuses: OrderStatus[]) => void;
  onPriorityChange: (value: string) => void;
  onSupplierChange: (value: string) => void;
  onWarehouseChange: (value: string) => void;
  onDateFromChange: (value: string) => void;
  onDateToChange: (value: string) => void;
  onSearchChange: (value: string) => void;
  onClearFilters: () => void;
}

export function OrdersFilters({
  statuses,
  priority,
  supplierId,
  warehouse,
  dateFrom,
  dateTo,
  search,
  suppliers,
  hasActiveFilters,
  onStatusesChange,
  onPriorityChange,
  onSupplierChange,
  onWarehouseChange,
  onDateFromChange,
  onDateToChange,
  onSearchChange,
  onClearFilters,
}: OrdersFiltersProps) {
  const toggleStatus = (status: OrderStatus) => {
    if (statuses.includes(status)) {
      onStatusesChange(statuses.filter((s) => s !== status));
    } else {
      onStatusesChange([...statuses, status]);
    }
  };

  return (
    <div className={styles.filters}>
      <div className={styles.row}>
        <div className={styles.fieldGrow}>
          <span className={styles.label}>Search</span>
          <input
            type="search"
            className={styles.input}
            placeholder="Product name…"
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
            aria-label="Search orders by product name"
          />
        </div>
        <div className={styles.actions}>
          <button
            type="button"
            className={styles.clearButton}
            onClick={onClearFilters}
            disabled={!hasActiveFilters}
          >
            Clear filters
          </button>
        </div>
      </div>

      <div className={styles.row}>
        <div className={styles.field}>
          <span className={styles.label}>Status</span>
          <div className={styles.statusGroup} role="group" aria-label="Filter by status">
            {ORDER_STATUSES.map((status) => {
              const active = statuses.includes(status);
              return (
                <button
                  key={status}
                  type="button"
                  className={[styles.statusChip, active ? styles.statusChipActive : ''].join(' ')}
                  aria-pressed={active}
                  onClick={() => toggleStatus(status)}
                >
                  {status}
                </button>
              );
            })}
          </div>
        </div>
      </div>

      <div className={styles.row}>
        <div className={styles.field}>
          <label className={styles.label} htmlFor="filter-priority">
            Priority
          </label>
          <select
            id="filter-priority"
            className={styles.select}
            value={priority}
            onChange={(e) => onPriorityChange(e.target.value)}
          >
            <option value="">All priorities</option>
            {ORDER_PRIORITIES.map((p) => (
              <option key={p} value={p}>
                {p}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.field}>
          <label className={styles.label} htmlFor="filter-supplier">
            Supplier
          </label>
          <select
            id="filter-supplier"
            className={styles.select}
            value={supplierId}
            onChange={(e) => onSupplierChange(e.target.value)}
          >
            <option value="">All suppliers</option>
            {suppliers.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.field}>
          <label className={styles.label} htmlFor="filter-warehouse">
            Warehouse
          </label>
          <select
            id="filter-warehouse"
            className={styles.select}
            value={warehouse}
            onChange={(e) => onWarehouseChange(e.target.value)}
          >
            <option value="">All warehouses</option>
            {WAREHOUSES.map((w) => (
              <option key={w} value={w}>
                {w.replace(/^warehouse_/, '').replace(/_/g, ' ')}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.field}>
          <label className={styles.label} htmlFor="filter-date-from">
            From
          </label>
          <input
            id="filter-date-from"
            type="date"
            className={styles.input}
            value={dateFrom}
            onChange={(e) => onDateFromChange(e.target.value)}
          />
        </div>

        <div className={styles.field}>
          <label className={styles.label} htmlFor="filter-date-to">
            To
          </label>
          <input
            id="filter-date-to"
            type="date"
            className={styles.input}
            value={dateTo}
            onChange={(e) => onDateToChange(e.target.value)}
          />
        </div>
      </div>
    </div>
  );
}
