interface LoadingSpinnerProps {
  message?: string;
  size?: 'sm' | 'md' | 'lg';
}

/**
 * Loading spinner component for displaying loading states
 * @param message - Optional loading message to display
 * @param size - Size variant (sm, md, lg)
 */
export function LoadingSpinner({ message = 'Loading...', size = 'md' }: LoadingSpinnerProps) {
  const sizeClass = size === 'sm' ? 'spinner-border-sm' : '';

  return (
    <div className="text-center py-5">
      <div className={`spinner-border text-primary ${sizeClass}`} role="status">
        <span className="visually-hidden">Loading...</span>
      </div>
      {message && <p className="mt-3">{message}</p>}
    </div>
  );
}
