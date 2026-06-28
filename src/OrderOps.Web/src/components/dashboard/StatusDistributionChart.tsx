import { useMemo } from 'react';
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
import { statusColor } from '../../lib/chart-colors';
import { formatCount, formatCurrency, formatStatusLabel } from '../../lib/formatters';
import type { OrderStats } from '../../types';
import { ChartCard } from './ChartCard';
import styles from './StatusDistributionChart.module.css';

export interface StatusDistributionChartProps {
  byStatus: OrderStats['by_status'] | undefined;
  isLoading?: boolean;
}

interface StatusDatum {
  status: string;
  label: string;
  count: number;
  total_value: number;
  fill: string;
}

function StatusTooltip({
  active,
  payload,
}: {
  active?: boolean;
  payload?: Array<{ payload: StatusDatum }>;
}) {
  if (!active || !payload?.length) {
    return null;
  }

  const item = payload[0].payload;

  return (
    <div className={styles.tooltip}>
      <p className={styles.tooltipTitle}>{item.label}</p>
      <p className={styles.tooltipRow}>Orders: {formatCount(item.count)}</p>
      <p className={styles.tooltipRow}>Value: {formatCurrency(item.total_value)}</p>
    </div>
  );
}

export function StatusDistributionChart({ byStatus, isLoading }: StatusDistributionChartProps) {
  const data = useMemo<StatusDatum[]>(() => {
    if (!byStatus) {
      return [];
    }

    return Object.entries(byStatus)
      .map(([status, aggregate], index) => ({
        status,
        label: formatStatusLabel(status),
        count: aggregate.count,
        total_value: aggregate.total_value,
        fill: statusColor(status, index),
      }))
      .sort((a, b) => b.count - a.count);
  }, [byStatus]);

  return (
    <ChartCard
      title="Orders by status"
      description="Distribution of order counts across workflow states"
      isLoading={isLoading}
    >
      {data.length === 0 ? (
        <p className={styles.empty}>No status data available.</p>
      ) : (
        <ResponsiveContainer width="100%" height={280}>
          <BarChart data={data} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" vertical={false} />
            <XAxis
              dataKey="label"
              tick={{ fill: 'var(--color-text-muted)', fontSize: 12 }}
              axisLine={{ stroke: 'var(--color-border)' }}
              tickLine={false}
            />
            <YAxis
              allowDecimals={false}
              tick={{ fill: 'var(--color-text-muted)', fontSize: 12 }}
              axisLine={false}
              tickLine={false}
            />
            <Tooltip content={<StatusTooltip />} cursor={{ fill: 'rgb(37 99 235 / 6%)' }} />
            <Bar dataKey="count" name="Orders" radius={[4, 4, 0, 0]}>
              {data.map((entry) => (
                <Cell key={entry.status} fill={entry.fill} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      )}
    </ChartCard>
  );
}
