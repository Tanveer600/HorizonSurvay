import { render, screen } from '@testing-library/react';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { store } from '../store';
import { categoriesLoaded } from '../features/categories/categoriesSlice';
import EquipmentPage from './EquipmentPage';

describe('equipment registration page', () => {
  it('describes quantity-tracked stock without hard-coding stock item names', () => {
    store.dispatch(categoriesLoaded([{ id: 'cat-rover', name: 'Rover' }, { id: 'cat-custom', name: 'Survey gear' }]));
    render(
      <Provider store={store}>
        <MemoryRouter><EquipmentPage /></MemoryRouter>
      </Provider>,
    );

    expect(screen.getByText('Quantity-tracked stock appears in Inventory and can be issued from Checkouts.')).toBeInTheDocument();
    expect(screen.queryByText(/Tripods|Field tablets/)).not.toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Survey gear' })).toBeInTheDocument();
    expect(screen.queryByRole('option', { name: 'Drone' })).not.toBeInTheDocument();
  });
});
