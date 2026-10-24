import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { InventorySnapshot } from '../../api/types';

export type RequestStatus = 'idle' | 'loading' | 'succeeded' | 'failed';

interface InventoryState {
  data: InventorySnapshot | null;
  status: RequestStatus;
  error: string | null;
}

const initialState: InventoryState = { data: null, status: 'loading', error: null };

const inventorySlice = createSlice({
  name: 'inventory',
  initialState,
  reducers: {
    loadInventory: (state) => { state.status = 'loading'; state.error = null; },
    inventoryLoaded: (state, action: PayloadAction<InventorySnapshot>) => {
      state.data = action.payload;
      state.status = 'succeeded';
    },
    inventoryFailed: (state, action: PayloadAction<string>) => {
      state.status = 'failed';
      state.error = action.payload;
    },
  },
});

export const { loadInventory, inventoryLoaded, inventoryFailed } = inventorySlice.actions;
export default inventorySlice.reducer;
