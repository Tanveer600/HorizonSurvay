import { FormEvent, useEffect, useState } from 'react';
import { FiCheck, FiEdit2, FiPlus, FiTrash2, FiX } from 'react-icons/fi';
import { Link } from 'react-router-dom';
import { ErrorState, LoadingState } from '../components/AsyncState';
import PageHeading from '../components/PageHeading';
import {
  createCategory,
  deleteCategory,
  loadCategories,
  updateCategory,
} from '../features/categories/categoriesSlice';
import { useAppDispatch, useAppSelector } from '../hooks';

export default function CategoriesPage() {
  const dispatch = useAppDispatch();
  const state = useAppSelector((root) => root.categories);
  const [name, setName] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editingName, setEditingName] = useState('');

  useEffect(() => {
    if (state.mutationStatus === 'succeeded') {
      setName('');
      setEditingId(null);
    }
  }, [state.mutationStatus]);

  function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    dispatch(createCategory(name.trim()));
  }

  function saveEdit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (editingId) dispatch(updateCategory({ id: editingId, name: editingName.trim() }));
  }

  return (
    <>
      <PageHeading eyebrow="INVENTORY SETUP" title="Equipment categories" description="Maintain the categories available when registering equipment."
        action={<Link className="button button-secondary heading-action" to="/equipment/new">Register equipment</Link>} />
      <section className="panel form-panel narrow-panel">
        <div className="panel-heading"><div><span className="eyebrow">CATEGORY LIST</span><h2>Manage categories</h2></div></div>
        {state.mutationError && <div className="inline-error" role="alert">{state.mutationError}</div>}
        {state.status === 'loading' && <LoadingState label="Loading categories…" />}
        {state.status === 'failed' && <ErrorState message={state.error ?? 'Categories unavailable.'} retry={() => dispatch(loadCategories())} />}
        {state.status === 'succeeded' && (
          <div className="category-list">
            {state.categories.length === 0 && <p className="category-empty">No categories yet. Add one below.</p>}
            {state.categories.map((category) => (
              <div className="category-row" key={category.id}>
                {editingId === category.id ? (
                  <form className="category-edit-form" onSubmit={saveEdit}>
                    <label className="visually-hidden" htmlFor={`edit-category-${category.id}`}>Category name</label>
                    <input id={`edit-category-${category.id}`} required maxLength={100} value={editingName} onChange={(event) => setEditingName(event.target.value)} />
                    <button className="icon-button" type="submit" disabled={state.mutationStatus === 'loading'} aria-label={`Save ${category.name}`}><FiCheck /></button>
                    <button className="icon-button" type="button" aria-label="Cancel edit" onClick={() => setEditingId(null)}><FiX /></button>
                  </form>
                ) : (
                  <>
                    <strong>{category.name}</strong>
                    <div className="category-actions">
                      <button className="icon-button" type="button" aria-label={`Edit ${category.name}`} onClick={() => { setEditingId(category.id); setEditingName(category.name); }}><FiEdit2 aria-hidden="true" /></button>
                      <button className="icon-button" type="button" aria-label={`Delete ${category.name}`} disabled={state.mutationStatus === 'loading'} onClick={() => {
                        if (window.confirm(`Delete category “${category.name}”? Equipment will be kept and become uncategorized. Categories used by equipment with an open checkout cannot be deleted.`)) dispatch(deleteCategory(category.id));
                      }}><FiTrash2 aria-hidden="true" /></button>
                    </div>
                  </>
                )}
              </div>
            ))}
          </div>
        )}
        <form className="stack-form category-create-form" onSubmit={create}>
          <label>New category name<input required maxLength={100} value={name} onChange={(event) => setName(event.target.value)} placeholder="e.g. Total station" /></label>
          <button className="button button-primary" type="submit" disabled={state.mutationStatus === 'loading'}><FiPlus aria-hidden="true" /> Add category</button>
        </form>
      </section>
    </>
  );
}
