import { useEffect } from 'react';
import { Link, NavLink, Outlet, Route, Routes } from 'react-router-dom';
import {
  FiActivity,
  FiBox,
  FiCheckCircle,
  FiGrid,
  FiPlus,
  FiRefreshCw,
  FiTag,
  FiTruck,
  FiX,
} from 'react-icons/fi';
import { useAppDispatch, useAppSelector } from './hooks';
import { loadOpenCheckouts } from './features/checkouts/checkoutsSlice';
import { loadInventory } from './features/inventory/inventorySlice';
import { loadCategories } from './features/categories/categoriesSlice';
import { clearToast } from './features/ui/uiSlice';
import DashboardPage from './pages/DashboardPage';
import InventoryPage from './pages/InventoryPage';
import CheckoutsPage from './pages/CheckoutsPage';
import ApprovalsPage from './pages/ApprovalsPage';
import EquipmentPage from './pages/EquipmentPage';
import HistoryPage from './pages/HistoryPage';
import CategoriesPage from './pages/CategoriesPage';

const navigation = [
  { to: '/', label: 'Overview', icon: FiGrid, end: true },
  { to: '/inventory', label: 'Inventory', icon: FiBox },
  { to: '/checkouts', label: 'Checkouts', icon: FiTruck },
  { to: '/approvals', label: 'Drone approval', icon: FiCheckCircle },
  { to: '/equipment/new', label: 'Register equipment', icon: FiPlus },
  { to: '/categories', label: 'Categories', icon: FiTag },
  { to: '/history', label: 'Movement history', icon: FiActivity },
];

function ToastHost() {
  const toast = useAppSelector((state) => state.ui.toast);
  const dispatch = useAppDispatch();

  useEffect(() => {
    if (!toast) return undefined;
    const timer = window.setTimeout(() => dispatch(clearToast()), 5000);
    return () => window.clearTimeout(timer);
  }, [dispatch, toast]);

  if (!toast) return null;
  return (
    <div className={`toast toast-${toast.tone}`} role={toast.tone === 'error' ? 'alert' : 'status'}>
      <span>{toast.message}</span>
      <button type="button" className="icon-button" aria-label="Dismiss notification" onClick={() => dispatch(clearToast())}>
        <FiX aria-hidden="true" />
      </button>
    </div>
  );
}

function Shell() {
  const dispatch = useAppDispatch();
  const openCount = useAppSelector((state) => state.checkouts.open.length);
  const inventoryStatus = useAppSelector((state) => state.inventory.status);
  const connectionLabel = inventoryStatus === 'succeeded'
    ? 'Inventory API connected'
    : inventoryStatus === 'failed'
      ? 'API unavailable'
      : inventoryStatus === 'loading'
        ? 'Connecting to API'
        : 'Checking API';

  useEffect(() => {
    dispatch(loadInventory());
    dispatch(loadOpenCheckouts());
    dispatch(loadCategories());
  }, [dispatch]);

  return (
    <div className="app-shell">
      <aside className="sidebar" aria-label="Main navigation">
        <Link className="brand" to="/" aria-label="Horizon Survey home">
          <span className="brand-mark">H</span>
          <span className="brand-copy"><strong>Horizon</strong><small>SURVEY SERVICES</small></span>
        </Link>
        <div className="nav-caption">WORKSPACE</div>
        <nav className="nav-list">
          {navigation.map(({ to, label, icon: Icon, end }) => (
            <NavLink key={to} to={to} end={end} className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}>
              <Icon aria-hidden="true" />
              <span>{label}</span>
              {label === 'Checkouts' && <span className="nav-count">{openCount}</span>}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-foot">
          <span className={`status-dot${inventoryStatus === 'failed' ? ' offline' : ''}`} />
          <span>{connectionLabel}</span>
        </div>
      </aside>
      <main className="main-content">
        <header className="topbar">
          <div className="mobile-brand"><span className="brand-mark">H</span><strong>Horizon Survey</strong></div>
          <div className="topbar-status"><span className={`status-dot${inventoryStatus === 'failed' ? ' offline' : ''}`} /> {inventoryStatus === 'succeeded' ? 'Live inventory' : connectionLabel}</div>
          <button type="button" className="refresh-button" onClick={() => {
            dispatch(loadInventory());
            dispatch(loadOpenCheckouts());
          }}>
            <FiRefreshCw aria-hidden="true" /> Refresh
          </button>
        </header>
        <section className="page-content"><Outlet /></section>
      </main>
      <ToastHost />
      <nav className="mobile-nav" aria-label="Mobile navigation">
        {navigation.filter(({ to }) => ['/', '/inventory', '/checkouts', '/categories'].includes(to)).map(({ to, label, icon: Icon, end }) => (
          <NavLink key={to} to={to} end={end} aria-label={label} className={({ isActive }) => `mobile-nav-link${isActive ? ' active' : ''}`}>
            <Icon aria-hidden="true" />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  );
}

export default function App() {
  return (
    <Routes>
      <Route element={<Shell />}>
        <Route index element={<DashboardPage />} />
        <Route path="inventory" element={<InventoryPage />} />
        <Route path="checkouts" element={<CheckoutsPage />} />
        <Route path="approvals" element={<ApprovalsPage />} />
        <Route path="equipment/new" element={<EquipmentPage />} />
        <Route path="categories" element={<CategoriesPage />} />
        <Route path="history" element={<HistoryPage />} />
        <Route path="*" element={<DashboardPage />} />
      </Route>
    </Routes>
  );
}
