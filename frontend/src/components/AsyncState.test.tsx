import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { EmptyState, ErrorState, LoadingState } from './AsyncState';

describe('accessible asynchronous states', () => {
  it('announces loading and empty state content', () => {
    render(<><LoadingState label="Loading live inventory…" /><EmptyState title="No checkouts" detail="Nothing is currently out." /></>);
    expect(screen.getByRole('status')).toHaveTextContent('Loading live inventory');
    expect(screen.getByText('No checkouts')).toBeInTheDocument();
    expect(screen.getByText('Nothing is currently out.')).toBeInTheDocument();
  });

  it('exposes an actionable retry for recoverable API errors', () => {
    const retry = vi.fn();
    render(<ErrorState message="API unavailable" retry={retry} />);
    expect(screen.getByRole('alert')).toHaveTextContent('API unavailable');
    fireEvent.click(screen.getByRole('button', { name: 'Try again' }));
    expect(retry).toHaveBeenCalledOnce();
  });
});
