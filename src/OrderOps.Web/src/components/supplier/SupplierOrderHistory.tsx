import { formatCurrency, formatDate, formatStatusLabel, formatWarehouse } from '../../lib/formatters';
import { DEFAULT_PAGE_SIZE, PAGE_SIZE_OPTIONS } from '../../lib/order-constants';
import type { OrderListItem } from '../../types';
import tableStyles from '../orders/OrdersTable.module.css';
import styles from './SupplierOrderHistory.module.css';

export interface SupplierOrderHistoryProps {
  orders: OrderListItem[];
  total: number;
  page: number;
  pageSize: number;
  isLoading?: boolean;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

function SkeletonRows({ count }: { count: number }) {
  return (
    <>
      {Array.from({ length: count }, (_, i) => (
        <tr key={i} className={tableStyles.skeletonRow}>
          {Array.from({ length: 8 }, (_, j) => (
            <td key={j}>
              <span className={tableStyles.skeleton} />
            </td>
          ))}
        </tr>
      ))}
    </>
  );
}

export function SupplierOrderHistory({
  orders,
  total,
  page,
  pageSize,
  isLoading,
  onPageChange,
  onPageSizeChange,
}: SupplierOrderHistoryProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const rangeStart = total === 0 ? 0 : (page - 1) * pageSize + 1;
  const rangeEnd = Math.min(page * pageSize, total);

  return (
    <section className={styles.section} aria-labelledby="supplier-orders-heading">
      <h2 id="supplier-orders-heading" className={styles.heading}>
        Order history
      </h2>

      <div className={tableStyles.tableWrap}>
        <table className={tableStyles.table}>
          <thead>
            <tr>
              <th scope="col">ID</th>
              <th scope="col">Product</th>
              <th scope="col">Qty</th>
              <th scope="col">Total</th>
              <th scope="col">Status</th>
              <th scope="col">Priority</th>
              <th scope="col">Warehouse</th>
              <th scope="col">Created</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <SkeletonRows count={pageSize || DEFAULT_PAGE_SIZE} />
            ) : orders.length === 0 ? (
              <tr>
                <td colSpan={8} className={styles.emptyCell}>
                  No orders for this supplier.
                </td>
              </tr>
            ) : (
              orders.map((order) => (
                <tr key={order.id}>
                  <td className={tableStyles.mono}>{order.id}</td>
                  <td>{order.product_name ?? order.product_id}</td>
                  <td>{order.quantity.toLocaleString()}</td>
                  <td>{formatCurrency(order.total_price)}</td>
                  <td>
                    <span className={styles.status}>{formatStatusLabel(order.status)}</span>
                  </td>
                  <td className={tableStyles.priority}>{order.priority}</td>
                  <td className={tableStyles.warehouse}>{formatWarehouse(order.warehouse)}</td>
                  <td>{formatDate(order.created_at)}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className={tableStyles.pagination}>
        <p className={tableStyles.paginationInfo}>
          {total === 0
            ? 'No orders'
            : `Showing ${rangeStart.toLocaleString()}–${rangeEnd.toLocaleString()} of ${total.toLocaleString()}`}
        </p>
        <div className={tableStyles.paginationControls}>
          <label className={tableStyles.paginationInfo} htmlFor="supplier-page-size">
            Rows
          </label>
          <select
            id="supplier-page-size"
            className={tableStyles.pageSizeSelect}
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
            className={tableStyles.pageButton}
            disabled={page <= 1 || isLoading}
            onClick={() => onPageChange(page - 1)}
          >
            Previous
          </button>
          <span className={tableStyles.paginationInfo}>
            Page {page} of {totalPages}
          </span>
          <button
            type="button"
            className={tableStyles.pageButton}
            disabled={page >= totalPages || isLoading}
            onClick={() => onPageChange(page + 1)}
          >
            Next
          </button>
        </div>
      </div>
    </section>
  );
}
