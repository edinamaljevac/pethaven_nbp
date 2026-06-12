# PetHaven

PetHaven is a full-stack platform for animal adoption, shelter management, foster care, lost/found reports, donations, volunteering and notifications. It consists of an ASP.NET Core Web API and a React frontend.

## Architecture

The repository is split into a `backend/` folder (the .NET solution) and a `frontend/` folder (the React app). The backend follows Clean Architecture:

- `backend/PetHaven.Domain`: entities, enums and shared domain base classes.
- `backend/PetHaven.Application`: DTOs, CQRS commands/queries, MediatR handlers, FluentValidation, AutoMapper mappings and service abstractions.
- `backend/PetHaven.Persistence`: EF Core PostgreSQL DbContext, Repository pattern, Unit of Work and migrations.
- `backend/PetHaven.Infrastructure`: JWT, password hashing, file storage (local disk or Cloudinary), geo-location and adoption contract generation services.
- `backend/PetHaven.Api`: controllers, middleware, Serilog, Swagger/OpenAPI and authentication pipeline.
- `frontend`: feature-based React application using Vite, React Router, Axios and Tailwind CSS.

For hosting on free cloud services, see [`DEPLOYMENT.md`](DEPLOYMENT.md).

## Technical Requirements Covered

- Repository pattern and Unit of Work
- Clean Architecture
- CQRS with MediatR
- AutoMapper DTO mapping
- JWT access token and refresh-token rotation/revoke flow
- FluentValidation pipeline
- Serilog structured logging to console and rolling files
- Global RFC 7807 Problem Details error responses
- Swagger/OpenAPI with JWT security, response metadata and request examples
- API versioning through `/api/v1/...` routes
- Custom geo-location enrichment middleware via `HttpContext.Items["RequestGeo"]`
- PostgreSQL through EF Core and Npgsql
- EF Core migrations for the complete current model
- README with run instructions, architecture description and text ER overview

## Prerequisites

- .NET 8 SDK
- PostgreSQL
- Node.js and npm
- Optional: MaxMind `GeoLite2-City.mmdb` for IP-based geo-location enrichment

## Run Locally

All backend commands are run from the `backend/` folder.

1. Copy `backend/PetHaven.Api/appsettings.Development.example.json` to `backend/PetHaven.Api/appsettings.Development.json` and set local PostgreSQL/JWT values. The development file is ignored by Git.
2. Copy `frontend/.env.example` to `frontend/.env`.
3. Start the API (pending EF Core migrations are applied automatically on startup, so the schema is created for you):

```powershell
cd backend
dotnet run --project PetHaven.Api --launch-profile https
```

   To apply migrations manually instead, run:

```powershell
cd backend
dotnet ef database update --project PetHaven.Persistence --startup-project PetHaven.Api
```

5. Open Swagger in development:

```text
https://localhost:<port>/swagger
```

6. Configure the frontend API URL in `frontend/.env`:

```text
VITE_API_BASE_URL=https://localhost:7298/api/v1
```

7. Start the frontend:

```powershell
cd frontend
npm install
npm run dev
```

## Deployment Configuration

See [`DEPLOYMENT.md`](DEPLOYMENT.md) for a full free-tier hosting walkthrough (Neon, Render, Vercel, Cloudinary).

Do not commit production credentials. Configure these values through deployment environment variables or a secret manager:

```text
ConnectionStrings__DefaultConnection=<production PostgreSQL connection string>
JwtSettings__Secret=<strong random secret with at least 32 characters>
JwtSettings__Issuer=PetHaven
JwtSettings__Audience=PetHavenUsers
GeoLocation__DatabasePath=GeoIP/GeoLite2-City.mmdb
Cors__AllowedOrigins=<deployed frontend URL, e.g. https://your-app.vercel.app>
```

When the host's filesystem is ephemeral (most free tiers wipe uploads on restart), configure Cloudinary so uploaded images and videos are stored externally. When these are set the API uses Cloudinary automatically; otherwise it falls back to local disk:

```text
Cloudinary__CloudName=<cloud name>
Cloudinary__ApiKey=<api key>
Cloudinary__ApiSecret=<api secret>
```

Configure the frontend at build time:

```text
VITE_API_BASE_URL=https://your-api-domain.example/api/v1
```

To run the optional admin seed tool, provide credentials through environment variables:

```text
PETHAVEN_ADMIN_EMAIL=<admin email>
PETHAVEN_ADMIN_PASSWORD=<strong admin password>
ConnectionStrings__DefaultConnection=<database connection string>
```

Uploaded files, generated contracts, logs and the licensed GeoLite2 database are runtime data and are intentionally excluded from Git.

## Main API Areas

- `api/v1/auth`: registration, shelter verification registration, login, refresh token and revoke token
- `api/v1/animals`: role-based listing, search, intake-date sorting, photos, video and special-needs spotlight
- `api/v1/animals/{animalId}/care`: health record and behavior profile
- `api/v1/shelters`: shelter profiles, coordinates, Haversine filtering and admin verification
- `api/v1/profile`: role-based current-user profile
- `api/v1/adoptions`: applications, status workflow, appointments, contracts and post-adoption reports
- `api/v1/fosters`: foster profiles, capacity-protected assignments and foster reports
- `api/v1/lost-found`: role-based reports, potential matches, resolution and admin hide/restore moderation
- `api/v1/donations`: donation goals and mock donation payments
- `api/v1/volunteers`: role-based volunteer applications and approval workflow
- `api/v1/notifications`: adopter notifications for animal matches, adoption status changes and reminders
- `api/v1/notification-jobs`: optional saved-filter matching job endpoint
- `api/v1/saved-filters`: adopter saved animal search filters
- `api/v1/files`: multipart file upload (local disk or Cloudinary)
- `api/v1/media`: associates uploaded images with animal and shelter galleries
- `api/v1/admin`: verification requests, moderation and platform statistics

## Implementation Notes

- Contract generation creates real PDF documents with PdfSharpCore under `wwwroot/generated/contracts`.
- Multipart uploads persist metadata in `FileAssets`; the file itself is saved under `wwwroot/uploads` locally, or to Cloudinary when Cloudinary credentials are configured.
- Animal profiles support multiple photos, one main profile photo, video, health records and behavior profiles with special-needs information.
- Geo-location middleware uses MaxMind GeoLite2 City when available and falls back to geo headers or `Unknown`.
- Donation payment is a mock payment flow and completed goals no longer accept donations.
- Saved animal filters are automatically evaluated when an animal becomes available. Matching can also be run manually through the job endpoint.
- Foster assignments enforce profile capacity on the backend.
- Interview and home-visit dates are stored with adoption applications and included in adopter notifications.
- Lost & Found hidden reports are excluded from non-admin lists and potential matches.

## GeoLite2 Setup

Download `GeoLite2-City.mmdb` from MaxMind and place it in `backend/PetHaven.Api/GeoIP/`, or set another path in `backend/PetHaven.Api/appsettings.json`:

```json
"GeoLocation": {
  "DatabasePath": "GeoIP/GeoLite2-City.mmdb"
}
```

## Frontend Build

```powershell
cd frontend
npm run build
```

## Model Overview

Core relationships:

```text
User 1 --- 0..1 AdopterProfile
User 1 --- 0..1 ShelterProfile
User 1 --- 0..1 FosterProfile

ShelterProfile 1 --- N Animal
ShelterProfile 1 --- N ShelterPhoto
ShelterProfile 1 --- N DonationGoal
ShelterProfile 1 --- N Donation
ShelterProfile 1 --- N VolunteerApplication

Animal 1 --- N AnimalPhoto
Animal 1 --- 0..1 BehaviorProfile
Animal 1 --- 0..1 HealthRecord
Animal 1 --- N AdoptionApplication
Animal 1 --- N FosterAssignment

AdopterProfile 1 --- N AdoptionApplication
AdoptionApplication 1 --- 0..1 AdoptionContract
AdoptionApplication 1 --- N PostAdoptionReport

FosterProfile 1 --- N FosterAssignment
FosterAssignment 1 --- N FosterReport

User 1 --- N LostFoundReport
User 1 --- N Notification
User 1 --- N RefreshToken
User 1 --- N SavedAnimalSearchFilter
User 1 --- N LoginEvent
User 1 --- N VolunteerApplication
User 1 --- N ContentModeration

User 0..1 --- N Donation
User 0..1 --- N FileAsset
```

`Animal.VideoUrl` is stored directly on the `Animal` entity. `ContentModeration.TargetId` is a generic target reference selected through `TargetType`.
