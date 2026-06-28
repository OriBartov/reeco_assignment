import type { JobStatus } from '../types';
import { apiGet } from './client';

export function getJob(jobId: string) {
  return apiGet<JobStatus>(`/jobs/${encodeURIComponent(jobId)}`);
}
