#!/usr/bin/env bash
# ============================================================================
#  Fino — installer for a Linux server with Docker.
#
#      cd deploy/docker && ./install.sh
#
#  Asks a short list of questions, generates this installation's own secrets,
#  writes .env and panel-config.json, then builds and starts the stack. The
#  application creates its own database on first start.
#
#  Re-running it on a server that already has a .env keeps the existing keys
#  (changing them logs everyone out and breaks the panel's transport) and only
#  rebuilds and restarts.
# ============================================================================
set -euo pipefail

cd "$(dirname "$0")"

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; CYAN='\033[0;36m'; NC='\033[0m'
ok()   { echo -e "  ${GREEN}[OK]${NC}   $1"; }
warn() { echo -e "  ${YELLOW}[!]${NC}    $1"; }
fail() { echo -e "  ${RED}[FAIL]${NC} $1"; }
step() { echo -e "\n${CYAN}=== $1${NC}"; }

ask() { # ask <prompt> <default>
    local prompt="$1" default="${2:-}" answer
    if [ -n "$default" ]; then
        read -rp "$prompt [$default]: " answer
        echo "${answer:-$default}"
    else
        read -rp "$prompt: " answer
        echo "$answer"
    fi
}

ask_secret() { # ask_secret <prompt>
    local answer
    read -rsp "$1: " answer
    echo >&2
    echo "$answer"
}

# 32/16/64-character alphanumeric secrets. These land in JSON, in environment
# variables and (the AES key) in a browser-readable config file, so keeping them
# alphanumeric avoids a quoting problem somewhere in that chain.
gen() { LC_ALL=C tr -dc 'A-Za-z0-9' < /dev/urandom | head -c "$1"; }

echo
echo "  ============================================================"
echo "    Fino  -  installation (Docker)"
echo "  ============================================================"

step "Checking the server"

if ! command -v docker >/dev/null 2>&1; then
    fail "Docker is not installed."
    echo "        Install it with:  curl -fsSL https://get.docker.com | sh"
    exit 1
fi
if ! docker compose version >/dev/null 2>&1; then
    fail "The Docker Compose plugin is missing (docker compose)."
    echo "        On Debian/Ubuntu:  apt-get install docker-compose-plugin"
    exit 1
fi
if ! docker info >/dev/null 2>&1; then
    fail "Cannot talk to the Docker daemon — run this with sudo, or add yourself to the docker group."
    exit 1
fi
ok "Docker is available"

total_mem_mb=$(awk '/MemTotal/ {print int($2/1024)}' /proc/meminfo 2>/dev/null || echo 0)
if [ "$total_mem_mb" -gt 0 ] && [ "$total_mem_mb" -lt 3500 ]; then
    warn "This machine has ${total_mem_mb} MB of RAM. SQL Server alone wants 2 GB; 4 GB total is the realistic minimum."
fi

# ---------------------------------------------------------------------------
# Questions (skipped for values already in .env)
# ---------------------------------------------------------------------------
if [ -f .env ]; then
    step "Existing installation found"
    ok "Keeping the keys and passwords already in .env"
    # shellcheck disable=SC1091
    set -a; . ./.env; set +a
    REUSING=1
else
    REUSING=0
    step "A few questions"
    echo "  (press Enter to accept the value in brackets)"
    echo

    BRAND_NAME=$(ask "Company / brand name shown in the panel" "Fino")
    DOMAIN=$(ask "Domain for the panel (e.g. panel.client.com)")
    while [ -z "$DOMAIN" ]; do
        warn "A domain is required — HTTPS is issued for it automatically."
        DOMAIN=$(ask "Domain for the panel")
    done
    DB_NAME=$(ask "Database name" "FinoApp")
    ADMIN_MOBILE=$(ask "First administrator's mobile number")
    ADMIN_EMAIL=$(ask "First administrator's email" "admin@${DOMAIN}")
    ADMIN_NAME=$(ask "First administrator's name" "Administrator")

    while true; do
        ADMIN_PASSWORD=$(ask_secret "First administrator's password (min 8 chars)")
        ADMIN_PASSWORD2=$(ask_secret "Password again")
        [ "$ADMIN_PASSWORD" = "$ADMIN_PASSWORD2" ] || { warn "The two passwords do not match."; continue; }
        [ ${#ADMIN_PASSWORD} -ge 8 ] || { warn "Too short — use at least 8 characters."; continue; }
        break
    done

    step "Generating this installation's keys"
    SA_PASSWORD="$(gen 24)aA1#"     # SQL Server rejects a password without mixed classes
    JWT_SECRET=$(gen 64)
    AES_KEY=$(gen 32)
    AES_IV=$(gen 16)
    ok "New database password, JWT secret and AES key/IV created"

    umask 077
    cat > .env <<EOF
# Fino — installation settings. Generated $(date -Iseconds).
#
# These values belong to THIS client only. Never copy this file to another
# installation: the AES key and JWT secret are what keep one client's sessions
# from being valid on another's server.
#
# Back this file up. Without it a restored database cannot be used.

BRAND_NAME=$BRAND_NAME
DOMAIN=$DOMAIN
DB_NAME=$DB_NAME

SA_PASSWORD=$SA_PASSWORD
JWT_SECRET=$JWT_SECRET
AES_KEY=$AES_KEY
AES_IV=$AES_IV

ADMIN_NAME=$ADMIN_NAME
ADMIN_MOBILE=$ADMIN_MOBILE
ADMIN_EMAIL=$ADMIN_EMAIL
# Cleared automatically once the first start has used it.
ADMIN_PASSWORD=$ADMIN_PASSWORD

# Express is free and limited to 10 GB per database. Switch to Standard (with a
# licence) when the ledger outgrows it.
MSSQL_PID=Express
FINO_VERSION=latest
EOF
    chmod 600 .env
    ok ".env written (readable only by you)"
fi

# ---------------------------------------------------------------------------
# Panel branding — read by the browser at load time, no rebuild needed.
# ---------------------------------------------------------------------------
cat > panel-config.json <<EOF
{
  "appName": "${BRAND_NAME}",
  "tagline": "Console",
  "logoUrl": "",
  "theme": "azure",
  "loginHeadline": "The control room for your ${BRAND_NAME} network.",
  "loginSubtext": "Recharges, money transfer, AEPS, wallets and settlements — one secure console for every role in your distribution chain.",
  "apiUrl": "/api/",
  "encKey": "${AES_KEY}",
  "encIv": "${AES_IV}"
}
EOF
ok "Panel branding written"

# ---------------------------------------------------------------------------
# Build and start
# ---------------------------------------------------------------------------
step "Building the application (first time takes a few minutes)"
docker compose build

step "Starting"
docker compose up -d

step "Waiting for the database to be created and seeded"
echo "  The first start builds the whole schema; 3-5 minutes is normal."

deadline=$(( $(date +%s) + 900 ))
healthy=0
while [ "$(date +%s)" -lt "$deadline" ]; do
    if docker compose exec -T app curl -fsS http://localhost:8080/health >/dev/null 2>&1; then
        healthy=1
        break
    fi
    if [ "$(docker compose ps -q app | xargs -r docker inspect -f '{{.State.Running}}' 2>/dev/null)" = "false" ]; then
        fail "The application container stopped."
        echo "  Logs:"
        docker compose logs --tail 40 app
        exit 1
    fi
    printf '.'
    sleep 10
done
echo

if [ "$healthy" -ne 1 ]; then
    fail "The application did not report healthy in time."
    docker compose logs --tail 60 app
    exit 1
fi
ok "Application is up and the database is ready"

# The first-login password has served its purpose; leaving it in a file on disk has not.
if [ "$REUSING" -eq 0 ]; then
    sed -i 's/^ADMIN_PASSWORD=.*/ADMIN_PASSWORD=/' .env
    docker compose up -d --force-recreate app >/dev/null
    ok "First-login password removed from .env"
fi

cat > INSTALLATION-SUMMARY.txt <<EOF
${BRAND_NAME} — installation summary
Installed $(date -Iseconds) on $(hostname)

  Panel         : https://${DOMAIN}
  Health check  : https://${DOMAIN}/health

  First login   : mobile ${ADMIN_MOBILE}
                  password: the one typed during installation
                  (the panel asks for a new password on the first login)

  Database      : ${DB_NAME} in the "db" container (volume fino_db-data)
  Keys          : deploy/docker/.env  — BACK THIS FILE UP

  Daily backup  : ./backup.sh  (add to cron)
  Logs          : docker compose logs -f app
  Restart       : docker compose restart app
  Upgrade       : git pull && docker compose build && docker compose up -d
EOF

echo
echo -e "  ${GREEN}============================================================${NC}"
echo -e "  ${GREEN}  Installation complete${NC}"
echo -e "  ${GREEN}============================================================${NC}"
cat INSTALLATION-SUMMARY.txt
echo
warn "Open the panel and change the administrator password now."
warn "Back up deploy/docker/.env — the database alone is not enough to restore this installation."
