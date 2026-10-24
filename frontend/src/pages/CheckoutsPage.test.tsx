import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { submitCheckout } from '../features/checkouts/checkoutsSlice';
import CheckoutsPage from './CheckoutsPage';

const mocks = vi.hoisted(() => ({
  dispatch: vi.fn(),
  state: {
    inventory: null as unknown,
    approvals: null as unknown,
    checkouts: null as unknown,
  },
}));

vi.mock('../hooks', () => ({
  useAppDispatch: () => mocks.dispatch,
  useAppSelector: (selector: (state: never) => unknown) => selector(mocks.state as never),
}));

describe('checkout form', () => {
  beforeEach(() => {
    mocks.dispatch.mockClear();
    mocks.state.inventory = {
      data: {
        equipment: [
          { id: 'drone-1', name: 'Drone 1', categoryId: 'category-drone', category: 'dRoNe', isAvailable: true, currentHolder: null, expectedReturnAt: null, isOverdue: false, latestDamageDescription: null },
          { id: 'rover-1', name: 'Rover 1', category: 'Rover', isAvailable: true, currentHolder: null, expectedReturnAt: null, isOverdue: false, latestDamageDescription: null },
        ],
        pooledStock: [],
      },
      status: 'succeeded',
      error: null,
    };
    mocks.state.approvals = {
      recorded: [
        { id: 'approval-1', equipmentAssetId: 'drone-1', holder: 'Amina', approvedBy: 'Hamza', approvedAt: '2026-10-21T10:00:00Z' },
        { id: 'approval-2', equipmentAssetId: 'drone-1', holder: 'Saima', approvedBy: 'Hamza', approvedAt: '2026-10-21T10:00:00Z' },
      ],
    };
    mocks.state.checkouts = {
      open: [], status: 'succeeded', error: null,
      mutationError: null, mutationStatus: 'idle', returnStatus: 'idle', receipt: null,
    };
  });

  it('offers only a matching drone-holder approval and submits that API ID', () => {
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><CheckoutsPage /></MemoryRouter>);
    fireEvent.change(screen.getByLabelText('Holder name'), { target: { value: 'Amina' } });
    const targetSelect = screen.getByLabelText('Equipment or stock');
    fireEvent.change(targetSelect, { target: { value: 'asset:drone-1' } });
    expect(targetSelect).toHaveValue('asset:drone-1');
    const approvalSelect = screen.getByLabelText(/^Matching Hamza approval/);
    expect(screen.getByRole('option', { name: /Amina/ })).toBeInTheDocument();
    expect(screen.queryByRole('option', { name: /Saima/ })).not.toBeInTheDocument();
    fireEvent.change(approvalSelect, { target: { value: 'approval-1' } });
    fireEvent.change(screen.getByLabelText('Expected return'), { target: { value: '2030-01-01T10:00' } });
    fireEvent.click(screen.getByRole('button', { name: /Confirm checkout/ }));

    expect(mocks.dispatch).toHaveBeenCalledWith(submitCheckout({
      holder: 'Amina',
      expectedReturnAt: new Date('2030-01-01T10:00').toISOString(),
      quantity: 1,
      equipmentAssetId: 'drone-1',
      approvalId: 'approval-1',
    }));
  });

  it('keeps form values and shows a conflict instead of a false success', () => {
    mocks.state.checkouts = {
      open: [], status: 'succeeded', error: null,
      mutationError: 'Equipment asset is already checked out.',
      mutationStatus: 'failed', returnStatus: 'idle', receipt: null,
    };
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><CheckoutsPage /></MemoryRouter>);
    const holderInput = screen.getByLabelText('Holder name');
    const targetSelect = screen.getByLabelText('Equipment or stock');
    fireEvent.change(holderInput, { target: { value: 'Amina' } });
    fireEvent.change(targetSelect, { target: { value: 'asset:rover-1' } });
    fireEvent.change(screen.getByLabelText('Expected return'), { target: { value: '2030-01-01T10:00' } });
    fireEvent.click(screen.getByRole('button', { name: /Confirm checkout/ }));

    expect(screen.getByRole('alert')).toHaveTextContent('Equipment asset is already checked out.');
    expect(holderInput).toHaveValue('Amina');
    expect(targetSelect).toHaveValue('asset:rover-1');
    expect(screen.queryByText('Checkout recorded')).not.toBeInTheDocument();
  });

  it('shows a refresh failure rather than presenting cached checkouts as current', () => {
    mocks.state.checkouts = {
      open: [{
        id: 'checkout-1', equipmentAssetId: 'rover-1', pooledStockId: null, itemName: 'Rover 1',
        category: 'Rover', holder: 'Amina', expectedReturnAt: '2030-01-01T10:00:00Z',
        outstandingQuantity: 1, isOverdue: false,
      }],
      status: 'failed', error: 'Checkout list could not be refreshed.',
      mutationError: null, mutationStatus: 'idle', returnStatus: 'idle', receipt: null,
    };
    render(<MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}><CheckoutsPage /></MemoryRouter>);

    expect(screen.getByRole('alert')).toHaveTextContent('Checkout list could not be refreshed.');
    expect(screen.queryByText('Rover 1', { selector: 'h3' })).not.toBeInTheDocument();
  });
});
