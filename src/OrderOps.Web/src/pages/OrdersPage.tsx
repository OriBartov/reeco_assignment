import { useCallback, useEffect, useMemo, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import layoutStyles from '../components/layout/AppLayout.module.css';
import { BulkActionBar } from '../components/orders/BulkActionBar';
import { BulkActionModal } from '../components/orders/BulkActionModal';
import { BulkJobProgressModal } from '../components/orders/BulkJobProgressModal';
import { OrdersFilters } from '../components/orders/OrdersFilters';
import { OrdersTable } from '../components/orders/OrdersTable';
import { EmptyState, ErrorMessage } from '../components/shared';
import { useBulkAction } from '../hooks/useBulkAction';
import { isJobTerminal, useBulkJobQuery } from '../hooks/useBulkJobQuery';
import { useOrderListParams } from '../hooks/useOrderListParams';
import { useOrdersQuery } from '../hooks/useOrdersQuery';
import { useSuppliersLookup } from '../hooks/useSuppliersLookup';
import type { BulkActionType } from '../types';
import { statsQueryKey } from '../hooks/useStatsQuery';
import styles from './OrdersPage.module.css';

export function OrdersPage() {
  const queryClient = useQueryClient();
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
  const bulkAction = useBulkAction();

  const [selectedIds, setSelectedIds] = useState<Set<string>>(() => new Set());
  const [pendingAction, setPendingAction] = useState<BulkActionType | null>(null);
  const [activeJobId, setActiveJobId] = useState<string | null>(null);
  const [bulkError, setBulkError] = useState<string | null>(null);

  const jobQuery = useBulkJobQuery(activeJobId);
  const jobComplete = isJobTerminal(jobQuery.data);

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
  const bulkBusy = bulkAction.isPending || (!!activeJobId && !jobComplete);

  useEffect(() => {
    if (!activeJobId || !jobComplete) {
      return;
    }

    queryClient.invalidateQueries({ queryKey: ['orders'] });
    queryClient.invalidateQueries({ queryKey: statsQueryKey });
  }, [activeJobId, jobComplete, queryClient]);

  useEffect(() => {
    setSelectedIds((current) => {
      const visibleIds = new Set(orders.map((order) => order.id));
      const next = new Set([...current].filter((id) => visibleIds.has(id)));
      return next.size === current.size ? current : next;
    });
  }, [orders]);

  const toggleRow = useCallback((id: string) => {
    setSelectedIds((current) => {
      const next = new Set(current);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  const togglePage = useCallback(
    (checked: boolean) => {
      setSelectedIds((current) => {
        const next = new Set(current);
        for (const order of orders) {
          if (checked) {
            next.add(order.id);
          } else {
            next.delete(order.id);
          }
        }
        return next;
      });
    },
    [orders],
  );

  const clearSelection = useCallback(() => {
    setSelectedIds(new Set());
  }, []);

  const handleBulkConfirm = (reason: string) => {
    if (!pendingAction || selectedIds.size === 0) {
      return;
    }

    setBulkError(null);
    bulkAction.mutate(
      {
        orderIds: [...selectedIds],
        action: pendingAction,
        reason: reason || undefined,
      },
      {
        onSuccess: (response) => {
          setPendingAction(null);
          setActiveJobId(response.jobId);
        },
        onError: (error) => {
          setBulkError(error.message);
        },
      },
    );
  };

  const handleJobDismiss = () => {
    setActiveJobId(null);
    setBulkError(null);
    clearSelection();
    ordersQuery.refetch();
  };

  return (
    <section className={selectedIds.size > 0 ? styles.withBulkBar : undefined}>
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
            selectedIds={selectedIds}
            isLoading={ordersQuery.isLoading}
            onSort={setSort}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
            onToggleRow={toggleRow}
            onTogglePage={togglePage}
          />
        )}
      </div>

      <BulkActionBar
        selectedCount={selectedIds.size}
        disabled={bulkBusy}
        onAction={setPendingAction}
        onClear={clearSelection}
      />

      <BulkActionModal
        open={pendingAction !== null}
        action={pendingAction ?? 'approve'}
        selectedCount={selectedIds.size}
        isSubmitting={bulkAction.isPending}
        errorMessage={bulkError}
        onConfirm={handleBulkConfirm}
        onCancel={() => {
          setPendingAction(null);
          setBulkError(null);
        }}
      />

      <BulkJobProgressModal
        open={activeJobId !== null}
        status={jobQuery.data?.status}
        progress={jobQuery.data?.progress}
        errorMessage={jobQuery.isError ? jobQuery.error.message : null}
        onDismiss={handleJobDismiss}
      />
    </section>
  );
}
