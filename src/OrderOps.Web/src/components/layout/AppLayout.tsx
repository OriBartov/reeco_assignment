import { NavLink, Outlet, useMatch } from 'react-router-dom';
import { useEventStream } from '../../hooks/useEventStream';
import styles from './AppLayout.module.css';

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? `${styles.navLink} ${styles.navLinkActive}` : styles.navLink;

export function AppLayout() {
  const supplierMatch = useMatch('/suppliers/:id');
  const supplierId = supplierMatch?.params.id;

  useEventStream({ supplierId });

  return (
    <div className={styles.layout}>
      <header className={styles.header}>
        <NavLink to="/" className={styles.brand} end>
          <span className={styles.brandMark} aria-hidden="true">
            OO
          </span>
          OrderOps
        </NavLink>

        <nav className={styles.nav} aria-label="Main navigation">
          <NavLink to="/" className={navLinkClass} end>
            Orders
          </NavLink>
          <NavLink to="/dashboard" className={navLinkClass}>
            Dashboard
          </NavLink>
        </nav>
      </header>

      <main className={styles.main}>
        <Outlet />
      </main>
    </div>
  );
}
