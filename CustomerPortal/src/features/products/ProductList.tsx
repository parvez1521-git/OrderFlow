/** Product feature component. Renders catalog data and captures quantities;
 * it does not know how the parent submits an order. */
import type { Product } from '../../shared/models'

type Props = { products: Product[]; quantities: Record<number, number>; onQuantityChange: (id: number, quantity: number) => void; onAddToCart: (productId: number) => void }
export function ProductList({ products, quantities, onQuantityChange, onAddToCart }: Props) {
  return <div className="panel"><h1>Products</h1>{products.length ? products.map(product => <article className="product" key={product.id}><div><h2>{product.name}</h2><b>₹{product.price.toFixed(2)}</b></div><label>Quantity <input type="number" min="1" value={quantities[product.id] ?? 1} onChange={event => onQuantityChange(product.id, Number(event.target.value))} /></label><button onClick={() => onAddToCart(product.id)}>Add to cart</button></article>) : <p>No products yet. Ask an administrator to add one.</p>}</div>
}
