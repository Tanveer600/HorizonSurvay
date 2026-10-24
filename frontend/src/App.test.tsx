import { render } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { loadOpenCheckouts } from './features/checkouts/checkoutsSlice';
import { loadCategories } from './features/categories/categoriesSlice';
import { loadInventory } from './features/inventory/inventorySlice';
import App from './App';

const mocks = vi.hoisted(() => ({
  dispatch: vi.fn(),
  state: {
    inventory: { data: { equipment: [], pooledStock: [] }, status: 'succeeded', error: null },
    checkouts: { open: [], status: 'succeeded', error: null },
    ui: { toast: null },
  },
}));

vi.mock('./hooks', () => ({
  useAppDispatch: () => mocks.dispatch,
  useAppSelector: (selector: (state: never) => unknown) => selector(mocks.state as never),
}));

describe('application startup', () => {
  beforeEach(() => mocks.dispatch.mockClear());

  it('requests live inventory and open checkouts when the application mounts', () => {
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><App /></MemoryRouter>);

    expect(mocks.dispatch).toHaveBeenCalledWith(loadInventory());
    expect(mocks.dispatch).toHaveBeenCalledWith(loadOpenCheckouts());
    expect(mocks.dispatch).toHaveBeenCalledWith(loadCategories());
  });
});
