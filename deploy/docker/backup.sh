#!/usr/bin/env bash
# ============================================================================
#  Fino — nightly backup for the Docker stack.
#
#  Backs up the database, the uploaded files and .env (the keys), then prunes
#  anything older than KEEP_DAYS.
#
#      crontab -e
#      0 2 * * *  /opt/fino/deploy/docker/backup.sh >> /var/log/fino-backup.log 2>&1
#
#  A backup that has never been restored is a guess. Restore one into a spare
#  database once, and write down how long it took.
# ============================================================================
set -euo pipefail

cd "$(dirname "$0")"

BACKUP_ROOT="${BACKUP_ROOT:-/var/backups/fino}"
KEEP_DAYS="${KEEP_DAYS:-14}"

# shellcheck disable=SC1091
set -a; . ./.env; set +a

STAMP=$(date +%Y%m%d-%H%M%S)
TARGET="$BACKUP_ROOT/$STAMP"
mkdir -p "$TARGET"

echo "Fino backup -> $TARGET"

# --- database -------------------------------------------------------------
# BACKUP DATABASE writes inside the db container, so the file is produced there
# and copied out afterwards.
docker compose exec -T db /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C -b -Q \
    "BACKUP DATABASE [$DB_NAME] TO DISK = N'/var/opt/mssql/backup.bak' WITH COPY_ONLY, INIT, NAME = N'Fino automated backup';"

docker compose cp db:/var/opt/mssql/backup.bak "$TARGET/$DB_NAME-$STAMP.bak"
docker compose exec -T db rm -f /var/opt/mssql/backup.bak
echo "  database: $(du -h "$TARGET/$DB_NAME-$STAMP.bak" | cut -f1)"

# --- uploaded files (KYC documents, icons) --------------------------------
docker run --rm \
    -v "$(docker volume ls -q -f name=uploads | head -1)":/data:ro \
    -v "$TARGET":/backup \
    alpine tar czf "/backup/uploads-$STAMP.tar.gz" -C /data . 2>/dev/null || \
    echo "  uploads: nothing to back up yet"

# --- keys -----------------------------------------------------------------
# Without .env a restored database is unusable: sessions and the panel's
# encrypted transport both depend on these values.
cp .env "$TARGET/env.backup"
chmod 600 "$TARGET/env.backup"
echo "  keys: .env"

# --- retention ------------------------------------------------------------
find "$BACKUP_ROOT" -maxdepth 1 -type d -name '20*' -mtime "+$KEEP_DAYS" -exec rm -rf {} + 2>/dev/null || true

echo "Backup complete: $TARGET"
echo "Copy it off this server — a backup on the same machine does not survive the machine."
