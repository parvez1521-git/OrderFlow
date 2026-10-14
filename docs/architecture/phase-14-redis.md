# Phase 14 - Redis

Redis caching is implemented for the initial use case from the specification:

```text
GET /products
  -> Redis
  -> SQL Server
```

## Implemented Pieces

- Package: `StackExchange.Redis`
- Service: `ProductService`
- Cache key: `products:list`
- Expiration: two minutes
- Invalidation: create, update, and delete product clear the cache
- Fallback: if Redis is unavailable, ProductService reads from SQL Server

## Learning Point

Cache-aside means the application checks the cache first. If the cache misses, it reads from the database and stores the result in cache.
