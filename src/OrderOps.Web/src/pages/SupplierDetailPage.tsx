import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ApiClientError } from '../api/client';
import {
  SupplierMonthlyTrendChart,
  SupplierOrderHistory,
  SupplierPerformanceMetrics,
  SupplierProfileHeader,
} from '../components/supplier';
import { EmptyState, ErrorMessage } from '../components/shared';
import { useOrdersQuery } from '../hooks/useOrdersQuery';
import { useSupplierPerformanceQuery } from '../hooks/useSupplierPerformanceQuery';
import { useSupplierQuery } from '../hooks/useSupplierQuery';
import { DEFAULT_PAGE_SIZE } from '../lib/order-constants';
import styles from './SupplierDetailPage.module.css';

export function SupplierDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);

  const supplierQuery = useSupplierQuery(id);
  const supplierNotFound =
    supplierQuery.isError &&
    supplierQuery.error instanceof ApiClientError &&
    supplierQuery.error.isNotFound;

  const supplierLoaded = supplierQuery.isSuccess;
  const performanceQuery = useSupplierPerformanceQuery(id, supplierLoaded);
  const ordersQuery = useOrdersQuery(
    {
      supplier_id: id,
      limit: pageSize,
      offset: (page - 1) * pageSize,
      sort: 'created_at',
      order: 'desc',
    },
    { enabled: supplierLoaded },
  );

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setPage(1);
  };

  if (!id) {
    return (
      <EmptyState
        title="Invalid supplier"
        description="No supplier ID was provided in the URL."
        action={
          <Link to="/" className={styles.backLink}>
            Back to orders
          </Link>
        }
      />
    );
  }

  if (supplierNotFound) {
    return (
      <EmptyState
        title="Supplier not found"
        description={`No supplier exists with ID "${id}". It may have been removed or the link is incorrect.`}
        action={
          <Link to="/" className={styles.backLink}>
            Back to orders
          </Link>
        }
      />
    );
  }

  const supplier = supplierQuery.data;
  const performance = performanceQuery.data;
  const orders = ordersQuery.data?.data ?? [];
  const totalOrders = ordersQuery.data?.total ?? 0;

  const showProfileError = supplierQuery.isError && !supplierNotFound;
  const showPerformanceError = performanceQuery.isError;
  const showOrdersError = ordersQuery.isError;

  return (
    <section>
      <nav className={styles.breadcrumb} aria-label="Breadcrumb">
        <Link to="/">Orders</Link>
        <span aria-hidden="true">/</span>
        <span>{supplier?.name ?? id}</span>
      </nav>

      {showProfileError ? (
        <ErrorMessage
          error={supplierQuery.error}
          title="Unable to load supplier"
          onRetry={() => supplierQuery.refetch()}
        />
      ) : (
        <SupplierProfileHeader supplier={supplier} isLoading={supplierQuery.isLoading} />
      )}

      <div className={styles.content}>
        {showPerformanceError ? (
          <ErrorMessage
            error={performanceQuery.error}
            title="Unable to load performance metrics"
            onRetry={() => performanceQuery.refetch()}
          />
        ) : (
          <>
            <SupplierPerformanceMetrics
              performance={performance}
              isLoading={performanceQuery.isLoading}
            />
            <SupplierMonthlyTrendChart
              monthlyTrend={performance?.monthly_trend}
              isLoading={performanceQuery.isLoading}
            />
          </>
        )}

        {showOrdersError ? (
          <ErrorMessage
            error={ordersQuery.error}
            title="Unable to load order history"
            onRetry={() => ordersQuery.refetch()}
          />
        ) : (
          <SupplierOrderHistory
            orders={orders}
            total={totalOrders}
            page={page}
            pageSize={pageSize}
            isLoading={ordersQuery.isLoading}
            onPageChange={setPage}
            onPageSizeChange={handlePageSizeChange}
          />
        )}
      </div>
    </section>
  );
}
