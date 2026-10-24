import { runSaga } from 'redux-saga';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { approvalApi, checkoutApi, historyApi, inventoryApi } from '../api/client';
import { categoryApi } from '../api/client';
import type { CheckoutReceipt } from '../api/types';
import { approvalConsumed, approvalFailed, approvalRecorded, recordApproval } from './approvals/approvalsSlice';
import {
  checkoutMutationFailed,
  checkoutSucceeded,
  loadOpenCheckouts,
  returnFailed,
  returnSucceeded,
  submitCheckout,
  submitReturn,
} from './checkouts/checkoutsSlice';
import { equipmentRegistered, registerEquipment } from './equipment/equipmentSlice';
import { deleteMovement, historyFailed, historyLoaded, historyMutationFailed, loadHistory, movementDeleted } from './history/historySlice';
import { categoriesFailed, categoriesLoaded } from './categories/categoriesSlice';
import { inventoryLoaded, loadInventory } from './inventory/inventorySlice';
import { createApproval, createCheckout, createEquipment, fetchCategories, fetchHistory, fetchInventory, removeMovement, returnCheckout } from './rootSaga';
import { showToast } from './ui/uiSlice';

describe('inventory saga', () => {
  afterEach(() => vi.restoreAllMocks());

  it('loads live inventory and dispatches the fulfilled result', async () => {
    const snapshot = { equipment: [], pooledStock: [] };
    vi.spyOn(inventoryApi, 'getInventory').mockResolvedValue(snapshot);
    const actions: unknown[] = [];

    await runSaga({ dispatch: (action: unknown) => actions.push(action) }, fetchInventory).toPromise();

    expect(inventoryApi.getInventory).toHaveBeenCalledOnce();
    expect(actions).toContainEqual(inventoryLoaded(snapshot));
    expect(loadInventory().type).toBe('inventory/loadInventory');
  });

  it('records a successful checkout once and refreshes inventory and active checkouts', async () => {
    const receipt: CheckoutReceipt = {
      id: 'checkout-1',
      holder: 'Amina',
      expectedReturnAt: '2026-10-22T12:00:00Z',
      quantity: 1,
      outstandingQuantity: 1,
      assetAvailable: false,
      stockAvailableQuantity: null,
    };
    vi.spyOn(checkoutApi, 'checkout').mockResolvedValue(receipt);
    const actions: unknown[] = [];
    const request = { holder: 'Amina', expectedReturnAt: receipt.expectedReturnAt, equipmentAssetId: 'drone-1', quantity: 1, approvalId: 'approval-1' };

    await runSaga({
      dispatch: (action: unknown) => actions.push(action),
      getState: () => ({ history: { query: null } }),
    }, createCheckout, submitCheckout(request)).toPromise();

    expect(checkoutApi.checkout).toHaveBeenCalledOnce();
    expect(actions).toContainEqual(checkoutSucceeded(receipt));
    expect(actions).toContainEqual(approvalConsumed('approval-1'));
    expect(actions).toContainEqual(loadInventory());
    expect(actions).toContainEqual(loadOpenCheckouts());
    expect(actions).toContainEqual(showToast({ tone: 'success', message: 'Checkout recorded for Amina.' }));
  });

  it('shows API conflict detail without dispatching checkout success', async () => {
    vi.spyOn(checkoutApi, 'checkout').mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: { detail: 'Drone approval does not match this holder.' } },
    });
    const actions: unknown[] = [];
    const request = { holder: 'Amina', expectedReturnAt: '2026-10-22T12:00:00Z', equipmentAssetId: 'drone-1', quantity: 1, approvalId: 'wrong-id' };

    await runSaga({
      dispatch: (action: unknown) => actions.push(action),
      getState: () => ({ history: { query: null } }),
    }, createCheckout, submitCheckout(request)).toPromise();

    expect(actions).toContainEqual(checkoutMutationFailed('Drone approval does not match this holder.'));
    expect(actions.some((action) => (action as { type?: string }).type === checkoutSucceeded.type)).toBe(false);
  });

  it('records and refreshes a successful return, but keeps API failures as errors', async () => {
    const returnSpy = vi.spyOn(checkoutApi, 'returnItems').mockResolvedValue();
    const actions: unknown[] = [];
    await runSaga({
      dispatch: (action: unknown) => actions.push(action),
      getState: () => ({ history: { query: { from: '2026-10-01T00:00:00Z', to: '2026-10-21T00:00:00Z' } } }),
    }, returnCheckout, submitReturn({ id: 'checkout-1', request: { returner: 'Saima', quantity: 1 } })).toPromise();

    expect(returnSpy).toHaveBeenCalledWith('checkout-1', { returner: 'Saima', quantity: 1 });
    expect(actions).toContainEqual(returnSucceeded());
    expect(actions.some((action) => (action as { type?: string }).type === 'history/loadHistory')).toBe(true);

    returnSpy.mockRejectedValue({
      isAxiosError: true,
      response: { status: 400, data: { detail: 'Return quantity exceeds the outstanding amount.' } },
    });
    const failedActions: unknown[] = [];
    await runSaga({
      dispatch: (action: unknown) => failedActions.push(action),
      getState: () => ({ history: { query: null } }),
    }, returnCheckout, submitReturn({ id: 'checkout-1', request: { returner: 'Saima', quantity: 9 } })).toPromise();

    expect(failedActions).toContainEqual(returnFailed('Return quantity exceeds the outstanding amount.'));
    expect(failedActions.some((action) => (action as { type?: string }).type === returnSucceeded.type)).toBe(false);
  });

  it('keeps approval errors visible and registers assets only after API success', async () => {
    vi.spyOn(approvalApi, 'record').mockRejectedValue({
      isAxiosError: true,
      response: { status: 404, data: { detail: 'Drone was not found.' } },
    });
    const approvalActions: unknown[] = [];
    await runSaga({ dispatch: (action: unknown) => approvalActions.push(action) }, createApproval,
      recordApproval({ equipmentAssetId: 'missing-drone', holder: 'Amina', approvedBy: 'Hamza' })).toPromise();
    expect(approvalActions).toContainEqual(approvalFailed('Drone was not found.'));
    expect(approvalActions.some((action) => (action as { type?: string }).type === approvalRecorded.type)).toBe(false);

    vi.spyOn(inventoryApi, 'registerEquipment').mockResolvedValue({
      id: 'drone-1', name: 'Drone 1', category: 'Drone', isAvailable: true, currentHolder: null,
      categoryId: 'drone-category',
      expectedReturnAt: null, isOverdue: false, latestDamageDescription: null,
    });
    const equipmentActions: unknown[] = [];
    await runSaga({ dispatch: (action: unknown) => equipmentActions.push(action) }, createEquipment,
      registerEquipment({ name: 'Drone 1', categoryId: 'drone-category' })).toPromise();
    expect(equipmentActions).toContainEqual(equipmentRegistered('Drone 1'));
    expect(equipmentActions).toContainEqual(loadInventory());
  });

  it('loads categories from the API and preserves API failures', async () => {
    vi.spyOn(categoryApi, 'getAll').mockResolvedValue([{ id: 'category-1', name: 'Drone' }]);
    const actions: unknown[] = [];
    await runSaga({ dispatch: (action: unknown) => actions.push(action) }, fetchCategories).toPromise();
    expect(actions).toContainEqual(categoriesLoaded([{ id: 'category-1', name: 'Drone' }]));

    vi.mocked(categoryApi.getAll).mockRejectedValue(new Error('offline'));
    const failed: unknown[] = [];
    await runSaga({ dispatch: (action: unknown) => failed.push(action) }, fetchCategories).toPromise();
    expect(failed).toContainEqual(categoriesFailed('offline'));
  });

  it('removes one movement only after API success and retains the row on failure', async () => {
    vi.spyOn(historyApi, 'deleteOne').mockResolvedValue();
    const actions: unknown[] = [];
    await runSaga({ dispatch: (action: unknown) => actions.push(action) }, removeMovement, deleteMovement('movement-1')).toPromise();
    expect(actions).toContainEqual(movementDeleted('movement-1'));

    vi.mocked(historyApi.deleteOne).mockRejectedValue(new Error('delete failed'));
    const failed: unknown[] = [];
    await runSaga({ dispatch: (action: unknown) => failed.push(action) }, removeMovement, deleteMovement('movement-1')).toPromise();
    expect(failed).toContainEqual(historyMutationFailed('delete failed'));
    expect(failed.some((action) => (action as { type?: string }).type === movementDeleted.type)).toBe(false);
  });

  it('loads movement history and reports API errors without fabricating records', async () => {
    const movements = [{
      id: 'movement-1', checkoutId: 'checkout-1', itemName: 'Rover', type: 'Checkout' as const,
      quantity: 1, occurredAt: '2026-10-21T12:00:00Z', performedBy: 'Amina', description: null,
    }];
    vi.spyOn(historyApi, 'get').mockResolvedValue(movements);
    const historyActions: unknown[] = [];
    const query = { from: '2026-10-01T00:00:00Z', to: '2026-10-21T23:59:00Z' };
    await runSaga({ dispatch: (action: unknown) => historyActions.push(action) }, fetchHistory, loadHistory(query)).toPromise();
    expect(historyApi.get).toHaveBeenCalledWith(query.from, query.to);
    expect(historyActions).toContainEqual(historyLoaded(movements));

    vi.mocked(historyApi.get).mockRejectedValue({
      isAxiosError: true,
      response: { status: 400, data: { detail: 'Both range boundaries are required.' } },
    });
    const failedHistoryActions: unknown[] = [];
    await runSaga({ dispatch: (action: unknown) => failedHistoryActions.push(action) }, fetchHistory, loadHistory(query)).toPromise();
    expect(failedHistoryActions).toContainEqual(historyFailed('Both range boundaries are required.'));
    expect(failedHistoryActions.some((action) => (action as { type?: string }).type === historyLoaded.type)).toBe(false);
  });
});
