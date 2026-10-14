/** Shared API contract types for the admin portal. Keeping these outside the
 * root component is the first step toward feature-specific Angular models. */
export type Customer = { id: number; name: string; email: string };
export type Product = { id: number; name: string; price: number };
export type Order = { id: number; customerId: number; orderDate: string; status: string; total: number; items: { productId: number; productName: string; quantity: number; unitPrice: number }[] };
export type Inventory = { id: number; productId: number; productName?: string; availableQuantity: number };
export type AuthResponse = { token: string; expiresAtUtc: string; userId: string; email: string; customerId?: number; roles: string[] };
