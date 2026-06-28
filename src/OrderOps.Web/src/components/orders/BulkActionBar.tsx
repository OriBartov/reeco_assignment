import type { BulkActionType } from '../../types';
import styles from './BulkActionBar.module.css';

const BULK_ACTIONS: BulkActionType[] = ['approve', 'reject', 'flag'];

export interface BulkActionBarProps {
  selectedCount: number;
  onAction: (action: BulkActionType) => void;
  onClear: () => void;
  disabled?: boolean;
}

export function BulkActionBar({
  selectedCount,
  onAction,
  onClear,
  disabled,
}: BulkActionBarProps) {
  if (selectedCount === 0) {
    return null;
  }

  return (
    <div className={styles.bar} role="region" aria-label="Bulk actions">
      <p className={styles.summary}>
        {selectedCount.toLocaleString()} order{selectedCount === 1 ? '' : 's'} selected
      </p>
      <div className={styles.actions}>
        {BULK_ACTIONS.map((action) => (
          <button
            key={action}
            type="button"
            className={[
              styles.actionButton,
              action === 'approve' ? styles.actionButtonPrimary : '',
              action === 'reject' ? styles.actionButtonDanger : '',
            ].join(' ')}
            disabled={disabled}
            onClick={() => onAction(action)}
          >
            {action}
          </button>
        ))}
        <button type="button" className={styles.clearButton} disabled={disabled} onClick={onClear}>
          Clear selection
        </button>
      </div>
    </div>
  );
}
