# docker/backend.Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files needed for the API
COPY ["CryptoFinData.API/CryptoFinData.API.csproj", "CryptoFinData.API/"]
COPY ["CryptoFinData.Core/CryptoFinData.Core.csproj", "CryptoFinData.Core/"]
COPY ["CryptoFinData.Infrastructure/CryptoFinData.Infrastructure.csproj", "CryptoFinData.Infrastructure/"]
COPY ["CryptoFinData.SharedKernel/CryptoFinData.SharedKernel.csproj", "CryptoFinData.SharedKernel/"]

# Restore packages for API projects only
RUN dotnet restore "CryptoFinData.API/CryptoFinData.API.csproj"

# Copy the rest of the API source code
COPY CryptoFinData.API/. CryptoFinData.API/
COPY CryptoFinData.Core/. CryptoFinData.Core/
COPY CryptoFinData.Infrastructure/. CryptoFinData.Infrastructure/
COPY CryptoFinData.SharedKernel/. CryptoFinData.SharedKernel/

# Build and publish
WORKDIR "/src/CryptoFinData.API"
RUN dotnet build "CryptoFinData.API.csproj" -c Release -o /app/build
RUN dotnet publish "CryptoFinData.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CryptoFinData.API.dll"]