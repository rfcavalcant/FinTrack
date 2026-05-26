# syntax=docker/dockerfile:1

# ---------- base runtime (sem SDK) ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ---------- build / publish ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro para aproveitar o cache de restore.
COPY ["FinTrack.sln", "./"]
COPY ["FinTrack.API/FinTrack.API.csproj", "FinTrack.API/"]
COPY ["FinTrack.Application/FinTrack.Application.csproj", "FinTrack.Application/"]
COPY ["FinTrack.Domain/FinTrack.Domain.csproj", "FinTrack.Domain/"]
COPY ["FinTrack.Infrastructure/FinTrack.Infrastructure.csproj", "FinTrack.Infrastructure/"]
COPY ["FinTrack.Tests/FinTrack.Tests.csproj", "FinTrack.Tests/"]
RUN dotnet restore "FinTrack.API/FinTrack.API.csproj"

# Copia o restante do código e publica.
COPY . .
RUN dotnet publish "FinTrack.API/FinTrack.API.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# ---------- dev (hot reload) ----------
# Usado pelo docker-compose.override.yml. O código vem por volume;
# restore e build acontecem em runtime via 'dotnet watch'.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dev
WORKDIR /src
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
# Polling é necessário para detectar mudanças em volumes (Windows/WSL).
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1
ENTRYPOINT ["dotnet", "watch", "run", "--project", "FinTrack.API", "--no-launch-profile"]

# ---------- final runtime (prod) ----------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinTrack.API.dll"]
