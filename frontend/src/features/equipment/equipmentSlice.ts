import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { RegisterEquipmentRequest } from '../../api/types';
import type { RequestStatus } from '../inventory/inventorySlice';

interface EquipmentState {
  status: RequestStatus;
  error: string | null;
  registeredName: string | null;
}

const initialState: EquipmentState = { status: 'idle', error: null, registeredName: null };

const equipmentSlice = createSlice({
  name: 'equipment',
  initialState,
  reducers: {
    registerEquipment: (state, action: PayloadAction<RegisterEquipmentRequest>) => {
      state.status = 'loading';
      state.error = null;
      state.registeredName = null;
      void action.payload;
    },
    equipmentRegistered: (state, action: PayloadAction<string>) => {
      state.status = 'succeeded';
      state.registeredName = action.payload;
    },
    equipmentRegistrationFailed: (state, action: PayloadAction<string>) => {
      state.status = 'failed';
      state.error = action.payload;
    },
  },
});

export const { registerEquipment, equipmentRegistered, equipmentRegistrationFailed } = equipmentSlice.actions;
export default equipmentSlice.reducer;
