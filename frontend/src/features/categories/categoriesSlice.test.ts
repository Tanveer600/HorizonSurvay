import { describe, expect, it } from 'vitest';
import reducer, { categoriesFailed, categoriesLoaded, loadCategories } from './categoriesSlice';

describe('categories state', () => {
  it('loads API categories and exposes retryable load errors without a fallback list', () => {
    const loading = reducer(undefined, loadCategories());
    expect(loading.status).toBe('loading');
    expect(loading.categories).toEqual([]);

    const failed = reducer(loading, categoriesFailed('API unavailable'));
    expect(failed.status).toBe('failed');
    expect(failed.error).toBe('API unavailable');
    expect(failed.categories).toEqual([]);

    const loaded = reducer(failed, categoriesLoaded([{ id: 'cat-1', name: 'Rover' }]));
    expect(loaded.categories).toEqual([{ id: 'cat-1', name: 'Rover' }]);
    expect(loaded.status).toBe('succeeded');
  });
});
