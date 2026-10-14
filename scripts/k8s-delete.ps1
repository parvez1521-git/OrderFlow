# Removes the local Kubernetes OrderFlow namespace and all resources in it.
# This deletes the local SQL Server persistent volume claim created by k8s.
$ErrorActionPreference = 'Stop'

kubectl delete namespace orderflow-local
