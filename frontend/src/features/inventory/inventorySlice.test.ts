import { describe, expect, it } from 'vitest';
import reducer, { inventoryFailed, inventoryLoaded, loadInventory } from './inventorySlice';

describe('inventory slice', () => {
  it('starts without inventory data', () => {
    const initial = reducer(undefined, { type: 'unknown' });
    expect(initial.data).toBeNull();
  });

  it('tracks loading, live data and recoverable errors', () => {
    const loading = reducer(undefined, loadInventory());
    expect(loading.status).toBe('loading');
    const loaded = reducer(loading, inventoryLoaded({ equipment: [], pooledStock: [] }));
    expect(loaded.status).toBe('succeeded');
    expect(loaded.data).toEqual({ equipment: [], pooledStock: [] });
    const failed = reducer(loading, inventoryFailed('API unavailable'));
    expect(failed.status).toBe('failed');
    expect(failed.error).toBe('API unavailable');
  });
});
