# Phase 19 — Local Kubernetes for OrderFlow

This phase adds Kubernetes manifests for local learning only. The manifests are intended for Docker Desktop Kubernetes, Minikube, or Kind on your own computer. Nothing is deployed to an online server.

## What Kubernetes gives us here

Kubernetes introduces the production-style concepts used in enterprise microservices:

- Namespace: a logical area for all OrderFlow resources.
- Deployment: tells Kubernetes how to run and restart app containers.
- Pod: the actual running container instance.
- Service: stable internal network name for a pod group.
- ConfigMap: non-secret configuration.
- Secret: sensitive configuration such as passwords.
- PersistentVolumeClaim: local persistent storage for SQL Server.

## Local Kubernetes files

- `k8s/00-namespace.yaml`
  - Creates the `orderflow-local` namespace.

- `k8s/01-config.yaml`
  - Creates shared local config and the local SQL password secret.

- `k8s/02-infrastructure.yaml`
  - Runs SQL Server, Redis, and RabbitMQ.

- `k8s/03-backend.yaml`
  - Runs Auth API, microservices, and Gateway.

- `k8s/04-portals.yaml`
  - Runs React Customer Portal and Angular Admin Portal.

## How to run locally

First build local Docker images:

```powershell
cd C:\Training2026\OrderFlow
.\scripts\k8s-build-images.ps1
```

Then apply Kubernetes manifests:

```powershell
.\scripts\k8s-apply.ps1
```

The script applies the manifests and opens local port-forward windows.

## Local URLs after port-forwarding

- Customer Portal: `http://localhost:5173`
- Admin Portal: `http://localhost:4200`
- Gateway: `http://localhost:6200`
- Auth API: `http://localhost:5078`

## Useful Kubernetes commands

See pods:

```powershell
kubectl get pods -n orderflow-local
```

See services:

```powershell
kubectl get services -n orderflow-local
```

Watch pods starting:

```powershell
kubectl get pods -n orderflow-local -w
```

Check logs for one app:

```powershell
kubectl logs -n orderflow-local deployment/order-service
```

Delete the local Kubernetes environment:

```powershell
.\scripts\k8s-delete.ps1
```

## Beginner mental model

Docker Compose is like saying:

> Run all these containers together on my laptop.

Kubernetes is like saying:

> Keep these application parts running, give them stable names, restart them if they crash, and let me scale or replace them later.

In this project, we are using Kubernetes locally so you can learn the concepts safely before touching any cloud environment.
