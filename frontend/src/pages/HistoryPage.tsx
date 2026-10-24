import { FormEvent, useState } from 'react';
import { FiSearch, FiTrash2 } from 'react-icons/fi';
import { EmptyState, ErrorState, LoadingState } from '../components/AsyncState';
import { formatDate } from '../components/Format';
import PageHeading from '../components/PageHeading';
import { deleteHistoryRange, deleteMovement, loadHistory } from '../features/history/historySlice';
import { useAppDispatch, useAppSelector } from '../hooks';

function toInputValue(date: Date) {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return local.toISOString().slice(0, 16);
}

function toIsoValue(value: string) {
  if (!value) return '';
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? '' : parsed.toISOString();
}

export default function HistoryPage() {
  const dispatch = useAppDispatch();
  const history = useAppSelector((state) => state.history);
  const [from, setFrom] = useState(() => toInputValue(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)));
  const [to, setTo] = useState(() => toInputValue(new Date()));

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const range = { from: toIsoValue(from), to: toIsoValue(to) };
    if (!range.from || !range.to || range.from > range.to) return;
    dispatch(loadHistory(range));
  }

  function deleteSelectedRange() {
    const range = { from: toIsoValue(from), to: toIsoValue(to) };
    if (!history.query || !range.from || !range.to || range.from !== history.query.from || range.to !== history.query.to || range.from > range.to) return;
    if (window.confirm(`Delete all movement records from ${from} through ${to}, inclusive? Equipment, checkouts, and inventory balances will not be changed.`)) {
      dispatch(deleteHistoryRange(range));
    }
  }

  const selectedRangeIsLoaded = Boolean(toIsoValue(from) && toIsoValue(to)) &&
    history.query?.from === toIsoValue(from) && history.query?.to === toIsoValue(to) && history.status === 'succeeded';

  return (
    <>
      <PageHeading eyebrow="AUDIT TRAIL" title="Movement history" description="Review checkouts, returns, and damage reports within a selected date range." />
      <section className="panel history-query-panel">
        <form className="history-query" onSubmit={submit}>
          <label>From<input type="datetime-local" required value={from} onChange={(event) => setFrom(event.target.value)} /></label>
          <label>To<input type="datetime-local" required value={to} onChange={(event) => setTo(event.target.value)} /></label>
          <button className="button button-primary" type="submit"><FiSearch aria-hidden="true" /> Search history</button>
        </form>
        {from && to && new Date(from) > new Date(to) && <p className="inline-error" role="alert">The start date must be before or equal to the end date.</p>}
      </section>
      {history.mutationError && <div className="inline-error" role="alert">{history.mutationError}</div>}
      {history.status === 'loading' ? <LoadingState label="Loading movement history…" /> :
        history.status === 'failed' ? <ErrorState message={history.error ?? 'History unavailable.'} retry={() => {
          if (history.query) dispatch(loadHistory(history.query));
        }} /> :
          history.records.length === 0 ? <EmptyState title="No movements in this date range" detail="Try a wider range or a different time period." /> : (
            <section className="panel table-panel">
              <div className="history-bulk-action">
                <p>{history.records.length} movement record{history.records.length === 1 ? '' : 's'} in the selected range.</p>
                <button className="button button-secondary button-small" type="button" disabled={!selectedRangeIsLoaded || history.mutationStatus === 'loading'} onClick={deleteSelectedRange}><FiTrash2 aria-hidden="true" /> Delete movements in this range</button>
              </div>
              <div className="table-scroll"><table>
                <thead><tr><th scope="col">Date and time</th><th scope="col">Equipment</th><th scope="col">Movement</th><th scope="col">Quantity</th><th scope="col">Recorded by</th><th scope="col">Notes</th><th scope="col">Actions</th></tr></thead>
                <tbody>{history.records.map((record) => <tr key={record.id}><td>{formatDate(record.occurredAt)}</td><td><strong>{record.itemName}</strong><small className="cell-subtitle">Checkout · {record.checkoutId.slice(0, 8)}</small></td><td><span className={`badge ${record.type === 'Damage' ? 'badge-red' : record.type === 'Checkout' ? 'badge-neutral' : 'badge-green'}`}>{record.type}</span></td><td>{record.quantity}</td><td>{record.performedBy}</td><td>{record.description ?? <span className="muted">—</span>}</td><td><button className="icon-button" type="button" aria-label={`Delete ${record.type} record for ${record.itemName}`} disabled={history.mutationStatus === 'loading'} onClick={() => dispatch(deleteMovement(record.id))}><FiTrash2 aria-hidden="true" /></button></td></tr>)}</tbody>
              </table></div>
            </section>
          )}
    </>
  );
}
