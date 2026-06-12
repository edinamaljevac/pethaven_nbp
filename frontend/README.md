# PetHaven Frontend

React + Vite frontend for the PetHaven ASP.NET Core backend.

## Stack

- React
- Vite
- React Router
- Axios
- Tailwind CSS
- Lucide icons

## Setup

Create `.env` or edit the existing file:

```text
VITE_API_BASE_URL=https://localhost:5001/api/v1
```

Install and run:

```bash
npm install
npm run dev
```

Production build:

```bash
npm run build
```

## Feature Map

- `auth`: login, register, JWT local session
- `animals`: listing, filters, create animal
- `shelters`: listing, coordinates/radius filters, create shelter
- `adoptions`: applications, status updates, contracts, post-adoption reports
- `fosters`: profiles, assignments, reports
- `lostFound`: reports and matching
- `donations`: goals and mock payments
- `volunteers`: applications and approval
- `notifications`: user notifications and saved filters
- `admin`: statistics and moderation
- `files`: uploads and media gallery attachment