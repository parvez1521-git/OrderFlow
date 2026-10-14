# Starts the full OrderFlow stack locally using Docker Compose.
# Run from PowerShell after Docker Desktop is running:
#   .\scripts\docker-up.ps1
$ErrorActionPreference = 'Stop'

$orderflowRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $orderflowRoot

docker compose up --build
