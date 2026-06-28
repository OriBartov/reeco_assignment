export type JobStatusValue = 'processing' | 'completed' | 'failed';

export interface JobProgress {
  total: number;
  completed: number;
  failed: number;
}

export interface JobStatus {
  status: JobStatusValue;
  progress: JobProgress;
}
