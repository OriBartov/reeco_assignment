import { useQuery } from '@tanstack/react-query';
import { getJob } from '../api/jobs';
import type { JobStatus, JobStatusValue } from '../types';

export function jobQueryKey(jobId: string) {
  return ['jobs', jobId] as const;
}

const TERMINAL_STATUSES: JobStatusValue[] = ['completed', 'failed'];

export function useBulkJobQuery(jobId: string | null) {
  return useQuery({
    queryKey: jobQueryKey(jobId ?? ''),
    queryFn: () => getJob(jobId!),
    enabled: !!jobId,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      if (status && TERMINAL_STATUSES.includes(status)) {
        return false;
      }
      return 1000;
    },
  });
}

export function isJobTerminal(job: JobStatus | undefined): boolean {
  return !!job?.status && TERMINAL_STATUSES.includes(job.status);
}
