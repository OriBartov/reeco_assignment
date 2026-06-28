import {
  formatCount,
  formatCurrency,
  formatDate,
  formatRating,
} from '../../lib/formatters';
import type { SupplierDetail } from '../../types';
import styles from './SupplierProfileHeader.module.css';

export interface SupplierProfileHeaderProps {
  supplier: SupplierDetail | undefined;
  isLoading?: boolean;
}

export function SupplierProfileHeader({ supplier, isLoading }: SupplierProfileHeaderProps) {
  if (isLoading) {
    return (
      <header className={styles.header} aria-busy="true">
        <div className={styles.titleRow}>
          <div className={styles.skeletonTitle} aria-hidden="true" />
          <div className={styles.skeletonBadge} aria-hidden="true" />
        </div>
        <div className={styles.metaSkeleton} aria-hidden="true" />
        <div className={styles.stats}>
          {Array.from({ length: 2 }, (_, i) => (
            <div key={i} className={styles.statCard}>
              <div className={styles.skeletonLabel} aria-hidden="true" />
              <div className={styles.skeletonValue} aria-hidden="true" />
            </div>
          ))}
        </div>
      </header>
    );
  }

  if (!supplier) {
    return null;
  }

  return (
    <header className={styles.header}>
      <div className={styles.titleRow}>
        <h1 className={styles.name}>{supplier.name}</h1>
        <span
          className={[styles.badge, supplier.active ? styles.badgeActive : styles.badgeInactive].join(
            ' ',
          )}
        >
          {supplier.active ? 'Active' : 'Inactive'}
        </span>
      </div>

      <dl className={styles.meta}>
        <div className={styles.metaItem}>
          <dt>Country</dt>
          <dd>{supplier.country}</dd>
        </div>
        <div className={styles.metaItem}>
          <dt>Rating</dt>
          <dd>{formatRating(supplier.rating)}</dd>
        </div>
        <div className={styles.metaItem}>
          <dt>Email</dt>
          <dd>
            <a href={`mailto:${supplier.email}`}>{supplier.email}</a>
          </dd>
        </div>
        <div className={styles.metaItem}>
          <dt>Member since</dt>
          <dd>{formatDate(supplier.created_at)}</dd>
        </div>
      </dl>

      <div className={styles.stats}>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Total orders</p>
          <p className={styles.statValue}>{formatCount(supplier.order_count)}</p>
        </article>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Total revenue</p>
          <p className={styles.statValue}>{formatCurrency(supplier.total_revenue)}</p>
        </article>
      </div>
    </header>
  );
}
