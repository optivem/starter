import { Link } from 'react-router-dom';

export interface FeatureCardProps {
  icon: string;
  title: string;
  description: string;
  linkTo: string;
  linkText: string;
}

/**
 * Feature card component for displaying navigation cards on the home page
 * Provides a consistent layout for feature links with icon, title, description, and call-to-action
 */
export function FeatureCard({ icon, title, description, linkTo, linkText }: FeatureCardProps) {
  return (
    <div className="col-md-4 mb-3">
      <div className="card h-100">
        <div className="card-body">
          <h5 className="card-title">{icon} {title}</h5>
          <p className="card-text">{description}</p>
          <Link to={linkTo} className="btn btn-primary">
            {linkText}
          </Link>
        </div>
      </div>
    </div>
  );
}
