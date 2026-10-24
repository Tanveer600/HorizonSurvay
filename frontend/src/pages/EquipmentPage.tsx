import { FormEvent, useState } from 'react';
import { FiPlus, FiSettings } from 'react-icons/fi';
import { Link } from 'react-router-dom';
import PageHeading from '../components/PageHeading';
import { loadCategories } from '../features/categories/categoriesSlice';
import { registerEquipment } from '../features/equipment/equipmentSlice';
import { useAppDispatch, useAppSelector } from '../hooks';

export default function EquipmentPage() {
  const dispatch = useAppDispatch();
  const registration = useAppSelector((state) => state.equipment);
  const categories = useAppSelector((state) => state.categories);
  const [name, setName] = useState('');
  const [categoryId, setCategoryId] = useState('');

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    dispatch(registerEquipment({ name: name.trim(), categoryId }));
  }

  return (
    <>
      <PageHeading eyebrow="INVENTORY SETUP" title="Register equipment" description="Add an individually tracked asset to the live Horizon inventory."
        action={<Link className="button button-secondary heading-action" to="/categories"><FiSettings aria-hidden="true" /> Manage categories</Link>} />
      <section className="panel form-panel narrow-panel">
        <div className="panel-heading"><div><span className="eyebrow">NEW ASSET</span><h2>Equipment details</h2></div><span className="form-icon"><FiPlus aria-hidden="true" /></span></div>
        {registration.error && <div className="inline-error" role="alert">{registration.error}</div>}
        {registration.registeredName && <div className="receipt-box" role="status"><strong>Equipment registered</strong><span>{registration.registeredName} is available in inventory.</span></div>}
        {categories.status === 'failed' && <div className="inline-error" role="alert">{categories.error} <button className="text-link" type="button" onClick={() => dispatch(loadCategories())}>Retry category load</button></div>}
        <form className="stack-form" onSubmit={submit}>
          <label>Asset name<input autoFocus required maxLength={200} value={name} onChange={(event) => setName(event.target.value)} placeholder="e.g. Rover 2" /></label>
          <label>Category<select required value={categoryId} onChange={(event) => setCategoryId(event.target.value)} disabled={categories.status !== 'succeeded' || categories.categories.length === 0}>
            <option value="">{categories.status === 'loading' ? 'Loading categories…' : categories.categories.length === 0 ? 'No categories available' : 'Select a category'}</option>
            {categories.categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select></label>
          {categories.status === 'succeeded' && categories.categories.length === 0 && <p className="form-footnote">Create a category before registering equipment.</p>}
          <p className="form-footnote">Quantity-tracked stock appears in Inventory and can be issued from Checkouts.</p>
          <button className="button button-primary button-wide" type="submit" disabled={registration.status === 'loading' || categories.status !== 'succeeded' || categories.categories.length === 0}><FiPlus aria-hidden="true" /> Add to inventory</button>
        </form>
      </section>
    </>
  );
}
