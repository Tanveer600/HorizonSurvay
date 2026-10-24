import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { EquipmentCategory } from '../../api/types';
import type { RequestStatus } from '../inventory/inventorySlice';

interface CategoriesState {
  categories: EquipmentCategory[];
  status: RequestStatus;
  mutationStatus: RequestStatus;
  error: string | null;
  mutationError: string | null;
}

const initialState: CategoriesState = {
  categories: [],
  status: 'idle',
  mutationStatus: 'idle',
  error: null,
  mutationError: null,
};

const categoriesSlice = createSlice({
  name: 'categories',
  initialState,
  reducers: {
    loadCategories: (state) => {
      state.status = 'loading';
      state.error = null;
    },
    categoriesLoaded: (state, action: PayloadAction<EquipmentCategory[]>) => {
      state.categories = action.payload;
      state.status = 'succeeded';
    },
    categoriesFailed: (state, action: PayloadAction<string>) => {
      state.status = 'failed';
      state.error = action.payload;
    },
    createCategory: (state, action: PayloadAction<string>) => {
      state.mutationStatus = 'loading';
      state.mutationError = null;
      void action.payload;
    },
    updateCategory: (state, action: PayloadAction<{ id: string; name: string }>) => {
      state.mutationStatus = 'loading';
      state.mutationError = null;
      void action.payload;
    },
    deleteCategory: (state, action: PayloadAction<string>) => {
      state.mutationStatus = 'loading';
      state.mutationError = null;
      void action.payload;
    },
    categoryMutationSucceeded: (state) => {
      state.mutationStatus = 'succeeded';
    },
    categoryMutationFailed: (state, action: PayloadAction<string>) => {
      state.mutationStatus = 'failed';
      state.mutationError = action.payload;
    },
  },
});

export const {
  loadCategories,
  categoriesLoaded,
  categoriesFailed,
  createCategory,
  updateCategory,
  deleteCategory,
  categoryMutationSucceeded,
  categoryMutationFailed,
} = categoriesSlice.actions;
export default categoriesSlice.reducer;
