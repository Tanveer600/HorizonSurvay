import { all, call, put, select, takeLatest } from 'redux-saga/effects';
import { approvalApi, categoryApi, checkoutApi, getApiErrorMessage, historyApi, inventoryApi } from '../api/client';
import type { ApprovalRequest, CheckoutRequest, RegisterEquipmentRequest, ReturnRequest } from '../api/types';
import { approvalConsumed, approvalFailed, approvalRecorded, recordApproval } from './approvals/approvalsSlice';
import {
  checkoutMutationFailed,
  checkoutSucceeded,
  loadOpenCheckouts,
  openCheckoutsFailed,
  openCheckoutsLoaded,
  returnFailed,
  returnSucceeded,
  submitCheckout,
  submitReturn,
} from './checkouts/checkoutsSlice';
import {
  categoriesFailed,
  categoriesLoaded,
  categoryMutationFailed,
  categoryMutationSucceeded,
  createCategory,
  deleteCategory,
  loadCategories,
  updateCategory,
} from './categories/categoriesSlice';
import {
  equipmentRegistered,
  equipmentRegistrationFailed,
  registerEquipment,
} from './equipment/equipmentSlice';
import {
  deleteHistoryRange,
  deleteMovement,
  historyFailed,
  historyLoaded,
  historyMutationFailed,
  historyRangeDeleted,
  loadHistory,
  movementDeleted,
} from './history/historySlice';
import { inventoryFailed, inventoryLoaded, loadInventory } from './inventory/inventorySlice';
import { showToast } from './ui/uiSlice';
import type { RootState } from '../store';

function* refreshInventoryViews() {
  yield put(loadInventory());
  yield put(loadOpenCheckouts());
  const query: RootState['history']['query'] = yield select((state: RootState) => state.history.query);
  if (query) yield put(loadHistory(query));
}

export function* fetchInventory() {
  try {
    const inventory: ReturnType<typeof inventoryApi.getInventory> extends Promise<infer T> ? T : never =
      yield call(inventoryApi.getInventory);
    yield put(inventoryLoaded(inventory));
  } catch (error) {
    yield put(inventoryFailed(getApiErrorMessage(error)));
  }
}

export function* fetchCategories() {
  try {
    const categories: ReturnType<typeof categoryApi.getAll> extends Promise<infer T> ? T : never =
      yield call(categoryApi.getAll);
    yield put(categoriesLoaded(categories));
  } catch (error) {
    yield put(categoriesFailed(getApiErrorMessage(error)));
  }
}

function* mutateCategory(action: ReturnType<typeof createCategory> | ReturnType<typeof updateCategory> | ReturnType<typeof deleteCategory>) {
  try {
    if (createCategory.match(action)) {
      yield call(categoryApi.create, action.payload);
      yield put(showToast({ tone: 'success', message: 'Equipment category created.' }));
    } else if (updateCategory.match(action)) {
      yield call(categoryApi.update, action.payload.id, action.payload.name);
      yield put(showToast({ tone: 'success', message: 'Equipment category updated.' }));
    } else {
      yield call(categoryApi.delete, action.payload);
      yield put(showToast({ tone: 'success', message: 'Equipment category deleted.' }));
      yield put(loadInventory());
    }
    yield put(categoryMutationSucceeded());
    yield put(loadCategories());
  } catch (error) {
    const message = getApiErrorMessage(error);
    yield put(categoryMutationFailed(message));
    yield put(showToast({ tone: 'error', message }));
  }
}

function* fetchOpenCheckouts() {
  try {
    const checkouts: ReturnType<typeof checkoutApi.getOpenCheckouts> extends Promise<infer T> ? T : never =
      yield call(checkoutApi.getOpenCheckouts);
    yield put(openCheckoutsLoaded(checkouts));
  } catch (error) {
    yield put(openCheckoutsFailed(getApiErrorMessage(error)));
  }
}

export function* createCheckout(action: ReturnType<typeof submitCheckout>) {
  try {
    const receipt: ReturnType<typeof checkoutApi.checkout> extends Promise<infer T> ? T : never =
      yield call(checkoutApi.checkout, action.payload as CheckoutRequest);
    yield put(checkoutSucceeded(receipt));
    if (action.payload.approvalId) yield put(approvalConsumed(action.payload.approvalId));
    yield put(showToast({ tone: 'success', message: `Checkout recorded for ${receipt.holder}.` }));
    yield* refreshInventoryViews();
  } catch (error) {
    const message = getApiErrorMessage(error);
    yield put(checkoutMutationFailed(message));
    yield put(showToast({ tone: 'error', message }));
  }
}

export function* returnCheckout(action: ReturnType<typeof submitReturn>) {
  try {
    const { id, request } = action.payload as { id: string; request: ReturnRequest };
    yield call(checkoutApi.returnItems, id, request);
    yield put(returnSucceeded());
    yield put(showToast({ tone: 'success', message: 'Return recorded and inventory refreshed.' }));
    yield* refreshInventoryViews();
  } catch (error) {
    const message = getApiErrorMessage(error);
    yield put(returnFailed(message));
    yield put(showToast({ tone: 'error', message }));
  }
}

export function* createApproval(action: ReturnType<typeof recordApproval>) {
  try {
    const approval: ReturnType<typeof approvalApi.record> extends Promise<infer T> ? T : never =
      yield call(approvalApi.record, action.payload as ApprovalRequest);
    yield put(approvalRecorded(approval));
    yield put(showToast({ tone: 'success', message: `Drone approval recorded for ${approval.holder}.` }));
  } catch (error) {
    const message = getApiErrorMessage(error);
    yield put(approvalFailed(message));
    yield put(showToast({ tone: 'error', message }));
  }
}

export function* createEquipment(action: ReturnType<typeof registerEquipment>) {
  try {
    const request = action.payload as RegisterEquipmentRequest;
    yield call(inventoryApi.registerEquipment, request);
    yield put(equipmentRegistered(request.name));
    yield put(showToast({ tone: 'success', message: `${request.name} added to inventory.` }));
    yield put(loadInventory());
  } catch (error) {
    const message = getApiErrorMessage(error);
    yield put(equipmentRegistrationFailed(message));
    yield put(showToast({ tone: 'error', message }));
  }
}

export function* fetchHistory(action: ReturnType<typeof loadHistory>) {
  try {
    const { from, to } = action.payload;
    const records: ReturnType<typeof historyApi.get> extends Promise<infer T> ? T : never =
      yield call(historyApi.get, from, to);
    yield put(historyLoaded(records));
  } catch (error) {
    yield put(historyFailed(getApiErrorMessage(error)));
  }
}

export function* removeMovement(action: ReturnType<typeof deleteMovement>) {
  try {
    yield call(historyApi.deleteOne, action.payload);
    yield put(movementDeleted(action.payload));
    yield put(showToast({ tone: 'success', message: 'Movement record deleted.' }));
  } catch (error) {
    const message = getApiErrorMessage(error);
    yield put(historyMutationFailed(message));
    yield put(showToast({ tone: 'error', message }));
  }
}

function* removeHistoryRange(action: ReturnType<typeof deleteHistoryRange>) {
  try {
    const { from, to } = action.payload;
    const deletedCount: number = yield call(historyApi.deleteRange, from, to);
    yield put(historyRangeDeleted());
    yield put(showToast({ tone: 'success', message: `${deletedCount} movement record${deletedCount === 1 ? '' : 's'} deleted.` }));
  } catch (error) {
    const message = getApiErrorMessage(error);
    yield put(historyMutationFailed(message));
    yield put(showToast({ tone: 'error', message }));
  }
}

export default function* rootSaga() {
  yield all([
    takeLatest(loadInventory.type, fetchInventory),
    takeLatest(loadCategories.type, fetchCategories),
    takeLatest([createCategory.type, updateCategory.type, deleteCategory.type], mutateCategory),
    takeLatest(loadOpenCheckouts.type, fetchOpenCheckouts),
    takeLatest(submitCheckout.type, createCheckout),
    takeLatest(submitReturn.type, returnCheckout),
    takeLatest(recordApproval.type, createApproval),
    takeLatest(registerEquipment.type, createEquipment),
    takeLatest(loadHistory.type, fetchHistory),
    takeLatest(deleteMovement.type, removeMovement),
    takeLatest(deleteHistoryRange.type, removeHistoryRange),
  ]);
}
