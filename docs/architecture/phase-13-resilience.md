# Phase 13 - Resilience

OrderFlow includes lightweight resilience where service-to-service calls happen.

## Implemented Pieces

- Typed `HttpClient` in `OrderService`
- Three-second timeout per downstream service
- Standard resilience handler for retry/circuit-breaker-style behavior
- Gateway rate limiting
- OrderService rate limiting
- Health endpoints at `/health`
- Idempotent checkout using an idempotency key

## Failure Demonstrations

Stop `InventoryService`, then call checkout.

Expected result:

```text
503 Service Unavailable
```

Use mock payment mode `failed`.

Expected result:

```text
PaymentFailed order
Inventory released
Saga completed with compensation
```
