import type { ReactNode } from 'react';
import styles from './ChartCard.module.css';

export interface ChartCardProps {
  title: string;
  description?: string;
  children: ReactNode;
  isLoading?: boolean;
}

export function ChartCard({ title, description, children, isLoading }: ChartCardProps) {
  return (
    <article className={styles.card}>
      <header className={styles.header}>
        <h2 className={styles.title}>{title}</h2>
        {description ? <p className={styles.description}>{description}</p> : null}
      </header>
      <div className={styles.body} aria-busy={isLoading}>
        {isLoading ? <div className={styles.skeleton} aria-hidden="true" /> : children}
      </div>
    </article>
  );
}
