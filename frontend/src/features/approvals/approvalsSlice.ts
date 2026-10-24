import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { ApprovalReceipt, ApprovalRequest } from '../../api/types';
import type { RequestStatus } from '../inventory/inventorySlice';

interface ApprovalsState {
  recorded: ApprovalReceipt[];
  status: RequestStatus;
  error: string | null;
}

const initialState: ApprovalsState = { recorded: [], status: 'idle', error: null };

const approvalsSlice = createSlice({
  name: 'approvals',
  initialState,
  reducers: {
    recordApproval: (state, action: PayloadAction<ApprovalRequest>) => {
      state.status = 'loading';
      state.error = null;
      void action.payload;
    },
    approvalRecorded: (state, action: PayloadAction<ApprovalReceipt>) => {
      state.recorded.unshift(action.payload);
      state.status = 'succeeded';
    },
    approvalConsumed: (state, action: PayloadAction<string>) => {
      state.recorded = state.recorded.filter((approval) => approval.id !== action.payload);
    },
    approvalFailed: (state, action: PayloadAction<string>) => {
      state.status = 'failed';
      state.error = action.payload;
    },
  },
});

export const { recordApproval, approvalRecorded, approvalConsumed, approvalFailed } = approvalsSlice.actions;
export default approvalsSlice.reducer;
