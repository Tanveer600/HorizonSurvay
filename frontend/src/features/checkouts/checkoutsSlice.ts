import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { CheckoutReceipt, CheckoutRequest, OpenCheckout, ReturnRequest } from '../../api/types';
import type { RequestStatus } from '../inventory/inventorySlice';

interface CheckoutsState {
  open: OpenCheckout[];
  status: RequestStatus;
  error: string | null;
  mutationStatus: RequestStatus;
  mutationError: string | null;
  returnStatus: RequestStatus;
  receipt: CheckoutReceipt | null;
}

const initialState: CheckoutsState = {
  open: [],   status: 'loading', error: null, mutationStatus: 'idle', mutationError: null, returnStatus: 'idle', receipt: null,
};

const checkoutsSlice = createSlice({
  name: 'checkouts',
  initialState,
  reducers: {
    loadOpenCheckouts: (state) => { state.status = 'loading'; state.error = null; },
    openCheckoutsLoaded: (state, action: PayloadAction<OpenCheckout[]>) => {
      state.open = action.payload;
      state.status = 'succeeded';
    },
    openCheckoutsFailed: (state, action: PayloadAction<string>) => {
      state.status = 'failed';
      state.error = action.payload;
    },
    submitCheckout: (state, action: PayloadAction<CheckoutRequest>) => {
      state.mutationStatus = 'loading';
      state.mutationError = null;
      state.receipt = null;
      void action.payload;
    },
    checkoutSucceeded: (state, action: PayloadAction<CheckoutReceipt>) => {
      state.mutationStatus = 'succeeded';
      state.receipt = action.payload;
    },
    submitReturn: (state, action: PayloadAction<{ id: string; request: ReturnRequest }>) => {
      state.returnStatus = 'loading';
      state.mutationError = null;
      void action.payload;
    },
    returnSucceeded: (state) => { state.returnStatus = 'succeeded'; },
    returnFailed: (state, action: PayloadAction<string>) => {
      state.returnStatus = 'failed';
      state.mutationError = action.payload;
    },
    checkoutMutationFailed: (state, action: PayloadAction<string>) => {
      state.mutationStatus = 'failed';
      state.mutationError = action.payload;
    },
    clearCheckoutReceipt: (state) => {
      state.receipt = null;
      state.mutationStatus = 'idle';
      state.mutationError = null;
    },
    clearReturnStatus: (state) => {
      state.returnStatus = 'idle';
      state.mutationError = null;
    },
  },
});

export const {
  loadOpenCheckouts,
  openCheckoutsLoaded,
  openCheckoutsFailed,
  submitCheckout,
  checkoutSucceeded,
  submitReturn,
  returnSucceeded,
  returnFailed,
  checkoutMutationFailed,
  clearCheckoutReceipt,
  clearReturnStatus,
} = checkoutsSlice.actions;
export default checkoutsSlice.reducer;
