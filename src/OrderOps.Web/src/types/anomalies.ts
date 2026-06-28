export type AnomalySeverity = 'low' | 'medium' | 'high';

export interface OrderAnomaly {
  order_id: string;
  anomaly_types: string[];
  severity: AnomalySeverity;
}

export interface AnomaliesListResponse {
  data: OrderAnomaly[];
}
