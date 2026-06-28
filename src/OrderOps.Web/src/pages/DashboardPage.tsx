import layoutStyles from '../components/layout/AppLayout.module.css';

export function DashboardPage() {
  return (
    <section>
      <header className={layoutStyles.pageHeader}>
        <h1 className={layoutStyles.pageTitle}>Dashboard</h1>
        <p className={layoutStyles.pageDescription}>
          Order volume, revenue, and supplier analytics.
        </p>
      </header>
    </section>
  );
}
