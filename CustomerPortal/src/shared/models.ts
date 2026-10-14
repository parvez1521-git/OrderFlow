/** Shared TypeScript representations of API response payloads.
 * Enterprise concept: shared contracts prevent UI components from inventing
 * different shapes for the same backend data. */
export type Product = { id: number; name: string; price: number }
export type Customer = { id: number; name: string; email: string }
export type Order = { id: number; customerId: number; status: string; total: number; items: { productId: number; productName: string; quantity: number; unitPrice: number }[] }
export type Cart = { customerId: number; total: number; items: { productId: number; productName: string; unitPrice: number; quantity: number; lineTotal: number }[] }
export type CheckoutResponse = { orderId: number; orderStatus: string; paymentStatus: string; total: number; message: string }
