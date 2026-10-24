import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { loadInventory } from '../features/inventory/inventorySlice';
import DashboardPage from './DashboardPage';

const mocks = vi.hoisted(() => ({
  dispatch: vi.fn(),
  state: {
    inventory: null as unknown,
    checkouts: null as unknown,
  },
}));

vi.mock('../hooks', () => ({
  useAppDispatch: () => mocks.dispatch,
  useAppSelector: (selector: (state: never) => unknown) => selector(mocks.state as never),
}));

describe('dashboard', () => {
  beforeEach(() => {
    mocks.dispatch.mockClear();
    mocks.state.inventory = {
      data: {
        equipment: [{
          id: 'asset-1', name: 'Rover 1', category: 'Rover', isAvailable: false,
          currentHolder: 'Amina', expectedReturnAt: '2026-10-20T10:00:00Z',
          isOverdue: true, latestDamageDescription: null,
        }],
        pooledStock: [{
          id: 'stock-1', name: 'Tripods', totalQuantity: 4,
          availableQuantity: 2, overdueQuantity: 1, latestDamageDescription: null,
        }],
      },
      status: 'succeeded',
      error: null,
    };
    mocks.state.checkouts = {
      open: [{
        id: 'checkout-1', equipmentAssetId: 'asset-1', pooledStockId: null, itemName: 'Rover 1',
        category: 'Rover', holder: 'Amina', expectedReturnAt: '2026-10-20T10:00:00Z',
        outstandingQuantity: 1, isOverdue: true,
      }],
      status: 'succeeded',
      error: null,
    };
  });

  it('shows live availability, overdue equipment and active checkouts', () => {
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><DashboardPage /></MemoryRouter>);
    expect(screen.getAllByText('Rover 1')).toHaveLength(2);
    expect(screen.getAllByText('Past due')).toHaveLength(2);
    expect(screen.getByText(/Amina/)).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('shows API-backed empty states when inventory and checkouts are empty', () => {
    mocks.state.inventory = { data: { equipment: [], pooledStock: [] }, status: 'succeeded', error: null };
    mocks.state.checkouts = { open: [], status: 'succeeded', error: null };
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><DashboardPage /></MemoryRouter>);
    expect(screen.getByText('No equipment registered yet')).toBeInTheDocument();
    expect(screen.getByText('No equipment is out')).toBeInTheDocument();
  });

  it('offers retry instead of replacing an unavailable inventory response with demo data', () => {
    mocks.state.inventory = { data: null, status: 'failed', error: 'API could not be reached.' };
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><DashboardPage /></MemoryRouter>);
    expect(screen.getByRole('alert')).toHaveTextContent('API could not be reached.');
    fireEvent.click(screen.getByRole('button', { name: 'Try again' }));
    expect(mocks.dispatch).toHaveBeenCalledWith(loadInventory());
  });

  it('shows a checkout refresh failure even when old records remain in state', () => {
    mocks.state.checkouts = {
      open: [{
        id: 'checkout-1', equipmentAssetId: 'asset-1', pooledStockId: null, itemName: 'Rover 1',
        category: 'Rover', holder: 'Amina', expectedReturnAt: '2026-10-20T10:00:00Z',
        outstandingQuantity: 1, isOverdue: true,
      }],
      status: 'failed',
      error: 'Checkout list could not be refreshed.',
    };

    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><DashboardPage /></MemoryRouter>);

    expect(screen.getByRole('alert')).toHaveTextContent('Checkout list could not be refreshed.');
  });
});
