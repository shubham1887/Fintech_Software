# syntax=docker/dockerfile:1
# ============================================================================
#  Fino — one image containing the API and the operator console.
#
#  The panel is built and copied into wwwroot/panel, and the API serves it
#  (Panel:ServeStaticFiles). One image, one port, one origin: no CORS to get
#  wrong and no second web server to keep in step.
#
#  Build from the REPOSITORY ROOT, because the API references sibling projects:
#      docker build -f deploy/docker/Dockerfile -t fino:latest .
# ============================================================================

# ---------------------------------------------------------------------------
# 1. Operator console
# ---------------------------------------------------------------------------
FROM node:22-alpine AS web
WORKDIR /src

COPY Fino_Web/package.json Fino_Web/package-lock.json ./
RUN npm ci

COPY Fino_Web/ ./
# Same-origin default; the runtime config.json still wins if a deployment sets it.
ENV VITE_API_URL=/api/
RUN npm run build

# ---------------------------------------------------------------------------
# 2. API
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api
WORKDIR /src

# Restore first, as its own layer: dependencies change far less often than code, so a code-only
# rebuild skips the slowest step.
COPY Fino_API/FintechApp.csproj        Fino_API/
COPY Fino.PlanApi/Fino.PlanApi.csproj  Fino.PlanApi/
COPY Fino.JaiJuiApi/Fino.JaiJuiApi.csproj Fino.JaiJuiApi/
COPY Fino.Pay1Api/Fino.Pay1Api.csproj  Fino.Pay1Api/
RUN dotnet restore Fino_API/FintechApp.csproj

COPY Fino_API/       Fino_API/
COPY Fino.PlanApi/   Fino.PlanApi/
COPY Fino.JaiJuiApi/ Fino.JaiJuiApi/
COPY Fino.Pay1Api/   Fino.Pay1Api/
RUN dotnet publish Fino_API/FintechApp.csproj -c Release -o /app --no-restore -p:DebugType=none

# ---------------------------------------------------------------------------
# 3. Runtime
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# curl is here for the container healthcheck below, and for support to test from inside.
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

COPY --from=api /app ./
COPY --from=web /src/dist ./wwwroot/panel

# Uploaded KYC documents and icons must outlive the container — compose mounts a volume here.
RUN mkdir -p /app/wwwroot/uploads

# Not root. A payments API that is compromised should not also own the filesystem.
RUN useradd --uid 10001 --create-home --shell /usr/sbin/nologin fino \
 && chown -R fino:fino /app
USER fino

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_gcServer=1

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=300s --retries=5 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FintechApp.dll"]
