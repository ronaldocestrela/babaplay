# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### 🎯 Features

- Added support for web login via subdomain (`login-web`). Tenant inference from host.
- Maintained existing mobile/header login (`login`) for backward compatibility.
- Configurable `Tenancy:HostTemplate` for subdomain strategy (supports dev and prod).
- Updated OpenAPI docs with dual login endpoints.

### 🛠️ Improvements

- Added wildcard subdomain CORS support.
- Diagnostic logging during development to trace tenant resolution (removed before release).
- Cleaned up middleware and startup configuration for multi-tenancy.

### ✅ Documentation

- Updated README with new login instructions.
- Created frontend guidance file (`FRONTEND_LOGIN.md`).

### 🐛 Fixes

- Corrected host template syntax (removed braces around placeholder).
- Order of multi-tenant strategy registration fixed.


---

## [0.1.0] - 2026-03-03

Initial release (placeholder).