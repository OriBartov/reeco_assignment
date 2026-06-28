import { useParams } from 'react-router-dom';
import layoutStyles from '../components/layout/AppLayout.module.css';

export function SupplierDetailPage() {
  const { id } = useParams<{ id: string }>();

  return (
    <section>
      <header className={layoutStyles.pageHeader}>
        <h1 className={layoutStyles.pageTitle}>Supplier</h1>
        <p className={layoutStyles.pageDescription}>
          Supplier #{id} — performance and order history.
        </p>
      </header>
    </section>
  );
}
