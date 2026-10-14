# Stops the local Docker Compose stack but keeps SQL Server data volume.
# Use -RemoveData when you intentionally want a fresh local database.
param(
    [switch]$RemoveData
)

$ErrorActionPreference = 'Stop'

$orderflowRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $orderflowRoot

if ($RemoveData) {
    docker compose down -v
} else {
    docker compose down
}
