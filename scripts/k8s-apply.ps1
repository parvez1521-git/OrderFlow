# Applies the local Kubernetes manifests and starts port-forward windows.
# Requires Docker Desktop Kubernetes, Minikube, or Kind plus kubectl on PATH.
$ErrorActionPreference = 'Stop'

$orderflowRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $orderflowRoot

kubectl apply -f .\k8s\

Write-Host ''
Write-Host 'Waiting for OrderFlow pods. SQL Server can take a minute on first start...'
kubectl wait --namespace orderflow-local --for=condition=Available deployment/sqlserver --timeout=180s
kubectl wait --namespace orderflow-local --for=condition=Available deployment/redis --timeout=120s
kubectl wait --namespace orderflow-local --for=condition=Available deployment/rabbitmq --timeout=120s
kubectl wait --namespace orderflow-local --for=condition=Available deployment/gateway --timeout=240s
kubectl wait --namespace orderflow-local --for=condition=Available deployment/customer-portal --timeout=180s
kubectl wait --namespace orderflow-local --for=condition=Available deployment/admin-portal --timeout=180s

Write-Host ''
Write-Host 'Starting local port-forwards in separate PowerShell windows...'
Start-Process powershell -ArgumentList '-NoExit', '-Command', 'kubectl -n orderflow-local port-forward service/gateway 6200:8080' -WindowStyle Normal
Start-Process powershell -ArgumentList '-NoExit', '-Command', 'kubectl -n orderflow-local port-forward service/auth-api 5078:8080' -WindowStyle Normal
Start-Process powershell -ArgumentList '-NoExit', '-Command', 'kubectl -n orderflow-local port-forward service/customer-portal 5173:80' -WindowStyle Normal
Start-Process powershell -ArgumentList '-NoExit', '-Command', 'kubectl -n orderflow-local port-forward service/admin-portal 4200:80' -WindowStyle Normal

Write-Host ''
Write-Host 'Open after port-forward starts:'
Write-Host '  Customer Portal: http://localhost:5173'
Write-Host '  Admin Portal:    http://localhost:4200'
Write-Host '  Gateway:         http://localhost:6200'
Write-Host '  Auth API:        http://localhost:5078'
