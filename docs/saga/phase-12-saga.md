# Phase 12 - Saga

The checkout workflow is now represented as a simple orchestration-based Saga in `OrderService`.

## Happy Path

```text
Create saga state
Read product price
Reserve inventory
Create order
Process payment
Confirm order
Notify customer
Complete saga
```

## Failure Path

```text
Create saga state
Reserve inventory
Payment fails
Release inventory
Mark order PaymentFailed or Cancelled
Complete saga
```

## Implemented Pieces

- Saga state entity: `CheckoutSagaState`
- Saga list endpoint: `GET /api/checkout/sagas`
- Idempotency key: duplicate checkout requests return the existing saga result
- Compensation: failed checkout releases inventory

## Learning Point

Microservices do not share one database transaction. A Saga coordinates multiple local transactions and uses compensating actions when something fails.
