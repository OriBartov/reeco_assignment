import { ApiClientError } from '../../api/client';
import styles from './ErrorMessage.module.css';

export interface ErrorMessageProps {
  /** Error instance, message string, or unknown value from a query/mutation. */
  error: unknown;
  title?: string;
  onRetry?: () => void;
  retryLabel?: string;
}

function resolveMessage(error: unknown): string {
  if (error instanceof ApiClientError) {
    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  if (typeof error === 'string') {
    return error;
  }

  return 'Something went wrong. Please try again.';
}

export function ErrorMessage({
  error,
  title = 'Unable to load data',
  onRetry,
  retryLabel = 'Retry',
}: ErrorMessageProps) {
  const message = resolveMessage(error);

  return (
    <div className={styles.banner} role="alert">
      <div className={styles.content}>
        <p className={styles.title}>{title}</p>
        <p className={styles.message}>{message}</p>
      </div>
      {onRetry ? (
        <button type="button" className={styles.retryButton} onClick={onRetry}>
          {retryLabel}
        </button>
      ) : null}
    </div>
  );
}
