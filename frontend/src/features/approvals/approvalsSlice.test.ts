import { describe, expect, it } from 'vitest';
import reducer, { approvalConsumed, approvalRecorded } from './approvalsSlice';

describe('approval slice', () => {
  it('removes a one-use approval after a successful checkout consumes it', () => {
    const approval = {
      id: 'approval-1',
      equipmentAssetId: 'drone-1',
      holder: 'Amina',
      approvedBy: 'Hamza',
      approvedAt: '2026-10-21T10:00:00Z',
    };
    const recorded = reducer(undefined, approvalRecorded(approval));

    expect(recorded.recorded).toEqual([approval]);
    expect(reducer(recorded, approvalConsumed(approval.id)).recorded).toEqual([]);
  });
});
