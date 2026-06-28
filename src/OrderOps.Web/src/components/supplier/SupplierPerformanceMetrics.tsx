import { formatCurrency, formatDays, formatPercent } from '../../lib/formatters';
import type { SupplierPerformance } from '../../types';
import styles from './SupplierPerformanceMetrics.module.css';

export interface SupplierPerformanceMetricsProps {
  performance: SupplierPerformance | undefined;
  isLoading?: boolean;
}

const metrics = [
  {
    key: 'avg_delivery_days',
    label: 'Avg delivery time',
    format: formatDays,
  },
  {
    key: 'rejection_rate',
    label: 'Rejection rate',
    format: formatPercent,
  },
  {
    key: 'avg_order_value',
    label: 'Avg order value',
    format: formatCurrency,
  },
  {
    key: 'price_consistency',
    label: 'Price consistency',
    format: formatPercent,
  },
] as const;

export function SupplierPerformanceMetrics({
  performance,
  isLoading,
}: SupplierPerformanceMetricsProps) {
  return (
    <section className={styles.section} aria-labelledby="supplier-performance-heading">
      <h2 id="supplier-performance-heading" className={styles.heading}>
        Performance
      </h2>
      <div className={styles.grid}>
        {metrics.map(({ key, label, format }) => (
          <article key={key} className={styles.card} aria-busy={isLoading}>
            <p className={styles.label}>{label}</p>
            {isLoading ? (
              <div className={styles.skeleton} aria-hidden="true" />
            ) : (
              <p className={styles.value}>{format(performance?.[key] ?? 0)}</p>
            )}
          </article>
        ))}
      </div>
    </section>
  );
}
