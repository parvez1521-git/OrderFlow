import { useState, useRef, useEffect, type FormEvent } from 'react'
import { fromService, request } from '../../shared/api'
import './AiAssistant.css'

interface Message {
  role: 'user' | 'assistant'
  content: string
}

interface AiAssistantProps {
  customerId?: number
  customerEmail?: string
  customerName?: string
}

interface ChatResponse {
  reply: string
}

export function AiAssistant({ customerId, customerEmail, customerName }: AiAssistantProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)
  const [messages, setMessages] = useState<Message[]>([
    {
      role: 'assistant',
      content: customerName
        ? `Hello ${customerName}! I'm your OrderFlow Assistant. Ask me about your order status, history, or product stock!`
        : `Hello! I'm your OrderFlow Assistant. Create or select a customer profile to track your orders, or ask me about our products!`
    }
  ])

  const messagesEndRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (isOpen) {
      messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
    }
  }, [messages, isOpen])

  async function sendMessage(textToSend?: string) {
    const text = (textToSend ?? input).trim()
    if (!text || loading) return

    const newMessages: Message[] = [...messages, { role: 'user', content: text }]
    setMessages(newMessages)
    setInput('')
    setLoading(true)

    try {
      const response = await request<ChatResponse>(fromService('ai', '/ai/chat'), {
        method: 'POST',
        body: JSON.stringify({
          message: text,
          customerId: customerId || null,
          customerEmail: customerEmail || null,
          history: newMessages.slice(-6)
        })
      })

      setMessages(prev => [...prev, { role: 'assistant', content: response.reply }])
    } catch (err) {
      setMessages(prev => [
        ...prev,
        {
          role: 'assistant',
          content: `Sorry, I couldn't reach the local AI service. Ensure AiService and Ollama are running.`
        }
      ])
    } finally {
      setLoading(false)
    }
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    void sendMessage()
  }

  return (
    <>
      <button
        className="ai-assistant-toggle"
        onClick={() => setIsOpen(!isOpen)}
        title="Open AI Order Assistant"
      >
        <span>🤖</span>
        <span>Order Assistant</span>
      </button>

      {isOpen && (
        <aside className="ai-assistant-window" aria-label="AI Order Assistant">
          <header className="ai-header">
            <div className="ai-header-title">
              <span className="ai-status-dot" />
              <div>
                <h3>OrderFlow Assistant</h3>
                <span>Powered by Ollama (llama3.2)</span>
              </div>
            </div>
            <button className="ai-close-btn" onClick={() => setIsOpen(false)}>
              ✕
            </button>
          </header>

          <div className="ai-messages">
            {messages.map((m, idx) => (
              <div key={idx} className={`ai-message ${m.role}`}>
                {m.content}
              </div>
            ))}
            {loading && <div className="ai-typing">Checking OrderFlow and thinking...</div>}
            <div ref={messagesEndRef} />
          </div>

          <div className="ai-quick-chips">
            <button
              className="ai-chip"
              onClick={() => void sendMessage("What is the status of my latest order?")}
            >
              Latest order status
            </button>
            <button
              className="ai-chip"
              onClick={() => void sendMessage("Show my order history")}
            >
              My orders
            </button>
            <button
              className="ai-chip"
              onClick={() => void sendMessage("What products are in catalog?")}
            >
              Catalog items
            </button>
          </div>

          <form className="ai-input-form" onSubmit={handleSubmit}>
            <input
              type="text"
              placeholder="Ask about orders, products..."
              value={input}
              onChange={e => setInput(e.target.value)}
              disabled={loading}
            />
            <button type="submit" disabled={loading || !input.trim()}>
              Send
            </button>
          </form>
        </aside>
      )}
    </>
  )
}
