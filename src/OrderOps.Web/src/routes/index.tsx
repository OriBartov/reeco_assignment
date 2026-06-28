import { createBrowserRouter, Navigate } from 'react-router-dom';
import { AppLayout } from '../components/layout/AppLayout';
import { DashboardPage } from '../pages/DashboardPage';
import { OrdersPage } from '../pages/OrdersPage';
import { SupplierDetailPage } from '../pages/SupplierDetailPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <OrdersPage /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'suppliers/:id', element: <SupplierDetailPage /> },
      { path: '*', element: <Navigate to="/" replace /> },
    ],
  },
]);
