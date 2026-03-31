interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
}

/**
 * Error message component with optional retry functionality
 * @param message - Error message to display
 * @param onRetry - Optional callback function to retry the failed operation
 */
export function ErrorMessage({ message, onRetry }: ErrorMessageProps) {
  return (
    <div className="alert alert-danger d-flex justify-content-between align-items-center" role="alert">
      <div>
        <strong>Error:</strong> {message}
      </div>
      {onRetry && (
        <button className="btn btn-sm btn-outline-danger" onClick={onRetry}>
          Try Again
        </button>
      )}
    </div>
  );
}
