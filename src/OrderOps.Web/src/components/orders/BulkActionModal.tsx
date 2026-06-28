import { useEffect, useState } from 'react';
import type { BulkActionType } from '../../types';
import styles from './BulkActionModal.module.css';

export interface BulkActionModalProps {
  open: boolean;
  action: BulkActionType;
  selectedCount: number;
  isSubmitting?: boolean;
  errorMessage?: string | null;
  onConfirm: (reason: string) => void;
  onCancel: () => void;
}

function actionLabel(action: BulkActionType): string {
  return action.charAt(0).toUpperCase() + action.slice(1);
}

export function BulkActionModal({
  open,
  action,
  selectedCount,
  isSubmitting,
  errorMessage,
  onConfirm,
  onCancel,
}: BulkActionModalProps) {
  const [reason, setReason] = useState('');

  useEffect(() => {
    if (open) {
      setReason('');
    }
  }, [open, action]);

  if (!open) {
    return null;
  }

  const reasonRequired = action === 'reject' || action === 'flag';
  const canSubmit = !reasonRequired || reason.trim().length > 0;

  return (
    <div className={styles.overlay} role="presentation" onClick={onCancel}>
      <div
        className={styles.dialog}
        role="dialog"
        aria-modal="true"
        aria-labelledby="bulk-action-title"
        onClick={(event) => event.stopPropagation()}
      >
        <h2 id="bulk-action-title" className={styles.title}>
          Confirm bulk {action}
        </h2>
        <p className={styles.description}>
          {actionLabel(action)} {selectedCount.toLocaleString()} selected order
          {selectedCount === 1 ? '' : 's'}. This runs in the background and may take a moment
          for large batches.
        </p>

        <div className={styles.field}>
          <label className={styles.label} htmlFor="bulk-reason">
            Reason{reasonRequired ? '' : ' (optional)'}
          </label>
          <textarea
            id="bulk-reason"
            className={styles.textarea}
            value={reason}
            placeholder={
              action === 'flag'
                ? 'Describe why these orders are flagged…'
                : action === 'reject'
                  ? 'Explain the rejection…'
                  : 'Optional note for the approval…'
            }
            onChange={(event) => setReason(event.target.value)}
          />
        </div>

        {errorMessage ? (
          <p className={styles.error} role="alert">
            {errorMessage}
          </p>
        ) : null}

        <div className={styles.footer}>
          <button type="button" className={styles.button} disabled={isSubmitting} onClick={onCancel}>
            Cancel
          </button>
          <button
            type="button"
            className={[styles.button, styles.buttonPrimary].join(' ')}
            disabled={isSubmitting || !canSubmit}
            onClick={() => onConfirm(reason.trim())}
          >
            {isSubmitting ? 'Starting…' : `Confirm ${action}`}
          </button>
        </div>
      </div>
    </div>
  );
}
