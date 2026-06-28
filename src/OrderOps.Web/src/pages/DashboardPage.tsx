import layoutStyles from '../components/layout/AppLayout.module.css';
import {
  MonthlyTrendChart,
  StatsSummaryCards,
  StatusDistributionChart,
  TopSuppliersChart,
} from '../components/dashboard';
import { EmptyState, ErrorMessage } from '../components/shared';
import { useStatsQuery } from '../hooks/useStatsQuery';
import styles from './DashboardPage.module.css';

export function DashboardPage() {
  const statsQuery = useStatsQuery();
  const stats = statsQuery.data;
  const isLoading = statsQuery.isLoading;
  const showEmpty =
    !isLoading && !statsQuery.isError && stats !== undefined && stats.total_orders === 0;

  return (
    <section>
      <header className={layoutStyles.pageHeader}>
        <h1 className={layoutStyles.pageTitle}>Dashboard</h1>
        <p className={layoutStyles.pageDescription}>
          Order volume, revenue, and supplier analytics.
        </p>
      </header>

      <div className={styles.content}>
        {statsQuery.isError ? (
          <ErrorMessage
            error={statsQuery.error}
            title="Unable to load dashboard"
            onRetry={() => statsQuery.refetch()}
          />
        ) : null}

        {showEmpty ? (
          <EmptyState
            title="No order data yet"
            description="Analytics will appear once orders are imported into the system."
          />
        ) : (
          <>
            <StatsSummaryCards stats={stats} isLoading={isLoading} />

            <div className={styles.chartsGrid}>
              <StatusDistributionChart byStatus={stats?.by_status} isLoading={isLoading} />
              <MonthlyTrendChart byMonth={stats?.by_month} isLoading={isLoading} />
            </div>

            <TopSuppliersChart topSuppliers={stats?.top_suppliers} isLoading={isLoading} />
          </>
        )}
      </div>
    </section>
  );
}
