/** Small HTTP client for the customer portal.
 * Phase 15 calls microservices through the YARP gateway.
 */
export const serviceUrls = {
  customers: import.meta.env.VITE_CUSTOMER_SERVICE_URL ?? 'http://localhost:6200/api',
  products: import.meta.env.VITE_PRODUCT_SERVICE_URL ?? 'http://localhost:6200/api',
  orders: import.meta.env.VITE_ORDER_SERVICE_URL ?? 'http://localhost:6200/api',
  ai: import.meta.env.VITE_AI_SERVICE_URL ?? 'http://localhost:6200/api'
}

const gatewayAuthHeader = `Basic ${btoa(`${import.meta.env.VITE_GATEWAY_BASIC_USERNAME ?? 'orderflow-admin'}:${import.meta.env.VITE_GATEWAY_BASIC_PASSWORD ?? 'orderflow-dev-password'}`)}`

export async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...options,
    headers: { 'Content-Type': 'application/json', Authorization: gatewayAuthHeader, ...options?.headers }
  })
  if (!response.ok) throw new Error((await response.json().catch(() => null))?.title ?? 'Request failed.')
  return response.status === 204 ? undefined as T : response.json() as Promise<T>
}

export function fromService(service: keyof typeof serviceUrls, path: string) {
  return `${serviceUrls[service]}${path}`
}
