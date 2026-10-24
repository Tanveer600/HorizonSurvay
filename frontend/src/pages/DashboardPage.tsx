import { Link } from 'react-router-dom';
import { FiAlertTriangle, FiArrowUpRight, FiBox, FiClock, FiTruck } from 'react-icons/fi';
import PageHeading from '../components/PageHeading';
import { EmptyState, ErrorState, LoadingState } from '../components/AsyncState';
import { formatDate } from '../components/Format';
import { loadOpenCheckouts } from '../features/checkouts/checkoutsSlice';
import { loadInventory } from '../features/inventory/inventorySlice';
import { useAppDispatch, useAppSelector } from '../hooks';

export default function DashboardPage() {
  const dispatch = useAppDispatch();
  const inventory = useAppSelector((state) => state.inventory);
  const checkouts = useAppSelector((state) => state.checkouts);

  if (inventory.status === 'loading' && !inventory.data) return <LoadingState label="Preparing your inventory dashboard…" />;
  if (inventory.status === 'failed' && !inventory.data) {
    return <ErrorState message={inventory.error ?? 'Inventory unavailable.'} retry={() => dispatch(loadInventory())} />;
  }
  const equipment = inventory.data?.equipment ?? [];
  const stock = inventory.data?.pooledStock ?? [];
  const totalAvailable = equipment.filter((item) => item.isAvailable).length + stock.reduce((sum, item) => sum + item.availableQuantity, 0);
  const overdue = (checkouts.open ?? []).filter((item) => item.isOverdue).length;
  const inUse = (checkouts.open ?? []).length;

  return (
    <>
      <PageHeading eyebrow="FIELD OPERATIONS / OVERVIEW" title="Good to see you." description="A clear view of what’s ready, what’s out, and what needs to come back." action={<Link className="button button-primary" to="/checkouts"><FiTruck aria-hidden="true" /> Issue equipment</Link>} />
      {inventory.status === 'failed' && <ErrorState message={inventory.error ?? 'Inventory unavailable.'} retry={() => dispatch(loadInventory())} />}
      <div className="metric-grid">
        <article className="metric-card"><span className="metric-icon"><FiBox aria-hidden="true" /></span><div><span className="metric-label">Available now</span><strong>{totalAvailable}</strong><small>individual assets and pooled units</small></div></article>
        <article className="metric-card"><span className="metric-icon"><FiTruck aria-hidden="true" /></span><div><span className="metric-label">Currently checked out</span><strong>{inUse}</strong><small>open checkout records</small></div></article>
        <article className={`metric-card ${overdue > 0 ? 'metric-alert' : ''}`}><span className="metric-icon"><FiAlertTriangle aria-hidden="true" /></span><div><span className="metric-label">Past due</span><strong>{overdue}</strong><small>{overdue ? 'needs a follow-up' : 'everything is on schedule'}</small></div></article>
      </div>
      <div className="dashboard-grid">
        <section className="panel inventory-preview">
          <div className="panel-heading"><div><span className="eyebrow">ASSET SNAPSHOT</span><h2>Equipment status</h2></div><Link className="text-link" to="/inventory">Full inventory <FiArrowUpRight aria-hidden="true" /></Link></div>
          {equipment.length + stock.length === 0 ? <EmptyState title="No equipment registered yet" detail="Register an asset to begin tracking availability." /> : (
            <div className="compact-list">
              {equipment.slice(0, 4).map((item) => <div className="compact-row" key={item.id}>
                <span className={`item-avatar ${(item.category ?? '').toLowerCase()}`}>{item.category?.slice(0, 1) ?? '?'}</span>
                <div className="compact-copy"><strong>{item.name}</strong><span>{item.category ?? 'Uncategorized'}</span></div>
                <span className={`badge ${item.isAvailable ? 'badge-green' : item.isOverdue ? 'badge-red' : 'badge-neutral'}`}>{item.isAvailable ? 'Available' : item.isOverdue ? 'Overdue' : `With ${item.currentHolder}`}</span>
              </div>)}
              {stock.slice(0, 3).map((item) => <div className="compact-row" key={item.id}>
                <span className="item-avatar stock">#</span>
                <div className="compact-copy"><strong>{item.name}</strong><span>Pooled stock · {item.totalQuantity} total</span></div>
                <span className="badge badge-neutral">{item.availableQuantity} available</span>
              </div>)}
            </div>
          )}
        </section>
        <section className="panel checkouts-preview">
          <div className="panel-heading"><div><span className="eyebrow">LIVE TRACKING</span><h2>Open checkouts</h2></div><Link className="text-link" to="/checkouts">View all <FiArrowUpRight aria-hidden="true" /></Link></div>
          {checkouts.status === 'loading' && checkouts.open.length === 0 ? <LoadingState label="Loading checkouts…" /> :
            checkouts.status === 'failed' ? <ErrorState message={checkouts.error ?? 'Checkouts unavailable.'} retry={() => dispatch(loadOpenCheckouts())} /> :
              checkouts.open.length === 0 ? <EmptyState title="No equipment is out" detail="New checkouts will appear here." /> : (
                <div className="compact-list">
                  {checkouts.open.slice(0, 5).map((item) => <div className="compact-row" key={item.id}>
                    <span className={`checkout-status-icon ${item.isOverdue ? 'overdue' : ''}`}><FiClock aria-hidden="true" /></span>
                    <div className="compact-copy"><strong>{item.itemName} <span className="quantity-note">×{item.outstandingQuantity}</span></strong><span>{item.holder} · due {formatDate(item.expectedReturnAt)}</span></div>
                    {item.isOverdue && <span className="badge badge-red">Past due</span>}
                  </div>)}
                </div>
              )}
        </section>
      </div>
      <p className="quiet-note">Inventory is refreshed from the Horizon Survey API. No sample data is displayed.</p>
    </>
  );
}
