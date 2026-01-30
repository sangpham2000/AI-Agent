import * as signalR from '@microsoft/signalr'

type EventCallback = (...args: any[]) => void

export interface AgentConfig {
  apiKey: string
  apiUrl?: string
  theme?: 'light' | 'dark' | 'auto'
  containerId?: string
}

export class AgentSDK {
  private connection: signalR.HubConnection | null = null
  private listeners: Map<string, EventCallback[]> = new Map()
  private config: AgentConfig = { apiKey: '' }
  private _isOpen = false

  constructor() {}

  public init(config: AgentConfig) {
    this.config = { ...this.config, ...config }
    const url = this.config.apiUrl || 'http://localhost:5088/chatHub'

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => this.config.apiKey || '',
      })
      .withAutomaticReconnect()
      .build()

    this.connection.on('ReceiveMessage', (user: string, message: string) => {
      this.emit('message', { user, message })
    })

    this.connection
      .start()
      .then(() => {
        console.log('AgentSDK: Connected to ' + url)
        this.emit('connected')
      })
      .catch((err) => {
        console.error('AgentSDK: Connection Error', err)
        this.emit('error', err)
      })
  }

  public open() {
    this._isOpen = true
    this.emit('open')
  }

  public close() {
    this._isOpen = false
    this.emit('close')
  }

  public toggle() {
    if (this._isOpen) this.close()
    else this.open()
  }

  public async sendMessage(message: string) {
    if (!this.connection) return
    try {
      await this.connection.invoke('SendMessage', 'User', message)
    } catch (err) {
      console.error('AgentSDK: Send Error', err)
      this.emit('error', err)
    }
  }

  public on(event: string, callback: EventCallback) {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, [])
    }
    this.listeners.get(event)?.push(callback)
  }

  public off(event: string, callback: EventCallback) {
    if (!this.listeners.has(event)) return
    const callbacks = this.listeners.get(event)
    if (callbacks) {
      this.listeners.set(
        event,
        callbacks.filter((cb) => cb !== callback),
      )
    }
  }

  private emit(event: string, ...args: any[]) {
    this.listeners.get(event)?.forEach((cb) => cb(...args))
  }
}

// Singleton instance
export const sdk = new AgentSDK()
