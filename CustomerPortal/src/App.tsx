/** Customer portal root component.
 * Phase 10 customer journey: direct microservice calls plus checkout through
 * OrderService's synchronous service-to-service workflow.
 */
import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'
import { fromService, request } from './shared/api'
import type { Cart, CheckoutResponse, Customer, Order, Product } from './shared/models'
import { ProductList } from './features/products/ProductList'
import { OrderHistory } from './features/orders/OrderHistory'
import { AiAssistant } from './features/ai/AiAssistant'

export default function App() {
  const [products, setProducts] = useState<Product[]>([])
  const [customer, setCustomer] = useState<Customer | null>(null)
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [quantities, setQuantities] = useState<Record<number, number>>({})
  const [orders, setOrders] = useState<Order[]>([])
  const [cart, setCart] = useState<Cart>({ customerId: 0, total: 0, items: [] })
  const [paymentMode, setPaymentMode] = useState('success')
  const [message, setMessage] = useState('')
  const loadProducts = () => request<Product[]>(fromService('products', '/products')).then(setProducts).catch(error => setMessage(error.message))
  const loadOrders = () => request<Order[]>(fromService('orders', '/orders')).then(result => setOrders(customer ? result.filter(order => order.customerId === customer.id) : [])).catch(error => setMessage(error.message))
  useEffect(() => { void loadProducts() }, [])
  useEffect(() => { if (customer) void loadOrders() }, [customer])

  async function createCustomer(event: FormEvent) {
    event.preventDefault()
    try { const created = await request<Customer>(fromService('customers', '/customers'), { method: 'POST', body: JSON.stringify({ name, email }) }); setCustomer(created); setCart({ customerId: created.id, total: 0, items: [] }); setMessage(`Customer ${created.name} created in CustomerService.`) }
    catch (error) { setMessage((error as Error).message) }
  }
  async function addToCart(productId: number) {
    const product = products.find(item => item.id === productId)
    if (!product || !customer) return setMessage('Create a customer first.')
    const quantity = quantities[productId] ?? 1
    const existing = cart.items.find(item => item.productId === productId)
    const items = existing ? cart.items.map(item => item.productId === productId ? { ...item, quantity: item.quantity + quantity, lineTotal: (item.quantity + quantity) * item.unitPrice } : item) : [...cart.items, { productId, productName: product.name, unitPrice: product.price, quantity, lineTotal: product.price * quantity }]
    setCart({ customerId: customer.id, total: items.reduce((sum, item) => sum + item.lineTotal, 0), items })
    setMessage('Cart updated locally. Checkout will call OrderService.')
  }
  async function checkout() {
    if (!customer) return setMessage('Create a customer first.')
    if (!cart.items.length) return setMessage('Add at least one item to the cart.')
    try { const result = await request<CheckoutResponse>(fromService('orders', '/checkout'), { method: 'POST', body: JSON.stringify({ customerId: customer.id, customerEmail: customer.email, paymentMode, idempotencyKey: crypto.randomUUID(), items: cart.items.map(item => ({ productId: item.productId, quantity: item.quantity })) }) }); setCart({ customerId: customer.id, total: 0, items: [] }); await loadOrders(); setMessage(`Payment ${result.paymentStatus}. Order #${result.orderId} is ${result.orderStatus}.`) }
    catch (error) { setMessage((error as Error).message) }
  }
  function clearCart() { setCart({ customerId: customer?.id ?? 0, total: 0, items: [] }); setMessage('Cart cleared locally.') }

  return <main><header><div><strong>OrderFlow</strong><span>Customer portal - Phase 10 microservices</span></div><button onClick={() => { void loadProducts(); if (customer) void loadOrders() }}>Refresh</button></header><p className="notice">{message || 'Create a customer, add products to the local cart, then checkout through OrderService.'}</p><section className="layout"><ProductList products={products} quantities={quantities} onQuantityChange={(id, quantity) => setQuantities({ ...quantities, [id]: quantity })} onAddToCart={addToCart} /><div className="panel"><form onSubmit={createCustomer}><h1>Customer</h1><label>Name<input value={customer?.name ?? name} onChange={event => setName(event.target.value)} /></label><label>Email<input value={customer?.email ?? email} onChange={event => setEmail(event.target.value)} /></label><button className="primary">Create customer</button></form><h1 className="orders-title">Cart</h1>{cart.items.length ? cart.items.map(item => <article className="order" key={item.productId}><b>{item.productName}</b><span>x {item.quantity}</span><strong className="total">₹{item.lineTotal.toFixed(2)}</strong></article>) : <p>Your cart is empty.</p>}<label>Mock payment<input value={paymentMode} onChange={event => setPaymentMode(event.target.value)} /></label><div className="cart-actions"><button className="primary" onClick={checkout}>Checkout ₹{cart.total.toFixed(2)}</button><button onClick={clearCart} disabled={!cart.items.length}>Clear cart</button></div><OrderHistory orders={orders} /></div></section><AiAssistant customerId={customer?.id} customerEmail={customer?.email} customerName={customer?.name} /></main>
}
