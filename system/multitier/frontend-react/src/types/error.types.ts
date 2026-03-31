// Error handling types for API responses

export interface ValidationError {
  field: string;
  message: string;
}

export interface ProblemDetail {
  type?: string;
  title?: string;
  status: number;
  detail?: string;
  errors?: ValidationError[];
  timestamp?: string;
}

export interface ApiError {
  message: string;
  fieldErrors?: string[];
  status?: number;
}
