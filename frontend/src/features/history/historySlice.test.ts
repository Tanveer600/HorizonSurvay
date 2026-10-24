import { describe, expect, it } from 'vitest';
import reducer, {
  deleteMovement,
  historyLoaded,
  historyMutationFailed,
  historyRangeDeleted,
  movementDeleted,
} from './historySlice';
import type { MovementRecord } from '../../api/types';

const records: MovementRecord[] = [
  { id: 'movement-1', checkoutId: 'checkout-1', itemName: 'Rover', type: 'Checkout', quantity: 1, occurredAt: '2026-10-22T10:00:00Z', performedBy: 'Amina', description: null },
  { id: 'movement-2', checkoutId: 'checkout-1', itemName: 'Rover', type: 'Return', quantity: 1, occurredAt: '2026-10-22T11:00:00Z', performedBy: 'Saima', description: null },
];

describe('movement deletion state', () => {
  it('keeps a displayed row after an API failure and removes only a successful target', () => {
    const loaded = reducer(undefined, historyLoaded(records));
    const deleting = reducer(loaded, deleteMovement('movement-1'));
    expect(deleting.records).toEqual(records);
    const failed = reducer(deleting, historyMutationFailed('API unavailable'));
    expect(failed.records).toEqual(records);
    const succeeded = reducer(failed, movementDeleted('movement-1'));
    expect(succeeded.records).toEqual([records[1]]);
  });

  it('clears only the displayed filtered movements after a successful range deletion', () => {
    expect(reducer(reducer(undefined, historyLoaded(records)), historyRangeDeleted()).records).toEqual([]);
  });
});
