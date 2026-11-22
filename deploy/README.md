# Servidor de producción (Contabo VPS 6)

`195.26.247.240`, Ubuntu 24.04, hora America/Mexico_City (la API usa `DateTime.Now`, no cambiarla).
Acceso: `ssh -i ~/.ssh/id_mitaller_vps root@195.26.247.240` (solo llave; contraseña de root solo para la consola VNC de Contabo).

| Pieza | Dónde |
|---|---|
| SQL Server 2025 Express | solo `127.0.0.1:1433`; `sa` en `/etc/mitaller/sa.secret`, la API usa `mitaller_app` |
| API (.NET 8) | `/opt/mitaller/api`, servicio `mitaller-api`, `127.0.0.1:5000`, secretos en `/etc/mitaller/api.env` |
| Caddy | `/etc/caddy/Caddyfile`; los dominios están en `mitaller-sites.caddy` (descomentar el `import` cuando el DNS apunte aquí) |
| Web Talleres | `/var/www/plataforma` |
| Firewall | ufw: 22, 80, 443. fail2ban en sshd |

## Actualizar la API

```bash
dotnet publish MiTaller.csproj -c Release -r linux-x64 --self-contained false -o /tmp/api
rm /tmp/api/appsettings.Development.json
dotnet ef migrations bundle -r linux-x64 --self-contained -o /tmp/efbundle --force
# en el servidor: snapshot de Contabo, mitaller-backup.sh full, ./efbundle --connection "<ConnectionStrings__DefaultConnection>", copiar a /opt/mitaller/api, systemctl restart mitaller-api
```

## Respaldos

- `mitaller-backup.sh full` cada noche 03:30, `log` cada 15 min, `verify` domingo 05:00 (restaura en `M_T_DB_verify`, CHECKDB, borra).
- Cada snapshot de restic trae la cadena completa (full + logs), así que cualquiera se restaura sola: `mitaller-restore.sh latest M_T_DB_restore`.
- Configuración en `/etc/mitaller/backup.env` (ver `backup/backup.env.example`). Contraseña de cifrado: `/etc/mitaller/restic.pass` — guardarla fuera del servidor o los respaldos no sirven.
- Mientras `RESTIC_REPOSITORY` sea local, los respaldos NO sobreviven a la pérdida del servidor: pasar a Cloudflare R2.

## Pendiente para el lanzamiento

La base actual es el respaldo de prod del 2026-08-26 (pruebas). El día del lanzamiento: respaldo fresco de la base vieja si sigue viva, restaurar, migrar en dos pasos (`20261106050229_AccidentOwnerAlert`, verificar inspecciones con `Backups/migracion-inspecciones`, luego el resto), `seed-admin`, SMTP en `api.env`.
