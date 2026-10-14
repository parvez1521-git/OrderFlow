# Builds all OrderFlow images locally for Docker Desktop Kubernetes, Minikube,
# or Kind. No image is pushed to any online registry.
$ErrorActionPreference = 'Stop'

$orderflowRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $orderflowRoot

docker compose build

Write-Host ''
Write-Host 'Local images built:'
docker images 'orderflow/*:local'
Write-Host ''
Write-Host 'If you use Kind instead of Docker Desktop Kubernetes, load the images manually, for example:'
Write-Host '  kind load docker-image orderflow/gateway:local'
