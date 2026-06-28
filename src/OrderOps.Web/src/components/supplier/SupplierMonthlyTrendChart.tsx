import { useMemo } from 'react';
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { formatCount, formatMonth } from '../../lib/formatters';
import type { SupplierMonthlyTrend } from '../../types';
import { ChartCard } from '../dashboard/ChartCard';
import styles from './SupplierMonthlyTrendChart.module.css';

export interface SupplierMonthlyTrendChartProps {
  monthlyTrend: SupplierMonthlyTrend[] | undefined;
  isLoading?: boolean;
}

interface MonthDatum extends SupplierMonthlyTrend {
  label: string;
}

function TrendTooltip({
  active,
  payload,
}: {
  active?: boolean;
  payload?: Array<{ payload: MonthDatum }>;
}) {
  if (!active || !payload?.length) {
    return null;
  }

  const item = payload[0].payload;

  return (
    <div className={styles.tooltip}>
      <p className={styles.tooltipTitle}>{item.label}</p>
      <p className={styles.tooltipRow}>Orders: {formatCount(item.order_count)}</p>
    </div>
  );
}

export function SupplierMonthlyTrendChart({
  monthlyTrend,
  isLoading,
}: SupplierMonthlyTrendChartProps) {
  const data = useMemo<MonthDatum[]>(() => {
    if (!monthlyTrend?.length) {
      return [];
    }

    return monthlyTrend.map((entry) => ({
      ...entry,
      label: formatMonth(entry.month),
    }));
  }, [monthlyTrend]);

  return (
    <ChartCard
      title="Order trend"
      description="Monthly order volume for this supplier"
      isLoading={isLoading}
    >
      {data.length === 0 ? (
        <p className={styles.empty}>No order history to chart yet.</p>
      ) : (
        <ResponsiveContainer width="100%" height={280}>
          <LineChart data={data} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" vertical={false} />
            <XAxis
              dataKey="label"
              tick={{ fill: 'var(--color-text-muted)', fontSize: 12 }}
              axisLine={{ stroke: 'var(--color-border)' }}
              tickLine={false}
              interval="preserveStartEnd"
            />
            <YAxis
              allowDecimals={false}
              tick={{ fill: 'var(--color-text-muted)', fontSize: 12 }}
              axisLine={false}
              tickLine={false}
            />
            <Tooltip content={<TrendTooltip />} />
            <Line
              type="monotone"
              dataKey="order_count"
              name="Orders"
              stroke="var(--color-accent)"
              strokeWidth={2}
              dot={{ r: 3, fill: 'var(--color-accent)' }}
              activeDot={{ r: 5 }}
            />
          </LineChart>
        </ResponsiveContainer>
      )}
    </ChartCard>
  );
}
