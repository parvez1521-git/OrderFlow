/** Order feature component. Displays authenticated customer's order history. */
import type { Order } from '../../shared/models'
export function OrderHistory({ orders }: { orders: Order[] }) { return <div><h1 className="orders-title">My orders</h1>{orders.map(order => <article className="order" key={order.id}><b>Order #{order.id}</b><span>{order.status}</span><strong className="total">₹{order.total.toFixed(2)}</strong>{order.items.map(item => <p key={item.productName}>{item.productName} x {item.quantity} - ₹{item.unitPrice.toFixed(2)} each</p>)}</article>)}</div> }
