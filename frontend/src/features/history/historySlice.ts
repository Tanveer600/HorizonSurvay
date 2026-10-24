import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { MovementRecord } from '../../api/types';
import type { RequestStatus } from '../inventory/inventorySlice';

interface HistoryQuery {
  from: string;
  to: string;
}

interface HistoryState {
  records: MovementRecord[];
  status: RequestStatus;
  error: string | null;
  query: HistoryQuery | null;
  mutationStatus: RequestStatus;
  mutationError: string | null;
}

const initialState: HistoryState = {
  records: [],
  status: 'idle',
  error: null,
  query: null,
  mutationStatus: 'idle',
  mutationError: null,
};

const historySlice = createSlice({
  name: 'history',
  initialState,
  reducers: {
    loadHistory: (state, action: PayloadAction<HistoryQuery>) => {
      state.status = 'loading';
      state.error = null;
      state.query = action.payload;
    },
    historyLoaded: (state, action: PayloadAction<MovementRecord[]>) => {
      state.records = action.payload;
      state.status = 'succeeded';
    },
    historyFailed: (state, action: PayloadAction<string>) => {
      state.status = 'failed';
      state.error = action.payload;
    },
    deleteMovement: (state, action: PayloadAction<string>) => {
      state.mutationStatus = 'loading';
      state.mutationError = null;
      void action.payload;
    },
    deleteHistoryRange: (state, action: PayloadAction<HistoryQuery>) => {
      state.mutationStatus = 'loading';
      state.mutationError = null;
      void action.payload;
    },
    movementDeleted: (state, action: PayloadAction<string>) => {
      state.records = state.records.filter((record) => record.id !== action.payload);
      state.mutationStatus = 'succeeded';
    },
    historyRangeDeleted: (state) => {
      state.records = [];
      state.mutationStatus = 'succeeded';
    },
    historyMutationFailed: (state, action: PayloadAction<string>) => {
      state.mutationStatus = 'failed';
      state.mutationError = action.payload;
    },
  },
});

export const {
  loadHistory,
  historyLoaded,
  historyFailed,
  deleteMovement,
  deleteHistoryRange,
  movementDeleted,
  historyRangeDeleted,
  historyMutationFailed,
} = historySlice.actions;
export default historySlice.reducer;
