export interface StatusAggregate {
  count: number;
  total_value: number;
}

export interface MonthAggregate {
  month: string;
  order_count: number;
  revenue: number;
}

export interface TopSupplier {
  supplier_id: string;
  supplier_name: string;
  total_revenue: number;
}

export interface WarehouseAggregate {
  warehouse: string;
  count: number;
  total_value: number;
}

export interface OrderStats {
  total_orders: number;
  total_revenue: number;
  avg_order_value: number;
  by_status: Record<string, StatusAggregate>;
  by_month: MonthAggregate[];
  top_suppliers: TopSupplier[];
  by_warehouse: WarehouseAggregate[];
}
