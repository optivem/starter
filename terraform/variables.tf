variable "region" {
  description = "GCP region for resources"
  type        = string
  default     = "us-central1"
}

variable "github_repo" {
  description = "GitHub repository in format 'owner/repo'"
  type        = string
}

variable "billing_account_id" {
  description = "GCP billing account ID"
  type        = string
}

variable "neon_database_url" {
  description = "Neon database connection string (optional)"
  type        = string
  default     = ""
  sensitive   = true
}
