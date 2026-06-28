import { useMemo } from 'react';
import layoutStyles from '../components/layout/AppLayout.module.css';
import { OrdersFilters } from '../components/orders/OrdersFilters';
import { OrdersTable } from '../components/orders/OrdersTable';
import { EmptyState, ErrorMessage } from '../components/shared';
import { useOrderListParams } from '../hooks/useOrderListParams';
import { useOrdersQuery } from '../hooks/useOrdersQuery';
import { useSuppliersLookup } from '../hooks/useSuppliersLookup';
import styles from './OrdersPage.module.css';

export function OrdersPage() {
  const {
    query,
    statuses,
    sort,
    sortOrder,
    page,
    pageSize,
    setStatuses,
    setFilter,
    setSort,
    setPage,
    setPageSize,
    clearFilters,
    hasActiveFilters,
  } = useOrderListParams();

  const ordersQuery = useOrdersQuery(query);
  const suppliersQuery = useSuppliersLookup();

  const supplierOptions = useMemo(() => {
    const map = suppliersQuery.data ?? new Map<string, string>();
    return [...map.entries()]
      .map(([id, name]) => ({ id, name }))
      .sort((a, b) => a.name.localeCompare(b.name));
  }, [suppliersQuery.data]);

  const supplierNames = suppliersQuery.data ?? new Map<string, string>();
  const orders = ordersQuery.data?.data ?? [];
  const total = ordersQuery.data?.total ?? 0;
  const showEmpty = !ordersQuery.isLoading && !ordersQuery.isError && orders.length === 0;

  return (
    <section>
      <header className={layoutStyles.pageHeader}>
        <h1 className={layoutStyles.pageTitle}>Orders</h1>
        <p className={layoutStyles.pageDescription}>
          Filter, sort, and manage purchase orders.
        </p>
      </header>

      <div className={styles.content}>
        <OrdersFilters
          statuses={statuses}
          priority={query.priority ?? ''}
          supplierId={query.supplier_id ?? ''}
          warehouse={query.warehouse ?? ''}
          dateFrom={query.date_from ?? ''}
          dateTo={query.date_to ?? ''}
          search={query.search ?? ''}
          suppliers={supplierOptions}
          hasActiveFilters={hasActiveFilters}
          onStatusesChange={setStatuses}
          onPriorityChange={(value) => setFilter('priority', value)}
          onSupplierChange={(value) => setFilter('supplier_id', value)}
          onWarehouseChange={(value) => setFilter('warehouse', value)}
          onDateFromChange={(value) => setFilter('date_from', value)}
          onDateToChange={(value) => setFilter('date_to', value)}
          onSearchChange={(value) => setFilter('search', value)}
          onClearFilters={clearFilters}
        />

        {ordersQuery.isError ? (
          <ErrorMessage
            error={ordersQuery.error}
            title="Unable to load orders"
            onRetry={() => ordersQuery.refetch()}
          />
        ) : null}

        {showEmpty ? (
          <EmptyState
            title="No orders match filters"
            description={
              hasActiveFilters
                ? 'Try adjusting or clearing your filters to see more results.'
                : 'There are no orders to display.'
            }
            action={
              hasActiveFilters ? (
                <button type="button" className={styles.clearLink} onClick={clearFilters}>
                  Clear filters
                </button>
              ) : undefined
            }
          />
        ) : (
          <OrdersTable
            orders={orders}
            total={total}
            page={page}
            pageSize={pageSize}
            sort={sort}
            sortOrder={sortOrder}
            query={query}
            supplierNames={supplierNames}
            isLoading={ordersQuery.isLoading}
            onSort={setSort}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
          />
        )}
      </div>
    </section>
  );
}
