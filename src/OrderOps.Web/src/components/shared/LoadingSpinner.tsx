import styles from './LoadingSpinner.module.css';

export interface LoadingSpinnerProps {
  /** Optional status message shown below the spinner. */
  message?: string;
  size?: 'sm' | 'md' | 'lg';
  /** Accessible label for screen readers. Defaults to message or "Loading". */
  label?: string;
}

const sizeClass: Record<NonNullable<LoadingSpinnerProps['size']>, string> = {
  sm: styles.spinnerSm,
  md: '',
  lg: styles.spinnerLg,
};

export function LoadingSpinner({
  message,
  size = 'md',
  label,
}: LoadingSpinnerProps) {
  const ariaLabel = label ?? message ?? 'Loading';

  return (
    <div
      className={styles.container}
      role="status"
      aria-live="polite"
      aria-label={ariaLabel}
    >
      <div
        className={[styles.spinner, sizeClass[size]].filter(Boolean).join(' ')}
        aria-hidden="true"
      />
      {message ? <p className={styles.message}>{message}</p> : null}
    </div>
  );
}
