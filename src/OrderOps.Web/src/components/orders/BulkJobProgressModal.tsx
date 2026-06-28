import type { JobProgress, JobStatusValue } from '../../types';
import styles from './BulkJobProgressModal.module.css';

export interface BulkJobProgressModalProps {
  open: boolean;
  status?: JobStatusValue;
  progress?: JobProgress;
  errorMessage?: string | null;
  onDismiss: () => void;
}

function progressPercent(progress: JobProgress | undefined): number {
  if (!progress || progress.total <= 0) {
    return 0;
  }
  const processed = progress.completed + progress.failed;
  return Math.min(100, Math.round((processed / progress.total) * 100));
}

export function BulkJobProgressModal({
  open,
  status,
  progress,
  errorMessage,
  onDismiss,
}: BulkJobProgressModalProps) {
  if (!open) {
    return null;
  }

  const isComplete = status === 'completed' || status === 'failed';
  const percent = progressPercent(progress);

  return (
    <div className={styles.overlay} role="presentation">
      <div
        className={styles.dialog}
        role="dialog"
        aria-modal="true"
        aria-labelledby="bulk-job-title"
        aria-busy={!isComplete}
      >
        <h2 id="bulk-job-title" className={styles.title}>
          {isComplete ? 'Bulk action finished' : 'Processing bulk action…'}
        </h2>
        <p className={styles.description}>
          {isComplete
            ? 'Review the summary below. Orders on this page will refresh automatically.'
            : 'Orders are being updated in the background. You can keep browsing while this runs.'}
        </p>

        <div
          className={styles.progressTrack}
          role="progressbar"
          aria-valuemin={0}
          aria-valuemax={100}
          aria-valuenow={percent}
          aria-label="Bulk job progress"
        >
          <div className={styles.progressFill} style={{ width: `${percent}%` }} />
        </div>

        {progress ? (
          <div className={styles.stats}>
            <span>
              Total: <span className={styles.statValue}>{progress.total.toLocaleString()}</span>
            </span>
            <span>
              Completed:{' '}
              <span className={styles.statValue}>{progress.completed.toLocaleString()}</span>
            </span>
            <span>
              Failed:{' '}
              <span className={[styles.statValue, progress.failed > 0 ? styles.failed : ''].join(' ')}>
                {progress.failed.toLocaleString()}
              </span>
            </span>
          </div>
        ) : null}

        {isComplete && progress ? (
          <div className={styles.summary}>
            {progress.failed > 0 ? (
              <>
                {progress.completed.toLocaleString()} order
                {progress.completed === 1 ? '' : 's'} updated successfully.{' '}
                {progress.failed.toLocaleString()} could not be processed — typically cancelled
                orders, missing IDs, or concurrent updates.
              </>
            ) : (
              <>
                All {progress.completed.toLocaleString()} order
                {progress.completed === 1 ? '' : 's'} were processed successfully.
              </>
            )}
            {status === 'failed' ? ' The job ended in a failed state.' : null}
          </div>
        ) : null}

        {errorMessage ? (
          <p className={styles.error} role="alert">
            {errorMessage}
          </p>
        ) : null}

        {isComplete ? (
          <div className={styles.footer}>
            <button type="button" className={styles.button} onClick={onDismiss}>
              Done
            </button>
          </div>
        ) : null}
      </div>
    </div>
  );
}
