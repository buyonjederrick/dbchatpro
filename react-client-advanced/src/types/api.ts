export interface DatabaseConnectionRequest {
  name: string;
  databaseType: string;
  connectionString: string;
}

export interface DatabaseConnectionResponse {
  name: string;
  databaseType: string;
  isConnected: boolean;
  errorMessage?: string;
  schema?: DatabaseSchema;
}

export interface DatabaseSchema {
  schemaRaw: string[];
  tables: TableSchema[];
}

export interface TableSchema {
  tableName: string;
  columns: string[];
}

export interface AIQueryRequest {
  prompt: string;
  aiModel: string;
  aiService: string;
  databaseType: string;
  connectionString: string;
}

export interface AIQueryResponse {
  summary: string;
  query: string;
  results?: Record<string, any>[];
  errorMessage?: string;
}

export interface ChatRequest {
  messages: ChatMessage[];
  aiModel: string;
  aiService: string;
}

export interface ChatResponse {
  response: string;
  errorMessage?: string;
}

export interface ChatMessage {
  role: string;
  content: string;
}

export interface AvailableModels {
  [service: string]: string[];
}

export interface QueryHistory {
  id: string;
  timestamp: Date;
  prompt: string;
  query: string;
  summary: string;
  databaseName: string;
  aiModel: string;
  aiService: string;
  results?: Record<string, any>[];
}

export interface ConnectionStore {
  connections: DatabaseConnectionResponse[];
  selectedConnection: DatabaseConnectionResponse | null;
  addConnection: (connection: DatabaseConnectionResponse) => void;
  removeConnection: (name: string) => void;
  selectConnection: (connection: DatabaseConnectionResponse) => void;
  clearConnections: () => void;
}