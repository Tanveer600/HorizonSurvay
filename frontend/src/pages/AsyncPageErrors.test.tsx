import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { loadInventory } from '../features/inventory/inventorySlice';
import ApprovalsPage from './ApprovalsPage';
import InventoryPage from './InventoryPage';

const mocks = vi.hoisted(() => ({
  dispatch: vi.fn(),
  state: {
    inventory: { data: null as unknown, status: 'failed', error: 'API could not be reached.' },
    approvals: { recorded: [], status: 'idle', error: null },
  },
}));

vi.mock('../hooks', () => ({
  useAppDispatch: () => mocks.dispatch,
  useAppSelector: (selector: (state: never) => unknown) => selector(mocks.state as never),
}));

describe('inventory-backed page errors', () => {
  beforeEach(() => {
    mocks.dispatch.mockClear();
    mocks.state.inventory = { data: null, status: 'failed', error: 'API could not be reached.' };
  });

  it('does not describe unavailable inventory as empty and offers retry', () => {
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><InventoryPage /></MemoryRouter>);

    expect(screen.getByRole('alert')).toHaveTextContent('API could not be reached.');
    expect(screen.queryByText('Inventory is empty')).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Try again' }));
    expect(mocks.dispatch).toHaveBeenCalledWith(loadInventory());
  });

  it('does not describe unavailable drone inventory as empty and offers retry', () => {
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><ApprovalsPage /></MemoryRouter>);

    expect(screen.getByRole('alert')).toHaveTextContent('API could not be reached.');
    expect(screen.queryByText('No drones registered')).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Try again' }));
    expect(mocks.dispatch).toHaveBeenCalledWith(loadInventory());
  });

  it('does not call cached empty inventory current when refresh fails', () => {
    mocks.state.inventory = {
      data: { equipment: [], pooledStock: [] },
      status: 'failed',
      error: 'Inventory refresh failed.',
    };

    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><InventoryPage /></MemoryRouter>);

    expect(screen.getByRole('alert')).toHaveTextContent('Inventory refresh failed.');
    expect(screen.queryByText('Inventory is empty')).not.toBeInTheDocument();
  });
});
