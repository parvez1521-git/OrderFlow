/**
 * Angular admin root component.
 * It manages customer, product, inventory, and order workflows through the API.
 * Enterprise Angular apps usually split this into feature components, services,
 * typed models, and route-level pages; Phase 2 keeps it together for learning.
 */
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { Customer, Inventory, Order, Product } from './shared/models';
import { CustomerApiService } from './features/customers/customer-api.service';
import { ProductApiService } from './features/products/product-api.service';
import { OrderApiService } from './features/orders/order-api.service';
import { InventoryApiService } from './features/inventory/inventory-api.service';
import { AuthApiService } from './features/auth/auth-api.service';
import { KnowledgeApiService, KnowledgeDoc } from './features/knowledge/knowledge-api.service';

@Component({ selector: 'app-root', imports: [FormsModule], templateUrl: './app.html', styleUrl: './app.css' })
export class App implements OnInit {
  private readonly customerApi = inject(CustomerApiService); private readonly productApi = inject(ProductApiService); private readonly orderApi = inject(OrderApiService); private readonly inventoryApi = inject(InventoryApiService);
  private readonly authApi = inject(AuthApiService);
  private readonly knowledgeApi = inject(KnowledgeApiService);
  active = 'products'; message = '';
  login = { email: 'admin@oms.local', password: 'ChangeMe123!' };
  isLoggedIn = false;
  customers: Customer[] = []; products: Product[] = []; orders: Order[] = []; inventory: Inventory[] = [];
  policies: KnowledgeDoc[] = [];
  policyForm = { id: '', title: '', category: 'General', content: '' };
  customer: Customer = { id: 0, name: '', email: '' };
  product: Product = { id: 0, name: '', price: 0 };
  stock = { productId: 0, availableQuantity: 10 };
  order = { id: 0, customerId: 0, productId: 0, quantity: 1 };

  ngOnInit() {
    this.isLoggedIn = !!this.authApi.token;
    if (this.isLoggedIn) {
      setTimeout(() => this.reload(), 0);
    }
  }
  signIn() {
    this.authApi.login(this.login.email, this.login.password).subscribe({
      next: response => { this.authApi.store(response.token); this.isLoggedIn = true; this.message = 'Logged in successfully.'; this.reload(); },
      error: e => this.fail(e)
    });
  }
  logout() { this.authApi.clear(); this.isLoggedIn = false; this.message = 'Logged out.'; }
  reload() {
    if (!this.isLoggedIn) return;
    this.customerApi.list().subscribe({ next: x => this.customers = x, error: e => this.fail(e, 'Customers') });
    this.productApi.list().subscribe({ next: x => { this.products = x; this.message = `Loaded ${x.length} product(s).`; }, error: e => this.fail(e, 'Products') });
    this.orderApi.list().subscribe({ next: x => this.orders = x, error: e => this.fail(e, 'Orders') });
    this.inventoryApi.list().subscribe({ next: x => this.inventory = x, error: e => this.fail(e, 'Inventory') });
    this.knowledgeApi.list().subscribe({ next: x => this.policies = x, error: e => this.fail(e, 'Knowledge & Policies') });
  }
  saveCustomer() { this.customerApi.save(this.customer.id, { name: this.customer.name, email: this.customer.email }).subscribe({ next: () => { this.message = 'Customer saved.'; this.customer = { id: 0, name: '', email: '' }; this.reload(); }, error: e => this.fail(e) }); }
  saveProduct() {
    const isNewProduct = !this.product.id;
    this.productApi.save(this.product.id, { name: this.product.name, price: this.product.price }).subscribe({
      next: savedProduct => {
        if (!isNewProduct) {
          this.message = 'Product saved.';
          this.product = { id: 0, name: '', price: 0 };
          this.reload();
          return;
        }

        this.inventoryApi.save({ productId: savedProduct.id, availableQuantity: 10 }).subscribe({
          next: () => {
            this.message = 'Product saved with default inventory of 10.';
            this.product = { id: 0, name: '', price: 0 };
            this.reload();
          },
          error: e => this.fail(e, 'Default inventory')
        });
      },
      error: e => this.fail(e)
    });
  }
  saveInventory() { this.inventoryApi.save({ productId: Number(this.stock.productId), availableQuantity: Number(this.stock.availableQuantity) }).subscribe({ next: () => { this.message = 'Inventory saved.'; this.stock = { productId: 0, availableQuantity: 10 }; this.reload(); }, error: e => this.fail(e) }); }
  saveOrder() { const product = this.products.find(item => item.id === Number(this.order.productId)); const body = { customerId: Number(this.order.customerId), items: [{ productId: Number(this.order.productId), productName: product?.name ?? `Product ${this.order.productId}`, quantity: Number(this.order.quantity), unitPrice: product?.price ?? 0 }] }; this.orderApi.save(this.order.id, body).subscribe({ next: () => { this.message = 'Order saved in OrderService.'; this.order = { id: 0, customerId: 0, productId: 0, quantity: 1 }; this.reload(); }, error: e => this.fail(e) }); }
  editCustomer(item: Customer) { this.customer = { ...item }; this.active = 'customers'; }
  editProduct(item: Product) { this.product = { ...item }; this.active = 'products'; }
  editOrder(item: Order) { const first = item.items[0]; this.order = { id: item.id, customerId: item.customerId, productId: first?.productId ?? 0, quantity: first?.quantity ?? 1 }; this.active = 'orders'; }
  productName(productId: number) { return this.products.find(product => product.id === productId)?.name ?? `Product ${productId}`; }
  delete(resource: string, id: number) { if (!confirm('Delete this record?')) return; const call = resource === 'customers' ? this.customerApi.delete(id) : resource === 'products' ? this.productApi.delete(id) : this.orderApi.delete(id); call.subscribe({ next: () => { this.message = 'Record deleted.'; this.reload(); }, error: e => this.fail(e) }); }
  savePolicy() {
    if (!this.policyForm.title.trim() || !this.policyForm.content.trim()) return;
    const isEdit = !!this.policyForm.id;
    const call = isEdit
      ? this.knowledgeApi.update(this.policyForm.id, { title: this.policyForm.title, category: this.policyForm.category, content: this.policyForm.content })
      : this.knowledgeApi.create({ title: this.policyForm.title, category: this.policyForm.category, content: this.policyForm.content });
    call.subscribe({
      next: () => {
        this.message = isEdit ? 'Policy document updated.' : 'Knowledge policy document saved.';
        this.policyForm = { id: '', title: '', category: 'General', content: '' };
        this.reload();
      },
      error: e => this.fail(e, 'KnowledgeService')
    });
  }
  editPolicy(item: KnowledgeDoc) {
    this.policyForm = { id: item.id, title: item.title, category: item.category, content: item.content };
    this.active = 'policies';
  }
  cancelPolicyEdit() {
    this.policyForm = { id: '', title: '', category: 'General', content: '' };
  }
  deletePolicy(id: string) {
    if (!confirm('Delete this knowledge policy?')) return;
    this.knowledgeApi.delete(id).subscribe({
      next: () => {
        this.message = 'Policy document deleted.';
        this.reload();
      },
      error: e => this.fail(e, 'KnowledgeService')
    });
  }
  private fail(error: { error?: { title?: string } }, area = 'Request') { this.message = `${area} failed. ${error.error?.title ?? 'Ensure the related microservice is running.'}`; }
}
