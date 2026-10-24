import { FormEvent, useEffect, useMemo, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiCornerDownLeft, FiTruck } from 'react-icons/fi';
import { EmptyState, ErrorState, LoadingState } from '../components/AsyncState';
import { formatDate, todayDateTimeLocal } from '../components/Format';
import PageHeading from '../components/PageHeading';
import { clearReturnStatus, loadOpenCheckouts, submitCheckout, submitReturn } from '../features/checkouts/checkoutsSlice';
import { useAppDispatch, useAppSelector } from '../hooks';
import type { CheckoutRequest, ReturnRequest } from '../api/types';

export default function CheckoutsPage() {
  const dispatch = useAppDispatch();
  const inventory = useAppSelector((state) => state.inventory.data);
  const approvals = useAppSelector((state) => state.approvals.recorded);
  const { open, status, error, mutationError, receipt, mutationStatus, returnStatus } = useAppSelector((state) => state.checkouts);
  const [holder, setHolder] = useState('');
  const [target, setTarget] = useState('');
  const [expectedReturnAt, setExpectedReturnAt] = useState(todayDateTimeLocal);
  const [quantity, setQuantity] = useState('1');
  const [approvalId, setApprovalId] = useState('');
  const [returning, setReturning] = useState<string | null>(null);
  const [returner, setReturner] = useState('');
  const [returnQuantity, setReturnQuantity] = useState('1');
  const [damageDescription, setDamageDescription] = useState('');
  const [damageQuantity, setDamageQuantity] = useState('1');
  const returnDialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = returnDialogRef.current;
    if (returning && dialog && !dialog.open) dialog.showModal();
    if (!returning && dialog?.open) dialog.close();
  }, [returning]);

  useEffect(() => {
    if (returning && returnStatus === 'succeeded') {
      setReturning(null);
      setReturner('');
      setDamageDescription('');
    }
  }, [returnStatus, returning]);

  const selectedEquipmentId = target.startsWith('asset:') ? target.slice(6) : '';
  const selectedStockId = target.startsWith('stock:') ? target.slice(6) : '';
  const selectedAsset = inventory?.equipment.find((item) => item.id === selectedEquipmentId);
  const selectedAssetRequiresApproval = selectedAsset?.category?.toLowerCase() === 'drone';
  const availableTargets = useMemo(() => [
    ...(inventory?.equipment.filter((item) => item.isAvailable).map((item) => ({ key: `asset:${item.id}`, name: item.name, info: item.category })) ?? []),
    ...(inventory?.pooledStock.filter((item) => item.availableQuantity > 0).map((item) => ({ key: `stock:${item.id}`, name: item.name, info: `${item.availableQuantity} available` })) ?? []),
  ], [inventory]);
  const matchingApprovals = approvals.filter((approval) => approval.equipmentAssetId === selectedEquipmentId && approval.holder.toLowerCase() === holder.trim().toLowerCase());

  function handleCheckout(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!target) return;
    const request: CheckoutRequest = {
      holder: holder.trim(),
      expectedReturnAt: new Date(expectedReturnAt).toISOString(),
      quantity: selectedEquipmentId ? 1 : Number(quantity),
      ...(selectedEquipmentId ? { equipmentAssetId: selectedEquipmentId } : { pooledStockId: selectedStockId }),
      ...(selectedAssetRequiresApproval ? { approvalId } : {}),
    };
    dispatch(submitCheckout(request));
  }

  function handleReturn(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!returning) return;
    const request: ReturnRequest = {
      returner: returner.trim(),
      quantity: Number(returnQuantity),
      ...(damageDescription.trim() ? { damageDescription: damageDescription.trim(), damageQuantity: Number(damageQuantity) } : {}),
    };
    dispatch(submitReturn({ id: returning, request }));
  }

  return (
    <>
      <PageHeading eyebrow="ISSUE & RETURN" title="Checkouts" description="Record equipment leaving the store and bring it back into availability." />
      <div className="checkout-layout">
        <section className="panel form-panel">
          <div className="panel-heading"><div><span className="eyebrow">NEW MOVEMENT</span><h2>Issue equipment</h2></div><span className="form-icon"><FiTruck aria-hidden="true" /></span></div>
          {receipt && <div className="receipt-box" role="status"><strong>Checkout recorded</strong><span>Receipt ID · {receipt.id}</span><small>{receipt.outstandingQuantity} unit(s) now assigned to {receipt.holder}.</small></div>}
          {mutationError && <div className="inline-error" role="alert">{mutationError}</div>}
          <form className="stack-form" onSubmit={handleCheckout}>
            <label>Holder name<input required maxLength={200} value={holder} onChange={(event) => { setHolder(event.target.value); setApprovalId(''); }} placeholder="Who is taking the equipment?" /></label>
            <label>Equipment or stock<select required value={target} onChange={(event) => { setTarget(event.target.value); setApprovalId(''); }}>
              <option value="">Select available equipment</option>
              {availableTargets.map((item) => <option value={item.key} key={item.key}>{item.name} — {item.info}</option>)}
            </select></label>
            {selectedStockId && <label>Quantity<input type="number" min="1" max={inventory?.pooledStock.find((item) => item.id === selectedStockId)?.availableQuantity ?? 1} required value={quantity} onChange={(event) => setQuantity(event.target.value)} /></label>}
            <label>Expected return<input type="datetime-local" required value={expectedReturnAt} onChange={(event) => setExpectedReturnAt(event.target.value)} /></label>
            {selectedAssetRequiresApproval && <label>Matching Hamza approval<select required value={approvalId} onChange={(event) => setApprovalId(event.target.value)}>
              <option value="">Select a recorded approval</option>
              {matchingApprovals.map((item) => <option value={item.id} key={item.id}>{item.holder} · {formatDate(item.approvedAt)} · {item.id.slice(0, 8)}</option>)}
            </select><small className="field-hint">Approval must be for this drone and holder. <Link to="/approvals">Record approval</Link></small></label>}
            <button className="button button-primary button-wide" type="submit" disabled={mutationStatus === 'loading'}>
              <FiTruck aria-hidden="true" /> Confirm checkout
            </button>
            <small className="form-footnote">Submissions are recorded by the live API. Rejected requests keep your form values.</small>
          </form>
        </section>
        <section className="panel open-panel">
          <div className="panel-heading"><div><span className="eyebrow">CURRENTLY OUT</span><h2>Open checkouts</h2></div><span className="count-pill">{open.length}</span></div>
          {status === 'loading' && open.length === 0 ? <LoadingState label="Loading active checkouts…" /> :
            status === 'failed' ? <ErrorState message={error ?? 'Checkouts unavailable.'} retry={() => dispatch(loadOpenCheckouts())} /> :
              open.length === 0 ? <EmptyState title="Nothing is currently checked out" detail="New issues will appear here, including after you reload." /> : (
                <div className="checkout-list">
                  {open.map((item) => <article className="checkout-card" key={item.id}>
                    <div className="checkout-card-top"><div><span className="checkout-category">{item.category ?? 'POOLED STOCK'}</span><h3>{item.itemName}</h3></div><span className={`badge ${item.isOverdue ? 'badge-red' : 'badge-green'}`}>{item.isOverdue ? 'Past due' : 'On loan'}</span></div>
                    <div className="checkout-facts"><span><small>WITH</small><strong>{item.holder}</strong></span><span><small>EXPECTED BACK</small><strong>{formatDate(item.expectedReturnAt)}</strong></span><span><small>OUTSTANDING</small><strong>{item.outstandingQuantity} unit(s)</strong></span></div>
                    <div className="checkout-card-bottom"><small>Checkout · {item.id.slice(0, 8)}</small><button type="button" className="button button-secondary button-small" onClick={() => {
                      dispatch(clearReturnStatus());
                      setReturning(item.id);
                      setReturnQuantity(String(item.outstandingQuantity));
                    }}><FiCornerDownLeft aria-hidden="true" /> Record return</button></div>
                  </article>)}
                </div>
              )}
        </section>
      </div>
      {returning && <dialog ref={returnDialogRef} className="dialog-panel" aria-labelledby="return-title" onCancel={(event) => {
        event.preventDefault();
        setReturning(null);
      }} onClose={() => setReturning(null)}>
          <div className="panel-heading"><div><span className="eyebrow">RETURN EQUIPMENT</span><h2 id="return-title">Record a return</h2></div><button type="button" className="icon-button" aria-label="Close return dialog" onClick={() => setReturning(null)}>×</button></div>
          {mutationError && <div className="inline-error" role="alert">{mutationError}</div>}
          <form className="stack-form" onSubmit={handleReturn}>
            <label>Returner name<input autoFocus required maxLength={200} value={returner} onChange={(event) => setReturner(event.target.value)} /></label>
            <label>Quantity returned<input type="number" min="1" max={open.find((item) => item.id === returning)?.outstandingQuantity ?? 1} required value={returnQuantity} onChange={(event) => setReturnQuantity(event.target.value)} /></label>
            <label>Damage notes <span className="optional">(optional)</span><textarea maxLength={1000} rows={3} value={damageDescription} onChange={(event) => setDamageDescription(event.target.value)} placeholder="Describe any damage observed" /></label>
            {damageDescription.trim() && <label>Quantity affected<input type="number" min="1" max={returnQuantity || 1} required value={damageQuantity} onChange={(event) => setDamageQuantity(event.target.value)} /></label>}
            <div className="dialog-actions"><button type="button" className="button button-secondary" onClick={() => setReturning(null)}>Cancel</button><button type="submit" className="button button-primary" disabled={returnStatus === 'loading'}><FiCornerDownLeft aria-hidden="true" /> Save return</button></div>
          </form>
      </dialog>}
    </>
  );
}
