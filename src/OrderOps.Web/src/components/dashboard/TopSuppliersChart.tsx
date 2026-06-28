import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { CHART_COLORS } from '../../lib/chart-colors';
import { formatCurrency } from '../../lib/formatters';
import type { TopSupplier } from '../../types';
import { ChartCard } from './ChartCard';
import styles from './TopSuppliersChart.module.css';

export interface TopSuppliersChartProps {
  topSuppliers: TopSupplier[] | undefined;
  isLoading?: boolean;
}

interface SupplierDatum extends TopSupplier {
  label: string;
  fill: string;
}

function SupplierTooltip({
  active,
  payload,
}: {
  active?: boolean;
  payload?: Array<{ payload: SupplierDatum }>;
}) {
  if (!active || !payload?.length) {
    return null;
  }

  const item = payload[0].payload;

  return (
    <div className={styles.tooltip}>
      <p className={styles.tooltipTitle}>{item.supplier_name}</p>
      <p className={styles.tooltipRow}>Revenue: {formatCurrency(item.total_revenue)}</p>
      <p className={styles.tooltipHint}>Click bar to view supplier</p>
    </div>
  );
}

export function TopSuppliersChart({ topSuppliers, isLoading }: TopSuppliersChartProps) {
  const navigate = useNavigate();

  const data = useMemo<SupplierDatum[]>(() => {
    if (!topSuppliers?.length) {
      return [];
    }

    return topSuppliers.map((supplier, index) => ({
      ...supplier,
      label: supplier.supplier_name,
      fill: CHART_COLORS[index % CHART_COLORS.length],
    }));
  }, [topSuppliers]);

  const chartHeight = Math.max(280, data.length * 36);

  return (
    <ChartCard
      title="Top suppliers"
      description="Highest revenue suppliers — click a bar to open details"
      isLoading={isLoading}
    >
      {data.length === 0 ? (
        <p className={styles.empty}>No supplier data available.</p>
      ) : (
        <ResponsiveContainer width="100%" height={chartHeight}>
          <BarChart
            data={data}
            layout="vertical"
            margin={{ top: 8, right: 16, left: 8, bottom: 0 }}
          >
            <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" horizontal={false} />
            <XAxis
              type="number"
              tick={{ fill: 'var(--color-text-muted)', fontSize: 12 }}
              axisLine={{ stroke: 'var(--color-border)' }}
              tickLine={false}
              tickFormatter={(value: number) => formatCurrency(value)}
            />
            <YAxis
              type="category"
              dataKey="label"
              width={140}
              tick={{ fill: 'var(--color-text-muted)', fontSize: 12 }}
              axisLine={false}
              tickLine={false}
            />
            <Tooltip content={<SupplierTooltip />} cursor={{ fill: 'rgb(37 99 235 / 6%)' }} />
            <Bar
              dataKey="total_revenue"
              name="Revenue"
              radius={[0, 4, 4, 0]}
              cursor="pointer"
              onClick={(_bar, index) => {
                const supplier = data[index];
                if (supplier?.supplier_id) {
                  navigate(`/suppliers/${encodeURIComponent(supplier.supplier_id)}`);
                }
              }}
            >
              {data.map((entry) => (
                <Cell key={entry.supplier_id} fill={entry.fill} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      )}
    </ChartCard>
  );
}
