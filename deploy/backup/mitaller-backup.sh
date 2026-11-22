#!/usr/bin/env bash
# Usage: mitaller-backup.sh full|log|verify
# Each restic snapshot holds the whole current chain (full + its logs), so any snapshot restores on its own.
set -euo pipefail

source /etc/mitaller/backup.env
export RESTIC_REPOSITORY RESTIC_PASSWORD_FILE AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY
export SQLCMDPASSWORD="$(cat /etc/mitaller/sa.secret)"

DB="${DB:-M_T_DB}"
ROOT=/var/opt/mssql/backup/chain
LOCK=/run/mitaller-backup.lock
SQLCMD=/opt/mssql-tools18/bin/sqlcmd
MODE="${1:?full|log|verify}"
STAMP="$(date -u +%Y%m%d-%H%M%S)"

exec 9>"$LOCK"
flock -w 600 9

ping_hc() { [ -n "${HC_URL:-}" ] && curl -fsS -m 10 --retry 3 -o /dev/null "$HC_URL/$1" || true; }
fail() { echo "ERROR: $*" >&2; ping_hc fail; exit 1; }
trap 'fail "linea $LINENO"' ERR

sql() { "$SQLCMD" -S 127.0.0.1 -U sa -C -b -h -1 -W -Q "SET NOCOUNT ON; $1" </dev/null; }

ensure_repo() {
  restic cat config >/dev/null 2>&1 || restic init
}

new_chain() {
  local dir="$ROOT/$STAMP"
  install -d -o mssql -g mssql -m 750 "$dir"
  sql "BACKUP DATABASE [$DB] TO DISK='$dir/00-full.bak' WITH INIT, CHECKSUM"
  ln -sfn "$dir" "$ROOT/current"
}

upload() {
  local dir; dir="$(readlink -f "$ROOT/current")"
  restic backup --quiet --tag "$1" --host mitaller-db "$dir" /etc/mitaller /etc/caddy
}

case "$MODE" in
  full)
    ping_hc start
    install -d -o mssql -g mssql -m 750 "$ROOT"
    ensure_repo
    new_chain
    upload full
    # keep the two newest chains on disk for a quick local restore
    ls -1d "$ROOT"/2* | sort | head -n -2 | xargs -r rm -rf
    restic forget --quiet --host mitaller-db --keep-last 96 --keep-daily 7 --keep-weekly 4 --keep-monthly 12 --prune
    ping_hc ""
    ;;
  log)
    [ -L "$ROOT/current" ] || exec "$0" full
    dir="$(readlink -f "$ROOT/current")"
    sql "BACKUP LOG [$DB] TO DISK='$dir/$STAMP-log.trn' WITH INIT, CHECKSUM"
    upload log
    ;;
  verify)
    ping_hc start
    restic check --read-data-subset=10%
    /usr/local/bin/mitaller-restore.sh latest "${DB}_verify" >/dev/null
    users="$(sql "SELECT COUNT(*) FROM [${DB}_verify].dbo.AspNetUsers")"
    sql "DBCC CHECKDB ([${DB}_verify]) WITH NO_INFOMSGS"
    sql "ALTER DATABASE [${DB}_verify] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${DB}_verify]"
    [ "${users// /}" -gt 0 ] || fail "la copia restaurada no tiene usuarios"
    echo "verify OK: $users usuarios"
    ping_hc ""
    ;;
  *) fail "modo desconocido: $MODE" ;;
esac
