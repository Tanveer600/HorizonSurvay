import { FiAlertCircle, FiInbox, FiLoader } from 'react-icons/fi';

export function LoadingState({ label = 'Loading data…' }: { label?: string }) {
  return <div className="state-panel" role="status"><FiLoader className="spin" aria-hidden="true" /><span>{label}</span></div>;
}

export function ErrorState({ message, retry }: { message: string; retry: () => void }) {
  return (
    <div className="state-panel error-panel" role="alert">
      <FiAlertCircle aria-hidden="true" />
      <div><strong>We couldn’t load this information</strong><p>{message}</p></div>
      <button className="button button-secondary" type="button" onClick={retry}>Try again</button>
    </div>
  );
}

export function EmptyState({ title, detail }: { title: string; detail: string }) {
  return <div className="state-panel empty-panel"><FiInbox aria-hidden="true" /><div><strong>{title}</strong><p>{detail}</p></div></div>;
}
