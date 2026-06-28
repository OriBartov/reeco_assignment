import { formatCount, formatCurrency } from '../../lib/formatters';
import type { OrderStats } from '../../types';
import styles from './StatsSummaryCards.module.css';

export interface StatsSummaryCardsProps {
  stats: OrderStats | undefined;
  isLoading?: boolean;
}

const cards = [
  { key: 'total_orders', label: 'Total orders', format: formatCount },
  { key: 'total_revenue', label: 'Total revenue', format: formatCurrency },
  { key: 'avg_order_value', label: 'Avg order value', format: formatCurrency },
] as const;

export function StatsSummaryCards({ stats, isLoading }: StatsSummaryCardsProps) {
  return (
    <div className={styles.grid}>
      {cards.map(({ key, label, format }) => (
        <article key={key} className={styles.card} aria-busy={isLoading}>
          <p className={styles.label}>{label}</p>
          {isLoading ? (
            <div className={styles.skeleton} aria-hidden="true" />
          ) : (
            <p className={styles.value}>{format(stats?.[key] ?? 0)}</p>
          )}
        </article>
      ))}
    </div>
  );
}
