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

// Enterprise-grade types
export interface AuditLog {
  id: string;
  timestamp: Date;
  userId?: string;
  userName?: string;
  action: string;
  resourceType: string;
  resourceId?: string;
  details?: string;
  ipAddress?: string;
  userAgent?: string;
}

export interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface SystemConfiguration {
  id: string;
  key: string;
  value: string;
  category?: string;
  description?: string;
  isEncrypted: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface SystemConfigurationRequest {
  key: string;
  value: string;
  category?: string;
  description?: string;
  isEncrypted?: boolean;
  userId?: string;
  userName?: string;
}

export interface UserSession {
  sessionId: string;
  userId?: string;
  userName?: string;
  ipAddress?: string;
  userAgent?: string;
  createdAt: Date;
  lastActivityAt: Date;
  isActive: boolean;
}

export interface CreateSessionRequest {
  sessionId: string;
  userId?: string;
  userName?: string;
  ipAddress?: string;
  userAgent?: string;
}

export interface UpdateSessionRequest {
  userId?: string;
  userName?: string;
}

export interface MCPConnectionStatus {
  isConnected: boolean;
  lastConnectedAt?: Date;
  errorMessage?: string;
  serverInfo?: {
    name: string;
    version: string;
    capabilities: string[];
  };
}

export interface MCPQueryRequest {
  prompt: string;
  aiModel: string;
  aiPlatform: string;
  sessionId?: string;
  userId?: string;
}

export interface MCPQueryResult {
  response: string;
  query?: string;
  results?: Record<string, any>[];
  metadata?: {
    tokensUsed: number;
    processingTime: number;
    modelUsed: string;
  };
  errorMessage?: string;
}

export interface Metrics {
  mcpMetrics: {
    totalQueries: number;
    successfulQueries: number;
    averageResponseTime: number;
    lastQueryAt?: Date;
  };
  connectionStatus: MCPConnectionStatus;
  connections: {
    total: number;
    active: number;
  };
  weeklyStats: Array<{
    date: string;
    count: number;
    successful: number;
  }>;
}

export interface AuditLogFilters {
  fromDate?: Date;
  toDate?: Date;
  userId?: string;
  action?: string;
  page?: number;
  pageSize?: number;
}