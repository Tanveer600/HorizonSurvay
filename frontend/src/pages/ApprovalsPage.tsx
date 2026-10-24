import { FormEvent, useState } from 'react';
import { FiCheckCircle } from 'react-icons/fi';
import { EmptyState, ErrorState, LoadingState } from '../components/AsyncState';
import { formatDate } from '../components/Format';
import PageHeading from '../components/PageHeading';
import { recordApproval } from '../features/approvals/approvalsSlice';
import { loadInventory } from '../features/inventory/inventorySlice';
import { useAppDispatch, useAppSelector } from '../hooks';

export default function ApprovalsPage() {
  const dispatch = useAppDispatch();
  const inventory = useAppSelector((state) => state.inventory);
  const approvals = useAppSelector((state) => state.approvals);
  const drones = inventory.data?.equipment.filter((item) => item.category?.toLowerCase() === 'drone') ?? [];
  const [equipmentAssetId, setEquipmentAssetId] = useState('');
  const [holder, setHolder] = useState('');

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    dispatch(recordApproval({ equipmentAssetId, holder: holder.trim(), approvedBy: 'Hamza' }));
  }

  return (
    <>
      <PageHeading eyebrow="DRONE SAFETY" title="Drone approval" description="Record Hamza’s approval for one selected drone and the person who will hold it." />
      <div className="two-column">
        <section className="panel form-panel narrow-panel">
          <div className="panel-heading"><div><span className="eyebrow">APPROVAL RECORD</span><h2>Approve a checkout</h2></div><span className="form-icon"><FiCheckCircle aria-hidden="true" /></span></div>
          {inventory.status === 'loading' && !inventory.data ? <LoadingState label="Loading drones…" /> :
            inventory.status === 'failed' ? <ErrorState message={inventory.error ?? 'Inventory unavailable.'} retry={() => dispatch(loadInventory())} /> :
                drones.length === 0 ? <EmptyState title="No drones registered" detail="A drone must be added to inventory before an approval can be recorded." /> : (
            <form className="stack-form" onSubmit={submit}>
              {approvals.error && <div className="inline-error" role="alert">{approvals.error}</div>}
              <label>Select drone<select required value={equipmentAssetId} onChange={(event) => setEquipmentAssetId(event.target.value)}><option value="">Choose a drone</option>{drones.map((drone) => <option key={drone.id} value={drone.id}>{drone.name}</option>)}</select></label>
              <label>Approved holder<input required maxLength={200} value={holder} onChange={(event) => setHolder(event.target.value)} placeholder="Name of the operator" /></label>
              <label>Approver<input value="Hamza" readOnly aria-describedby="approval-note" /><small id="approval-note" className="field-hint">This workflow records the submitted name; it does not verify identity.</small></label>
              <button className="button button-primary button-wide" type="submit" disabled={approvals.status === 'loading'}><FiCheckCircle aria-hidden="true" /> Record approval</button>
            </form>
          )}
        </section>
        <section className="panel history-panel">
          <div className="panel-heading"><div><span className="eyebrow">THIS SESSION</span><h2>Recent approvals</h2></div><span className="count-pill">{approvals.recorded.length}</span></div>
          {approvals.recorded.length === 0 ? <EmptyState title="No approvals recorded here yet" detail="Approval records are held by the API. Only approvals made in this browser session are listed." /> : (
            <div className="compact-list">{approvals.recorded.map((approval) => {
              const drone = drones.find((item) => item.id === approval.equipmentAssetId);
              return <div className="compact-row" key={approval.id}><span className="checkout-status-icon"><FiCheckCircle aria-hidden="true" /></span><div className="compact-copy"><strong>{drone?.name ?? 'Drone'} · {approval.holder}</strong><span>Approved by {approval.approvedBy} · {formatDate(approval.approvedAt)}</span></div><code className="approval-short-id">{approval.id.slice(0, 8)}</code></div>;
            })}</div>
          )}
        </section>
      </div>
    </>
  );
}
