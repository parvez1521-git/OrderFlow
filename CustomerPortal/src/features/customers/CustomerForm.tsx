/** Customer feature component. Owns the registration form presentation and
 * delegates persistence to the parent/application API layer. */
import type { FormEvent } from 'react'
type Props = { customerName: string; email: string; customerExists: boolean; onNameChange: (value: string) => void; onEmailChange: (value: string) => void; onSubmit: (event: FormEvent) => void }
export function CustomerForm({ customerName, email, customerExists, onNameChange, onEmailChange, onSubmit }: Props) {
  return <div><h1>Your details</h1>{customerExists ? <div><p><b>{customerName}</b><br />{email}</p><p className="success">You can now create orders.</p></div> : <form onSubmit={onSubmit}><label>Name<input required value={customerName} onChange={event => onNameChange(event.target.value)} /></label><label>Email<input required type="email" value={email} onChange={event => onEmailChange(event.target.value)} /></label><button className="primary">Create customer</button></form>}</div>
}
