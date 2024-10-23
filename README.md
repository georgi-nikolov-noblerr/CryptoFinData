# CryptoFinData - Cryptocurrency Price Tracking Dashboard

A full-stack application for tracking cryptocurrency prices using the CoinDesk API. The application features a secure authentication system, price updates, and historical price tracking with data visualization.

## Tech Stack

### Frontend
- Angular 18 (Latest)
- TypeScript
- TailwindCSS
- Chart.js
- Angular Signals for state management
- Standalone Components Architecture

### Backend
- .NET 8 (Latest)
- Entity Framework Core 8
- SQL Server (Azure SQL)
- Refit for HTTP client
- JWT Authentication

### Infrastructure
- Docker & Docker Compose
- Azure Static Web Apps (Frontend)
- Azure App Service (Backend API)
- Azure SQL Database

## Solution Architecture

```
CryptoFinData/
├── src/
│   ├── backend/                 # .NET 8 Backend
│   │   ├── CryptoFinData.API/           # API Layer
│   │   ├── CryptoFinData.Core/          # Domain Layer
│   │   ├── CryptoFinData.Infrastructure/# Infrastructure Layer
│   │   └── CryptoFinData.SharedKernel/  # Shared Components
│   │
│   └── client/                 # Angular Frontend
│       ├── src/
│       │   ├── app/
│       │   │   ├── components/     # Reusable Components
│       │   │   ├── features/       # Feature Components
│       │   │   └── core/          # Core Services & Models
│       │   └── environments/      # Environment Configurations
│       └── ...
│
└── docker/                    # Docker files
|    ├── backend.Dockerfile
|    ├── client.Dockerfile
└── docker-compose.yaml
```

## Running Locally

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK (for local development)
- Node.js 18+ (for local development)

### Using Docker

1. Clone the repository:
```bash
git clone https://github.com/yourusername/CryptoFinData.git
cd CryptoFinData
```

2. Create a `.env` file in the root directory:
```env
JWT_KEY=your-secret-key
JWT_ISSUER=cryptofindata
JWT_AUDIENCE=cryptofindata-api
```

3. Start the application:
```bash
docker-compose up --build
```

4. Access the application:
- Frontend: http://localhost:3242
- Backend API: http://localhost:5242
- Swagger Documentation: http://localhost:5242/swagger

##### Authentication
The application uses JWT-based authentication:
- Default credentials:
  - Username: **admin**
  - Password: **admin**

##### API Documentation
- OpenAPI (Swagger) documentation available at `/swagger`

### Local Development

1. Backend:
```bash
cd src/backend/CryptoFinData.API
dotnet restore
dotnet watch run
```

2. Frontend:
```bash
cd src/client
npm install
npm start
```

## Deployment

The application is deployed on Azure using the following services:

- Frontend (Azure Static Web Apps)
    - URL: https://gentle-dune-0c460ce1e.5.azurestaticapps.net
- Backend API (Azure App Service)
    - URL: https://cryptofindata-backend.azurewebsites.net
- Database (Azure SQL)

## Development Tools

### Backend
- Visual Studio 2022
- Visual Studio Code
- .NET CLI
- Entity Framework Core CLI

### Frontend
- Visual Studio Code
- Angular CLI
- Node.js & npm

## License

This project is licensed under the MIT License - see the LICENSE.md file for details.