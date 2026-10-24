import PageHeading from '../components/PageHeading';
import { EmptyState, ErrorState, LoadingState } from '../components/AsyncState';
import { formatDate } from '../components/Format';
import { loadInventory } from '../features/inventory/inventorySlice';
import { useAppDispatch, useAppSelector } from '../hooks';

export default function InventoryPage() {
  const dispatch = useAppDispatch();
  const { data, status, error } = useAppSelector((state) => state.inventory);
  if (status === 'loading' && !data) return <LoadingState label="Loading inventory…" />;
  if (status === 'failed' && !data) {
    return (
      <>
        <PageHeading eyebrow="ASSET MANAGEMENT" title="Inventory" description="Current availability and holder for every tracked asset and pooled item." />
        <ErrorState message={error ?? 'Inventory unavailable.'} retry={() => dispatch(loadInventory())} />
      </>
    );
  }
  return (
    <>
      <PageHeading eyebrow="ASSET MANAGEMENT" title="Inventory" description="Current availability and holder for every tracked asset and pooled item." />
      {status === 'failed' && <ErrorState message={error ?? 'Inventory unavailable.'} retry={() => dispatch(loadInventory())} />}
      {data && (data.equipment.length > 0 || data.pooledStock.length > 0) ? (
        <section className="panel table-panel">
          <div className="table-scroll"><table>
            <thead><tr><th scope="col">Equipment</th><th scope="col">Type</th><th scope="col">Availability / holder</th><th scope="col">Expected back</th><th scope="col">Condition note</th></tr></thead>
            <tbody>
              {data.equipment.map((item) => <tr key={item.id}>
                <td><strong>{item.name}</strong><small className="cell-subtitle">Asset ID · {item.id.slice(0, 8)}</small></td>
                <td>{item.category}</td>
                <td><span className={`badge ${item.isAvailable ? 'badge-green' : item.isOverdue ? 'badge-red' : 'badge-neutral'}`}>{item.isAvailable ? 'Available' : item.isOverdue ? `Overdue · ${item.currentHolder}` : `With ${item.currentHolder}`}</span></td>
                <td>{formatDate(item.expectedReturnAt)}</td><td>{item.latestDamageDescription ?? <span className="muted">No reports</span>}</td>
              </tr>)}
              {data.pooledStock.map((item) => <tr key={item.id}>
                <td><strong>{item.name}</strong><small className="cell-subtitle">Pooled stock · {item.id.slice(0, 8)}</small></td>
                <td>Quantity tracked</td>
                <td><strong>{item.availableQuantity}</strong> / {item.totalQuantity} available{item.overdueQuantity > 0 && <span className="badge badge-red inline-badge">{item.overdueQuantity} overdue</span>}</td>
                <td>—</td><td>{item.latestDamageDescription ?? <span className="muted">No reports</span>}</td>
              </tr>)}
            </tbody>
          </table></div>
        </section>
      ) : status === 'succeeded' ? <EmptyState title="Inventory is empty" detail="Register equipment to start managing stock." /> : null}
    </>
  );
}
