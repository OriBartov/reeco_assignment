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
import { formatCount, formatCurrency, formatMonth } from '../../lib/formatters';
import type { MonthAggregate } from '../../types';
import { ChartCard } from './ChartCard';
import styles from './MonthlyTrendChart.module.css';

export interface MonthlyTrendChartProps {
  byMonth: MonthAggregate[] | undefined;
  isLoading?: boolean;
}

interface MonthDatum extends MonthAggregate {
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
      <p className={styles.tooltipRow}>Revenue: {formatCurrency(item.revenue)}</p>
    </div>
  );
}

export function MonthlyTrendChart({ byMonth, isLoading }: MonthlyTrendChartProps) {
  const data = useMemo<MonthDatum[]>(() => {
    if (!byMonth?.length) {
      return [];
    }

    return byMonth.map((entry) => ({
      ...entry,
      label: formatMonth(entry.month),
    }));
  }, [byMonth]);

  return (
    <ChartCard
      title="Monthly volume"
      description="Order count trend over time"
      isLoading={isLoading}
    >
      {data.length === 0 ? (
        <p className={styles.empty}>No monthly data available.</p>
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
