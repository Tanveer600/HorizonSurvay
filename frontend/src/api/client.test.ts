import { afterEach, describe, expect, it, vi } from 'vitest';
import { apiClient, categoryApi, getApiErrorMessage, historyApi, inventoryApi } from './client';

describe('API client', () => {
  afterEach(() => vi.restoreAllMocks());

  it('requests live inventory from the configured API client', async () => {
    const request = vi.spyOn(apiClient, 'get').mockResolvedValue({
      data: { equipment: [], pooledStock: [] },
    });

    await expect(inventoryApi.getInventory()).resolves.toEqual({
      equipment: [],
      pooledStock: [],
    });
    expect(request).toHaveBeenCalledWith('/inventory');
  });

  it('extracts API problem details and gives offline errors an actionable message', () => {
    expect(getApiErrorMessage({
      isAxiosError: true,
      response: { data: { detail: 'Asset is already checked out.' }, status: 409 },
    })).toBe('Asset is already checked out.');
    expect(getApiErrorMessage({
      isAxiosError: true,
      message: 'Network Error',
    })).toContain('API could not be reached');
  });

  it('uses persisted category and movement deletion endpoints', async () => {
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: [] });
    const post = vi.spyOn(apiClient, 'post').mockResolvedValue({ data: { id: 'category-1', name: 'Rover' } });
    const put = vi.spyOn(apiClient, 'put').mockResolvedValue({ data: { id: 'category-1', name: 'Survey gear' } });
    const deleteRequest = vi.spyOn(apiClient, 'delete').mockResolvedValue({ data: { deletedCount: 2 } });

    await categoryApi.getAll();
    await categoryApi.create('Rover');
    await categoryApi.update('category-1', 'Survey gear');
    await categoryApi.delete('category-1');
    await historyApi.deleteOne('movement-1');
    await expect(historyApi.deleteRange('from', 'to')).resolves.toBe(2);

    expect(get).toHaveBeenCalledWith('/categories');
    expect(post).toHaveBeenCalledWith('/categories', { name: 'Rover' });
    expect(put).toHaveBeenCalledWith('/categories/category-1', { name: 'Survey gear' });
    expect(deleteRequest).toHaveBeenCalledWith('/history', { params: { from: 'from', to: 'to' } });
  });
});
