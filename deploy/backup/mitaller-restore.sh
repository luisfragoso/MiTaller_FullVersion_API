#!/usr/bin/env bash
# Usage: mitaller-restore.sh [snapshot=latest] [target_db=M_T_DB_restore]
# Restores the full backup plus every log in that snapshot. Restoring over M_T_DB needs the target name spelled out.
set -euo pipefail

source /etc/mitaller/backup.env
export RESTIC_REPOSITORY RESTIC_PASSWORD_FILE AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY
export SQLCMDPASSWORD="$(cat /etc/mitaller/sa.secret)"

SNAP="${1:-latest}"
TARGET="${2:-M_T_DB_restore}"
SQLCMD=/opt/mssql-tools18/bin/sqlcmd
WORK="$(mktemp -d /var/opt/mssql/backup/restore.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

sql() { "$SQLCMD" -S 127.0.0.1 -U sa -C -b -h -1 -W -Q "SET NOCOUNT ON; $1" </dev/null; }

restic restore "$SNAP" --host mitaller-db --target "$WORK" --include /var/opt/mssql/backup/chain --quiet
chown -R mssql:mssql "$WORK"
full="$(find "$WORK" -name 00-full.bak | head -1)"
[ -n "$full" ] || { echo "no hay respaldo completo en $SNAP" >&2; exit 1; }
chain="$(dirname "$full")"

move=""
while IFS='|' read -r logical type; do
  ext=mdf; [ "$type" = "L" ] && ext=ldf
  move+=", MOVE '$logical' TO '/var/opt/mssql/data/${TARGET}_${logical}.$ext'"
done < <("$SQLCMD" -S 127.0.0.1 -U sa -C -b -h -1 -W -s '|' -Q "SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK='$full'" </dev/null | cut -d'|' -f1,3)

sql "IF DB_ID('$TARGET') IS NOT NULL ALTER DATABASE [$TARGET] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
sql "RESTORE DATABASE [$TARGET] FROM DISK='$full' WITH REPLACE, NORECOVERY, CHECKSUM$move"
count=0
for log in $(find "$chain" -name '*-log.trn' | sort); do
  sql "RESTORE LOG [$TARGET] FROM DISK='$log' WITH NORECOVERY, CHECKSUM"
  count=$((count + 1))
done
sql "RESTORE DATABASE [$TARGET] WITH RECOVERY; ALTER DATABASE [$TARGET] SET MULTI_USER"
echo "Restaurado $SNAP en $TARGET: 1 completo + $count logs"
