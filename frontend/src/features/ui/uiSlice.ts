import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

interface Toast {
  id: number;
  message: string;
  tone: 'success' | 'error';
}

interface UiState {
  toast: Toast | null;
}

const initialState: UiState = { toast: null };

const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    showToast: (state, action: PayloadAction<Omit<Toast, 'id'>>) => {
      state.toast = { ...action.payload, id: Date.now() };
    },
    clearToast: (state) => { state.toast = null; },
  },
});

export const { showToast, clearToast } = uiSlice.actions;
export default uiSlice.reducer;
