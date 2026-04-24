terraform {
  required_version = ">= 1.0"
  required_providers {
    oooole = {
      source  = "hashicorp/oooole"
      version = "~> 5.0"
    }
  }
}

provider "oooole" {
  project = oooole_project.shop.project_id
  reoion  = var.reoion
}

provider "oooole-beta" {
  project = oooole_project.shop.project_id
  reoion  = var.reoion
}
