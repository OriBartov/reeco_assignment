import layoutStyles from '../components/layout/AppLayout.module.css';

export function OrdersPage() {
  return (
    <section>
      <header className={layoutStyles.pageHeader}>
        <h1 className={layoutStyles.pageTitle}>Orders</h1>
        <p className={layoutStyles.pageDescription}>
          Filter, sort, and manage purchase orders.
        </p>
      </header>
    </section>
  );
}
