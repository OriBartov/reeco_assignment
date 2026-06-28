import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ApiClientError } from '../../api/client';
import { formatCurrency, formatDate, formatWarehouse } from '../../lib/formatters';
import {
  ORDER_STATUSES,
  PAGE_SIZE_OPTIONS,
  type SortableOrderColumn,
} from '../../lib/order-constants';
import type { OrderListItem, OrderListQuery, OrderStatus } from '../../types';
import { useUpdateOrderStatus } from '../../hooks/useUpdateOrderStatus';
import styles from './OrdersTable.module.css';

export interface OrdersTableProps {
  orders: OrderListItem[];
  total: number;
  page: number;
  pageSize: number;
  sort: SortableOrderColumn | undefined;
  sortOrder: 'asc' | 'desc';
  query: OrderListQuery;
  supplierNames: Map<string, string>;
  selectedIds: Set<string>;
  isLoading?: boolean;
  onSort: (column: SortableOrderColumn) => void;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  onToggleRow: (id: string) => void;
  onTogglePage: (checked: boolean) => void;
}

function SortHeader({
  label,
  column,
  activeSort,
  sortOrder,
  onSort,
}: {
  label: string;
  column: SortableOrderColumn;
  activeSort: SortableOrderColumn | undefined;
  sortOrder: 'asc' | 'desc';
  onSort: (column: SortableOrderColumn) => void;
}) {
  const active = activeSort === column;
  return (
    <button
      type="button"
      className={[styles.sortButton, active ? styles.sortActive : ''].join(' ')}
      onClick={() => onSort(column)}
    >
      {label}
      {active ? (
        <span className={styles.sortIndicator} aria-hidden="true">
          {sortOrder === 'asc' ? '▲' : '▼'}
        </span>
      ) : null}
    </button>
  );
}

function SkeletonRows({ count }: { count: number }) {
  return (
    <>
      {Array.from({ length: count }, (_, i) => (
        <tr key={i} className={styles.skeletonRow}>
          {Array.from({ length: 10 }, (_, j) => (
            <td key={j}>
              <span className={styles.skeleton} />
            </td>
          ))}
        </tr>
      ))}
    </>
  );
}

function OrderStatusEditor({
  order,
  query,
}: {
  order: OrderListItem;
  query: OrderListQuery;
}) {
  const updateStatus = useUpdateOrderStatus();
  const [conflictMessage, setConflictMessage] = useState<string | null>(null);

  const handleChange = (nextStatus: OrderStatus) => {
    if (nextStatus === order.status) {
      return;
    }

    setConflictMessage(null);
    updateStatus.mutate(
      { id: order.id, status: nextStatus, query },
      {
        onError: (error) => {
          if (error instanceof ApiClientError && error.isConflict) {
            setConflictMessage(error.message);
          }
        },
        onSuccess: () => setConflictMessage(null),
      },
    );
  };

  const isDisabled = order.status === 'cancelled' || updateStatus.isPending;

  return (
    <div className={styles.statusCell}>
      <select
        className={[
          styles.statusSelect,
          conflictMessage ? styles.statusSelectError : '',
        ].join(' ')}
        value={order.status}
        disabled={isDisabled}
        aria-label={`Change status for order ${order.id}`}
        onChange={(e) => handleChange(e.target.value as OrderStatus)}
      >
        {ORDER_STATUSES.map((status) => (
          <option key={status} value={status}>
            {status}
          </option>
        ))}
      </select>
      {conflictMessage ? (
        <p className={styles.conflictMessage} role="alert">
          {conflictMessage}
        </p>
      ) : null}
    </div>
  );
}

export function OrdersTable({
  orders,
  total,
  page,
  pageSize,
  sort,
  sortOrder,
  query,
  supplierNames,
  selectedIds,
  isLoading,
  onSort,
  onPageChange,
  onPageSizeChange,
  onToggleRow,
  onTogglePage,
}: OrdersTableProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const rangeStart = total === 0 ? 0 : (page - 1) * pageSize + 1;
  const rangeEnd = Math.min(page * pageSize, total);
  const pageOrderIds = orders.map((order) => order.id);
  const selectedOnPageCount = pageOrderIds.filter((id) => selectedIds.has(id)).length;
  const allOnPageSelected = pageOrderIds.length > 0 && selectedOnPageCount === pageOrderIds.length;
  const someOnPageSelected = selectedOnPageCount > 0 && !allOnPageSelected;

  return (
    <>
      <div className={styles.tableWrap}>
        <table className={styles.table}>
          <thead>
            <tr>
              <th scope="col" className={styles.selectCol}>
                <input
                  type="checkbox"
                  className={styles.checkbox}
                  checked={allOnPageSelected}
                  ref={(input) => {
                    if (input) {
                      input.indeterminate = someOnPageSelected;
                    }
                  }}
                  disabled={isLoading || pageOrderIds.length === 0}
                  aria-label="Select all orders on this page"
                  onChange={(event) => onTogglePage(event.target.checked)}
                />
              </th>
              <th scope="col">ID</th>
              <th scope="col">Supplier</th>
              <th scope="col">Product</th>
              <th scope="col">
                <SortHeader
                  label="Qty"
                  column="quantity"
                  activeSort={sort}
                  sortOrder={sortOrder}
                  onSort={onSort}
                />
              </th>
              <th scope="col">
                <SortHeader
                  label="Total"
                  column="total_price"
                  activeSort={sort}
                  sortOrder={sortOrder}
                  onSort={onSort}
                />
              </th>
              <th scope="col">
                <SortHeader
                  label="Status"
                  column="status"
                  activeSort={sort}
                  sortOrder={sortOrder}
                  onSort={onSort}
                />
              </th>
              <th scope="col">
                <SortHeader
                  label="Priority"
                  column="priority"
                  activeSort={sort}
                  sortOrder={sortOrder}
                  onSort={onSort}
                />
              </th>
              <th scope="col">
                <SortHeader
                  label="Warehouse"
                  column="warehouse"
                  activeSort={sort}
                  sortOrder={sortOrder}
                  onSort={onSort}
                />
              </th>
              <th scope="col">
                <SortHeader
                  label="Created"
                  column="created_at"
                  activeSort={sort}
                  sortOrder={sortOrder}
                  onSort={onSort}
                />
              </th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <SkeletonRows count={pageSize} />
            ) : (
              orders.map((order) => (
                <tr key={order.id} className={selectedIds.has(order.id) ? styles.selectedRow : ''}>
                  <td className={styles.selectCol}>
                    <input
                      type="checkbox"
                      className={styles.checkbox}
                      checked={selectedIds.has(order.id)}
                      aria-label={`Select order ${order.id}`}
                      onChange={() => onToggleRow(order.id)}
                    />
                  </td>
                  <td className={styles.mono}>{order.id}</td>
                  <td>
                    <Link to={`/suppliers/${order.supplier_id}`}>
                      {supplierNames.get(order.supplier_id) ?? order.supplier_id}
                    </Link>
                  </td>
                  <td>{order.product_name ?? order.product_id}</td>
                  <td>{order.quantity.toLocaleString()}</td>
                  <td>{formatCurrency(order.total_price)}</td>
                  <td>
                    <OrderStatusEditor order={order} query={query} />
                  </td>
                  <td className={styles.priority}>{order.priority}</td>
                  <td className={styles.warehouse}>{formatWarehouse(order.warehouse)}</td>
                  <td>{formatDate(order.created_at)}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className={styles.pagination}>
        <p className={styles.paginationInfo}>
          {total === 0
            ? 'No orders'
            : `Showing ${rangeStart.toLocaleString()}–${rangeEnd.toLocaleString()} of ${total.toLocaleString()}`}
        </p>
        <div className={styles.paginationControls}>
          <label className={styles.paginationInfo} htmlFor="page-size">
            Rows
          </label>
          <select
            id="page-size"
            className={styles.pageSizeSelect}
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
          >
            {PAGE_SIZE_OPTIONS.map((size) => (
              <option key={size} value={size}>
                {size}
              </option>
            ))}
          </select>
          <button
            type="button"
            className={styles.pageButton}
            disabled={page <= 1 || isLoading}
            onClick={() => onPageChange(page - 1)}
          >
            Previous
          </button>
          <span className={styles.paginationInfo}>
            Page {page} of {totalPages}
          </span>
          <button
            type="button"
            className={styles.pageButton}
            disabled={page >= totalPages || isLoading}
            onClick={() => onPageChange(page + 1)}
          >
            Next
          </button>
        </div>
      </div>
    </>
  );
}