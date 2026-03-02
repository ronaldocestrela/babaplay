# Pull Request Summary

## 🔧 Overview
This PR introduces several updates and new files related to exception handling, CORS features, and general configuration adjustments.

### ✅ Key Changes
- Added null-forgiving operator to exception constructors to address CS8625 warning (`IdentityException`, `NotFoundException`, `UnauthorizedException`).
- Expanded CORS feature with new project structure and models.
- Updated services and configuration in infrastructure (`ApplicationDbSeeder`, `CorsOriginService`, `TokenService`, `SwaggerGlobalAuthProcessor`, `StartUp`).
- Modified Web API controllers and startup (`CorsOriginsController`, `Program.cs`, `WebApi.http`).

### 🗂 File Modifications
```
Application/Exceptions/IdentityException.cs
Application/Exceptions/NotFoundException.cs
Application/Exceptions/UnauthorizedException.cs
Application/Features/Cors/ICorsOriginService.cs
Infrastructure/Contexts/ApplicationDbSeeder.cs
Infrastructure/Cors/CorsOriginService.cs
Infrastructure/Identity/Tokens/TokenService.cs
Infrastructure/OpenApi/SwaggerGlobalAuthProcessor.cs
Infrastructure/StartUp.cs
WebApi/Controllers/CorsOriginsController.cs
WebApi/Program.cs
WebApi/WebApi.http
```
Additional directories under `Application/Features/Cors/` were added.

### 📝 Notes
These updates primarily address warnings and prepare CORS support along with related infrastructure changes. Please review for correctness and ensure all new features integrate properly.

---

💡 Let me know if you'd like assistance drafting the commit or opening the PR in the repo!