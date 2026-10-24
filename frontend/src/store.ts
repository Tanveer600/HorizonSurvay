import { configureStore } from '@reduxjs/toolkit';
import createSagaMiddleware from 'redux-saga';
import approvalsReducer from './features/approvals/approvalsSlice';
import checkoutsReducer from './features/checkouts/checkoutsSlice';
import categoriesReducer from './features/categories/categoriesSlice';
import equipmentReducer from './features/equipment/equipmentSlice';
import historyReducer from './features/history/historySlice';
import inventoryReducer from './features/inventory/inventorySlice';
import uiReducer from './features/ui/uiSlice';
import rootSaga from './features/rootSaga';

const sagaMiddleware = createSagaMiddleware();

export const store = configureStore({
  reducer: {
    approvals: approvalsReducer,
    checkouts: checkoutsReducer,
    categories: categoriesReducer,
    equipment: equipmentReducer,
    history: historyReducer,
    inventory: inventoryReducer,
    ui: uiReducer,
  },
  middleware: (getDefaultMiddleware) => getDefaultMiddleware({ thunk: false }).concat(sagaMiddleware),
});

sagaMiddleware.run(rootSaga);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
