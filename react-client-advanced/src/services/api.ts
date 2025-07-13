import axios, { AxiosError, AxiosResponse } from 'axios';
import {
  DatabaseConnectionRequest,
  DatabaseConnectionResponse,
  AIQueryRequest,
  AIQueryResponse,
  ChatRequest,
  ChatResponse,
  AvailableModels,
  AuditLog,
  PagedResult,
  SystemConfiguration,
  SystemConfigurationRequest,
  UserSession,
  CreateSessionRequest,
  UpdateSessionRequest,
  MCPConnectionStatus,
  MCPQueryRequest,
  MCPQueryResult,
  Metrics,
  AuditLogFilters,
} from '@/types/api';

const API_BASE_URL = '/api';

// Create axios instance with default config
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 second timeout
});

// Request interceptor for authentication and logging
apiClient.interceptors.request.use(
  (config: any) => {
    // Add session token if available
    const sessionToken = localStorage.getItem('sessionToken');
    if (sessionToken) {
      config.headers.Authorization = `Bearer ${sessionToken}`;
    }
    
    // Add request ID for tracking
    config.headers['X-Request-ID'] = crypto.randomUUID();
    
    return config;
  },
  (error: any) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - redirect to login or refresh token
      localStorage.removeItem('sessionToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// API functions
export const api = {
  // Database endpoints
  connectDatabase: async (request: DatabaseConnectionRequest): Promise<DatabaseConnectionResponse> => {
    const response = await apiClient.post<DatabaseConnectionResponse>('/database/connect', request);
    return response.data;
  },

  getSupportedDatabaseTypes: async (): Promise<string[]> => {
    const response = await apiClient.get<string[]>('/database/supported-types');
    return response.data;
  },

  // AI endpoints
  generateQuery: async (request: AIQueryRequest): Promise<AIQueryResponse> => {
    const response = await apiClient.post<AIQueryResponse>('/ai/query', request);
    return response.data;
  },

  chat: async (request: ChatRequest): Promise<ChatResponse> => {
    const response = await apiClient.post<ChatResponse>('/ai/chat', request);
    return response.data;
  },

  getAvailableModels: async (): Promise<AvailableModels> => {
    const response = await apiClient.get<AvailableModels>('/ai/models');
    return response.data;
  },

  // Enterprise endpoints
  getAuditLogs: async (filters: AuditLogFilters = {}): Promise<PagedResult<AuditLog>> => {
    const params = new URLSearchParams();
    if (filters.fromDate) params.append('fromDate', filters.fromDate.toISOString());
    if (filters.toDate) params.append('toDate', filters.toDate.toISOString());
    if (filters.userId) params.append('userId', filters.userId);
    if (filters.action) params.append('action', filters.action);
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

    const response = await apiClient.get<PagedResult<AuditLog>>(`/enterprise/audit-logs?${params}`);
    return response.data;
  },

  getMetrics: async (): Promise<Metrics> => {
    const response = await apiClient.get<Metrics>('/enterprise/metrics');
    return response.data;
  },

  getSystemConfigurations: async (category?: string): Promise<SystemConfiguration[]> => {
    const params = category ? `?category=${encodeURIComponent(category)}` : '';
    const response = await apiClient.get<SystemConfiguration[]>(`/enterprise/system-config${params}`);
    return response.data;
  },

  setSystemConfiguration: async (request: SystemConfigurationRequest): Promise<SystemConfiguration> => {
    const response = await apiClient.post<SystemConfiguration>('/enterprise/system-config', request);
    return response.data;
  },

  createSession: async (request: CreateSessionRequest): Promise<UserSession> => {
    const response = await apiClient.post<UserSession>('/enterprise/session', request);
    return response.data;
  },

  updateSession: async (sessionId: string, request: UpdateSessionRequest): Promise<void> => {
    await apiClient.put(`/enterprise/session/${sessionId}`, request);
  },

  validateSession: async (sessionId: string): Promise<boolean> => {
    const response = await apiClient.get<boolean>(`/enterprise/session/${sessionId}/validate`);
    return response.data;
  },

  // MCP endpoints
  getMCPStatus: async (): Promise<MCPConnectionStatus> => {
    const response = await apiClient.get<MCPConnectionStatus>('/enterprise/mcp/status');
    return response.data;
  },

  executeMCPQuery: async (request: MCPQueryRequest): Promise<MCPQueryResult> => {
    const response = await apiClient.post<MCPQueryResult>('/enterprise/mcp/query', request);
    return response.data;
  },
};

// Query keys for TanStack Query
export const queryKeys = {
  database: {
    all: ['database'] as const,
    connections: () => [...queryKeys.database.all, 'connections'] as const,
    supportedTypes: () => [...queryKeys.database.all, 'supported-types'] as const,
  },
  ai: {
    all: ['ai'] as const,
    models: () => [...queryKeys.ai.all, 'models'] as const,
    query: (request: AIQueryRequest) => [...queryKeys.ai.all, 'query', request] as const,
    chat: (request: ChatRequest) => [...queryKeys.ai.all, 'chat', request] as const,
  },
  enterprise: {
    all: ['enterprise'] as const,
    auditLogs: (filters: AuditLogFilters) => [...queryKeys.enterprise.all, 'audit-logs', filters] as const,
    metrics: () => [...queryKeys.enterprise.all, 'metrics'] as const,
    systemConfig: (category?: string) => [...queryKeys.enterprise.all, 'system-config', category] as const,
    mcpStatus: () => [...queryKeys.enterprise.all, 'mcp', 'status'] as const,
  },
  session: {
    all: ['session'] as const,
    validate: (sessionId: string) => [...queryKeys.session.all, 'validate', sessionId] as const,
  },
} as const;

// Error handling utilities
export class APIError extends Error {
  constructor(
    message: string,
    public status: number,
    public code?: string,
    public details?: any
  ) {
    super(message);
    this.name = 'APIError';
  }
}

export const handleAPIError = (error: any): APIError => {
  if (axios.isAxiosError(error)) {
    return new APIError(
      error.response?.data?.error || error.message,
      error.response?.status || 500,
      error.response?.data?.code,
      error.response?.data
    );
  }
  return new APIError(error.message || 'An unexpected error occurred', 500);
};