# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first to leverage Docker layer caching on restore.
COPY PetHaven.Api/PetHaven.Api.csproj PetHaven.Api/
COPY PetHaven.Application/PetHaven.Application.csproj PetHaven.Application/
COPY PetHaven.Domain/PetHaven.Domain.csproj PetHaven.Domain/
COPY PetHaven.Infrastructure/PetHaven.Infrastructure.csproj PetHaven.Infrastructure/
COPY PetHaven.Persistence/PetHaven.Persistence.csproj PetHaven.Persistence/
RUN dotnet restore PetHaven.Api/PetHaven.Api.csproj

# Copy the rest of the source and publish.
COPY . .
RUN dotnet publish PetHaven.Api/PetHaven.Api.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl fontconfig fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

EXPOSE 8080
ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
