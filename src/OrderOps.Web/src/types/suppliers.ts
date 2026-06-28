export interface SupplierListItem {
  id: string;
  name: string;
  email: string;
  rating: number | null;
  country: string;
  active: boolean;
  created_at: string;
}

export interface SupplierDetail extends SupplierListItem {
  order_count: number;
  total_revenue: number;
}

export interface SupplierMonthlyTrend {
  month: string;
  order_count: number;
}

export interface SupplierPerformance {
  avg_delivery_days: number;
  rejection_rate: number;
  avg_order_value: number;
  monthly_trend: SupplierMonthlyTrend[];
  price_consistency: number;
}

export interface SupplierListQuery {
  limit?: number;
  offset?: number;
}
